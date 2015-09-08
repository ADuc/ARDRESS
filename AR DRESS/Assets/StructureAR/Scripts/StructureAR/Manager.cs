/*
  This file is part of the Structure SDK.
  Copyright © 2015 Occipital, Inc. All rights reserved.
  http://structure.io
*/

using System;
using System.Collections;
using UnityEngine;

namespace StructureAR
{
    /// <summary>
    /// Structure manager. This is the base class from which you can base your
    /// game ontop of to listen to events from the Structure Plug-in.
    /// </summary>
    public class Manager : MonoBehaviour
    {
        public static Manager _structureManager;
        public static Camera _MainCamera;
        public static GameObject _ScanObject;
        public Color ScanMeshWireColor = Color.white;
        public bool FreezePOV;

        #region Vars
            #region GAMEOBJECT_FIELDS
        public Camera MainCamera;
        private CameraViewScript cameraView;
        public bool ShowDebugLog;
            #endregion

            #region GAMESTATE_PROPERTY
        protected SensorState _gameState;
        public virtual SensorState gameState
        {
            get
            {
                return this._gameState;
            }
            set
            {
                this._gameState = value;
                if (StructureARGameEvent != null)
                {
                    //fire off any game changing events to the buttons.
                    StructureARGameEvent(this, new GameEventArgs(value, trackingIsGood));
                }

				//hide the occluding ground plane unless the game is running
				GameObject groundPlane = GameObject.Find ("GroundPlane");
				if (groundPlane)
				{
					switch(gameState)
					{
					case SensorState.DeviceReady: //the pre-scan phase
						groundPlane.GetComponent<MeshRenderer>().enabled = false;
						break;
					default:
						groundPlane.GetComponent<MeshRenderer>().enabled = true;
						break;
					}
				}
            }
        }

        /// <summary>
        /// The is tracking.
        /// true means the camera has some idea about wher it is in the world.
        /// false means the plug-in doesn't know how to update the in-game
        /// camera to match the scanned information.
        /// </summary>
        protected bool trackingIsGood;

        public delegate void StructureGameEventHandler(object sender,GameEventArgs args);
        //assign your event listeners to this event to get structure updates.
        public static event StructureGameEventHandler StructureARGameEvent;

            #endregion

            #region STRUCTURE_PLUGIN
        /// <summary>
        /// store the camera so its position and rotation can be updated.
        /// </summary>
        /// <value>The structure POV.</value>
        protected POV structurePOV{ get; set; }

        /// <summary>
        /// store the mesh loader to control the mesh that's loaded into the
        /// scanned object.
        /// </summary>
        /// <value>The structure object loader.</value>
        protected ObjectLoader structureObjectLoader{ get; set; }

        /// <summary>
        /// store the scanned data into a game object.
        /// </summary>
        /// <value>The scan object.</value>
        protected GameObject scanObject{ get; set; }

            #endregion
        #endregion

        #region UNITY_METHODS
        public virtual void Start()
        {
            Manager._structureManager = this;
            if (this.MainCamera != null)
            {
                Manager._MainCamera = this.MainCamera;
            }

            //check if we need to make a GameLog.
            if (this.ShowDebugLog && this.GetComponent<GameLog>() == null)
            {
                this.gameObject.AddComponent<GameLog>();
                GameLog.ShowGameLog = this.ShowDebugLog;
            }

            //tell the game to run at 30 fps rather than at max.
            //otherwise the game might be running faster
            //than the camera.
            Application.targetFrameRate = 30;

            //check Main Camera for StructurePOV
            if (!FreezePOV)
            {
                POV sPOV = this.MainCamera.GetComponent<POV>();
                if (sPOV == null)
                {
                    //assign one if it doesn't already have one.
                    this.structurePOV = this.MainCamera.gameObject.AddComponent<POV>();
                } else
                {
                    //get a reference to the component if it does have one.
                    this.structurePOV = sPOV;
                }
            }

            //initialize this game manager to recieve events
            //from the plug-in. Anything else you want to have listen to
            //the plugin can be done through here.
            StructureARPlugin.StructureEvent += this.HandleStructureEvent;

			StructureARPlugin.setCallbacks();

            //object loader to build the mesh in the game world.
            this.structureObjectLoader = new ObjectLoader();
            
            GameLog.Log(this.ToString() + this.structureObjectLoader);
            
            
            //provide the scanned mesh a place to live in the game world.
            this.scanObject = new GameObject("ScanObject");
            Manager._ScanObject = this.scanObject;
            Wireframe wf = this.scanObject.AddComponent<Wireframe>();
            wf.lineColor = this.ScanMeshWireColor;

            GameLog.Log(this.ToString() + this.scanObject);

            //setup the POV camera in game to match the projection
            //of the rear camera of the device
            StructureARPlugin.initStructureAR(this.MainCamera.projectionMatrix);
                        
            //Make color camera back plane
            this.MainCamera.clearFlags = CameraClearFlags.Depth;
            this.cameraView = new CameraViewScript(this.MainCamera, 1f);
            this.cameraView.CameraObject.transform.parent = this.MainCamera.transform;

            //should also set the camera FOV to 45
            this.MainCamera.fieldOfView = 45.0f;

            //do minor setup here, hide things, change UI etc.
            if (this.gameState == SensorState.CameraAccessRequired)
            {
                return;
            }

            // We need to update our status based on whether or not
            // the sensor is connected since we don't want to scan
            // without the sensor attached.
            if (StructureARPlugin.isStructureConnected())
            {
                this.gameState = SensorState.DeviceReady;
            } else
            {
                this.gameState = SensorState.DeviceNotReady;
            }

            //tracking is always false in DeviceReady and DeviceNotReady states
            this.trackingIsGood = false;
        }

        /// <summary>
        /// Raises the button click event.
        /// </summary>
        ///***********************  IMPORTANT  ***********************
        /// this is the method that sends the start, done and reset
        /// commands to the plug-in!
        public virtual void HandleButtonClickedEvent(object sender, ButtonEventArgs buttonArgs)
        {
            //we already know what state we're in, and what state we
            //need to go to. The local property will update the button's
            //when it's changed.
            switch (this.gameState)
            {   
                case SensorState.DeviceReady: // DeviceReady -> Scanning
                    if (this.MainCamera.GetComponent<PinchToScale>() != null)
                    {
                        this.MainCamera.GetComponent<PinchToScale>().enablePinchToScale = false;
                    }
                    //tell StructurePlugin to start scanning
                    StructureARPlugin.startScanning();
                    this.gameState = SensorState.Scanning;
                    break;

                case SensorState.Scanning: // Scanning -> WaitingForMesh
                    if (this.trackingIsGood)
                    {
                        //tell StructurePlugin to finish scanning.
                        //this finishes up the scanning session, when the
                        //mesh is finished, it's sent to the wireframe
                        //object where it's copied over.
                        //the HandleStructureEvent gets ScannedMeshReady where
                        //the construction of the mesh is completed.
                        StructureARPlugin.doneScanning();
                        this.gameState = SensorState.WaitingForMesh;
                    }
                    else
                    {
                        if (this.MainCamera.GetComponent<PinchToScale>() != null)
                        {
                            this.MainCamera.GetComponent<PinchToScale>().enablePinchToScale = true;
                        }

                        //clear scanned mesh
                        this.ClearScannedMesh();

                        StructureARPlugin.resetScanning();//tell StructurePlugin to reset the scanned data.
                        this.gameState = SensorState.DeviceReady;       
                    }
                    break;

                case SensorState.WaitingForMesh: // WaitingForMesh -> Playing
                    break;

                case SensorState.Playing: // Playing -> DeviceReady
                    if (this.MainCamera.GetComponent<PinchToScale>() != null)
                    {
                        this.MainCamera.GetComponent<PinchToScale>().enablePinchToScale = true;
                    }

                    //clear scanned mesh
                    this.ClearScannedMesh();

                    StructureARPlugin.resetScanning();//tell StructurePlugin to reset the scanned data.
                    this.gameState = SensorState.DeviceReady;
                    break;
                
                default:
                    GameLog.Log(this.ToString() + " -- unhandled game state for button" + buttonArgs.toState);
                    break;
            }
        }
        #endregion

        #region StructureDeviceCommands
        /// <summary>
        /// Raises the structure AR event event.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="structureArgs">Structure arguments.</param>
        protected virtual void HandleStructureEvent(object sender, StructureARPluginEventArgs structureArgs)
        {
            switch (structureArgs.eventType)
            {
                case StructureARPluginEvent.SensorConnected:
                    if (this.gameState == SensorState.DeviceNotReady)
                    {
                        if (this.MainCamera.GetComponent<PinchToScale>() != null)
                        {
                            this.MainCamera.GetComponent<PinchToScale>().enablePinchToScale = true;
                        }

                        //clear the scanned mesh
                        this.ClearScannedMesh();

                        StructureARPlugin.resetScanning();//tell StructurePlugin to reset the scanned data.
                    }
                    this.gameState = SensorState.DeviceReady;
                    break;

                case StructureARPluginEvent.CameraAccessRequired:
                    this.gameState = SensorState.CameraAccessRequired;
                    if (StructureARGameEvent != null)
                    {
                        StructureARGameEvent(this, new GameEventArgs(this.gameState, true));
                    }
                    break;

                case StructureARPluginEvent.SensorNeedsCharging:
                    this.gameState = SensorState.DeviceNeedsCharging;
                    break;

                case StructureARPluginEvent.SensorDisconnected:
                    this.gameState = SensorState.DeviceNotReady;
                    break;

                case StructureARPluginEvent.UpdateProjectionMatrix:
                    if (this.MainCamera != null)
                    {
                        this.MainCamera.projectionMatrix = StructureARPlugin.projectionMatrix;
                    }
                    break;

                case StructureARPluginEvent.ScannedMeshReady:
                    // this constructs the mesh from the scanned data
                    // and will be called once we have a mesh ready to build
                    this.BuildScannedMesh();
                    this.gameState = SensorState.Playing;
                    break;

                case StructureARPluginEvent.TrackingLost:
                    if (this.trackingIsGood == true)
                    {
                        if (StructureARGameEvent != null)
                        {
                            StructureARGameEvent(this, new GameEventArgs(this.gameState, false));
                        }
                        this.trackingIsGood = false;
                    }
                    break;

                case StructureARPluginEvent.TrackingFound:
                    if (this.trackingIsGood == false)
                    {
                        if (StructureARGameEvent != null)
                        {
                            StructureARGameEvent(this, new GameEventArgs(this.gameState, true));
                        }
                        this.trackingIsGood = true;
                    }
                    break;

                default:
                    break;
            }
        }
        #endregion

        #region DispatchUpdates
        protected void Update()
        {
            switch (this.gameState)
            {
                case SensorState.DeviceReady:
                case SensorState.Scanning:
                case SensorState.WaitingForMesh:
                case SensorState.Playing:
                    this.UpdateStructurePOV();
                    break;

                default:
                    break;
            }
        }

        //the camera in the game needs to update it's position
        //to the in pose reported by the plug-in
        protected virtual void UpdateStructurePOV()
        {
            if (this.structurePOV != null)
            {
                //get the position from the plug-in
                Vector3 position = StructureARPlugin.cameraPosition;
                this.structurePOV.UpdateCameraPosition(position);
                
                //get the rotation from the plug-in
                Quaternion rotation = StructureARPlugin.cameraRotation;
                this.structurePOV.UpdateCameraRotation(rotation);
            }
        }
        
        protected virtual void BuildScannedMesh()
        {
            if (this.scanObject != null && this.structureObjectLoader != null)
            {
                //this clears the wireframe of any Mesh data which may already be there.
                this.scanObject.GetComponent<Wireframe>().ClearMesh();

                //this will copy the mesh data from the objectLoader into the wireframe
                StartCoroutine(this.structureObjectLoader.LoadObject(this.scanObject));
                
				//this actually assigns the mesh from the objectLoadter to the wireframe
                this.scanObject.GetComponent<Wireframe>().ConstructMesh();
            }
        }
        
        protected virtual void ClearScannedMesh()
        {
            if (this.scanObject != null && structureObjectLoader != null)
            {
                this.scanObject.GetComponent<Wireframe>().ClearMesh();
            }
        }
    }
    #endregion

}