/*
  This file is part of the Structure SDK.
  Copyright © 2015 Occipital, Inc. All rights reserved.
  http://structure.io
*/

using UnityEngine;
using System.Collections;
using StructureAR;

namespace HoverCat
{
    public class DragMove : MonoBehaviour
    {
        public Camera MainCamera;
        public float originalY;
        public GameObject GroundObject;
        private bool touchedHoop;
    
        // Update is called once per frame
        void Update()
        {
            if (Input.touchCount > 0)
            {
                Ray ray = this.MainCamera.ScreenPointToRay(Input.touches [0].position);
                RaycastHit[] hits = Physics.RaycastAll(this.MainCamera.transform.position, ray.direction);
                switch (Input.touches [0].phase)
                {
                    case TouchPhase.Began:
                        foreach (RaycastHit hit in hits)
                        {
                            if (hit.collider == this.GetComponent<Collider>())
                            {
                                GameLog.Log(this, "touching hoop!!!");
                                this.touchedHoop = true;
                            }
                        }
                        break;
                    
                    case TouchPhase.Stationary:
                    case TouchPhase.Moved:
                        if (this.touchedHoop)
                        {
                            foreach (RaycastHit hit in hits)
                            {
                                if (hit.collider == this.GroundObject.GetComponent<Collider>())
                                {
                                    this.gameObject.transform.position = hit.point;
                                }
                            }
                        }
                        break;
                    
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        this.touchedHoop = false;
                        break;
                }
        
            }
        }
    }
}