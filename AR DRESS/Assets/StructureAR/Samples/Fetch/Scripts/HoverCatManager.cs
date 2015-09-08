/*
  This file is part of the Structure SDK.
  Copyright © 2015 Occipital, Inc. All rights reserved.
  http://structure.io
*/

using System;
using UnityEngine;
using StructureAR;

namespace HoverCat
{
    //adding a layer of added communication to the
    //base Manager class.
    public class HoverCatManager : Manager
    {
        public static HoverCatManager _HoverCatManager;
        public delegate void HoverCatHandler(object sender,HoverCatEventArgs catArgs);
        public static event HoverCatHandler HoverCatGameEvent;
        private bool isCatTracking;


        public override void Start()
        {
            base.Start();
            HoverCatManager._HoverCatManager = this;
			HoverCatButtons.HoverCatButtonClickedEvent += HandleHoverCatButtonClickedEvent;
        }

        public void HandleHoverCatButtonClickedEvent(object sender, HoverCatButtonEventArgs buttonArgs)
        {
            switch(this.gameState)
            {   
                case SensorState.DeviceReady:
                    this.MainCamera.GetComponent<PinchToScale>().enablePinchToScale = false;
                    StructureARPlugin.startScanning();//tell StructurePlugin to start scanning
                    this.gameState = SensorState.Scanning;
                    break;
                    
                case SensorState.Scanning:
                    StructureARPlugin.doneScanning();//tell StructurePlugin to finish scanning.
                    this.gameState = SensorState.WaitingForMesh;
                    break;
                    
                case SensorState.WaitingForMesh://ignore buttons.
                    break;
                    
                case SensorState.Playing:

					if (buttonArgs.catButtonEvent == HoverCatButtonEvent.ResetGame)
					{
                        this.MainCamera.GetComponent<PinchToScale>().enablePinchToScale = true;
                        structureObjectLoader.ClearMesh(this.scanObject);
                        StructureARPlugin.resetScanning();//tell StructurePlugin to reset the scanned data.
                        this.gameState = SensorState.DeviceReady;
					}
                    break;
                    
                default:
                    GameLog.Log(this.ToString() + " -- unhandled game state for button" + buttonArgs.catButtonEvent);
                    break;
            }

        }

        //inherit old events then send out new ones for
        //the cat and any other objects assigned to the event.
		protected override void HandleStructureEvent(object sender, StructureARPluginEventArgs structureArgs)
        {
			base.HandleStructureEvent(sender, structureArgs);

            if(HoverCatGameEvent == null)
            {
                return;
            }

            switch(structureArgs.eventType)
            {
                case StructureARPluginEvent.TrackingLost:
					if(this.trackingIsGood != isCatTracking)
                    {
                        if(this.gameState == SensorState.Playing)
                        {
                            HoverCatEventArgs catArgs = new HoverCatEventArgs(
                                this.gameState, this.trackingIsGood, HoverCatEvent.StateChange);
                            catArgs.catEvent = HoverCatEvent.Hide;
                            HoverCatGameEvent(this, catArgs);
                        }
                        isCatTracking = trackingIsGood;
                    }
                    break;
                
                case StructureARPluginEvent.TrackingFound:
					if(this.trackingIsGood != isCatTracking)
                    {
                        if(this.gameState == SensorState.Playing)
                        {
                            HoverCatEventArgs catArgs = new HoverCatEventArgs(
								this.gameState, this.trackingIsGood,  HoverCatEvent.StateChange);
                            catArgs.catEvent = HoverCatEvent.Show;
                            HoverCatGameEvent(this, catArgs);
                        }
						isCatTracking = trackingIsGood;
                    }
                    break;

                case StructureARPluginEvent.ScannedMeshReady:
					if(this.gameState == SensorState.Playing)
                    {
                        HoverCatEventArgs catArgs = new HoverCatEventArgs(
							this.gameState, this.trackingIsGood, HoverCatEvent.StateChange);
                        catArgs.catEvent = HoverCatEvent.Reset;
                        HoverCatGameEvent(this, catArgs);
                    }
                    break;

				case StructureARPluginEvent.CameraAccessRequired:
                case StructureARPluginEvent.SensorDisconnected:
				case StructureARPluginEvent.SensorNeedsCharging:
					if(this.gameState == SensorState.Playing)
                    {
                        HoverCatEventArgs catArgs = new HoverCatEventArgs(
							this.gameState, this.trackingIsGood, HoverCatEvent.StateChange);
                        catArgs.catEvent = HoverCatEvent.Hide;
                        HoverCatGameEvent(this, catArgs);
                    }
                    break;

                case StructureARPluginEvent.SensorConnected:
					if(this.gameState == SensorState.Playing)
                    {
                        HoverCatEventArgs catArgs = new HoverCatEventArgs(
							this.gameState, this.trackingIsGood, HoverCatEvent.StateChange);
                        catArgs.catEvent = HoverCatEvent.Show;
                        HoverCatGameEvent(this, catArgs);
                    }
                    break;

                case StructureARPluginEvent.UpdateProjectionMatrix:
                default:
                    break;
            }
        }
    }
}

