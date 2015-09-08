/*
  This file is part of the Structure SDK.
  Copyright © 2015 Occipital, Inc. All rights reserved.
  http://structure.io
*/

using UnityEngine;
using System.Collections;

namespace StructureAR
{
    //this class may also want to check that forward rendering is being used
    //otherwise the transparent shaders won't work properly.
    public class POV : MonoBehaviour
    {
        void Start()
        {
            //if the clear flag isn't set to Depth then
            //we'll have problems rendering the color camera
            //to the camera with the depth overlay.
            GameLog.Log("Checking camera clear flags...");
            Camera cam = this.GetComponent<Camera>();
            if(cam != null)
            {
                if (this.GetComponent<Camera>().clearFlags != CameraClearFlags.Depth)
                {
                    //make sure camera is set to depth for clear flags
                    //otherwise it will not draw the back camera view.
                    this.GetComponent<Camera>().clearFlags = CameraClearFlags.Depth;
                    GameLog.Log("clear flag changed to depth.");
                }
            }
#if UNITY_IPHONE
            Screen.orientation = ScreenOrientation.LandscapeLeft;
#endif
        }

        public void UpdateCameraPosition(Vector3 position)
        {
            //this positions the cameras translation in space
            //make sure that the camera starts around the origin
#if UNITY_IPHONE
            this.gameObject.transform.position = position;
#endif
        }

        public void UpdateCameraRotation(Quaternion rotation)
        {
            //this updates the cameras rotation
#if UNITY_IPHONE
            this.gameObject.transform.rotation = rotation;
#endif
        }
    
        protected void OnPostRender()
        {   
            //this tells the StructureAR plugin that
            //we have finished showing the rendering
#if UNITY_IPHONE
            StructureARPlugin.CallPostRenderEvent();
            GL.InvalidateState();
#endif
        }
    }
}