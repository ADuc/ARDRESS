/*
  This file is part of the Structure SDK.
  Copyright © 2015 Occipital, Inc. All rights reserved.
  http://structure.io
*/

using UnityEngine;
using System.Collections;

namespace StructureAR
{
    /// <summary>
    /// Pinch to scale.
    /// This should be added to the Main Camera to tell the plugin when
    /// you are changing the scan area. This is added automatically when
    /// the structureAR menu item Create -> Manager is used. though if
    /// you are using a custom manager you'll want to add these your self
    /// or you could create a manager and delete it from the scene before
    /// adding your own custom manager.
    /// </summary>
    public class PinchToScale : MonoBehaviour
    {
        //public static PinchToScale _PinchToScale;
        public bool enablePinchToScale;
        public float CurrentScale = 1.0f;
        public float MinScale = 0.5f;
        public float MaxScale = 2.0f;
        private float DeltaScale = 1.0f;
        private float InitialScale = 1.0f;   
        private Touch TouchStart0, TouchStart1;
        private int PrevTouchCount;

        public delegate void TouchEventHandler(ScaleEventArgs args);
        //public delegate void TouchEventHandler(float scale);

        //assign your event listeners to this event to get structure updates.
        public static event TouchEventHandler TouchEvent;
        
        // Use this for initialization
        void Start()
        {
            //PinchToScale._PinchToScale = this;
            PrevTouchCount = 0;
        }

        private void HandlePinchStarted()
        {
            this.InitialScale = this.CurrentScale;
            
            this.TouchStart0 = Input.GetTouch(0);
            this.TouchStart1 = Input.GetTouch(1);
        }
        
        private void HandlePinchEnded()
        {
            this.CurrentScale = this.InitialScale * this.DeltaScale;
            this.CurrentScale = Mathf.Max(Mathf.Min(this.MaxScale, this.CurrentScale), this.MinScale);
        }
        
        // Update is called once per frame
        void Update()
        {
            if (!this.enablePinchToScale)
            {
                return;
            }
            
            int touchesCount = 0;
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
                {
                    touchesCount++;
                }
            }
            
            if (this.PrevTouchCount < 2 && touchesCount == 2)
            {
                this.HandlePinchStarted();
            }
            else if (this.PrevTouchCount == 2 && touchesCount < 2)
            {
                this.HandlePinchEnded();
            }
            else if (touchesCount == 2 && this.PrevTouchCount == 2)// If there are two touches on the device...
            {
                // Store both touches.
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);
                
                if (touchZero.phase == TouchPhase.Moved && touchOne.phase == TouchPhase.Moved)
                {       
                    Touch touchCurr0, touchCurr1;
                    
                    if (touchZero.fingerId == TouchStart0.fingerId)
                    {
                        touchCurr0 = touchZero;
                        touchCurr1 = touchOne;
                    }
                    else
                    {
                        touchCurr0 = touchOne;
                        touchCurr1 = touchZero;
                    }
                    Vector2 v0 = TouchStart0.position - TouchStart1.position;
                    Vector2 v1 = touchCurr0.position - touchCurr1.position;
                    this.DeltaScale = Mathf.Sqrt(v1.magnitude / v0.magnitude);
                    this.CurrentScale = this.InitialScale * this.DeltaScale;
                }
                
                this.CurrentScale = Mathf.Max(Mathf.Min(this.MaxScale, this.CurrentScale), this.MinScale);

                // Send pinch events
                StructureARPlugin.handlePinchScale(this.CurrentScale);
                if (TouchEvent != null)
                {
                    TouchEvent(new ScaleEventArgs(this.CurrentScale));
                }
                
            }
            
            this.PrevTouchCount = touchesCount;
        }
    }
}
