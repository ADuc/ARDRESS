/*
  This file is part of the Structure SDK.
  Copyright © 2015 Occipital, Inc. All rights reserved.
  http://structure.io
*/

using System;
using System.Collections;
using UnityEngine;
using StructureAR;

namespace BallGame
{
    public class BallPhysics : BallCollision
    {
        #region LOCAL_VARIABLES
        public float scaleMin, scaleMax;
        private Rigidbody rigidBody;
        #endregion

        protected override void Start()
        {
            base.Start();
            this.startPosition = this.gameObject.transform.position;
			this.currentPosition = this.startPosition;
        }

        protected override void HandleStructureARGameEvent(object sender, GameEventArgs args)
        {
            this.isTracking = args.isTracking;
            switch(args.gameState)
            {
                case SensorState.DeviceNotReady:
				case SensorState.CameraAccessRequired:
				case SensorState.DeviceNeedsCharging:
                case SensorState.DeviceReady:
                case SensorState.Scanning:
                    this.GetComponent<Renderer>().enabled = false;
                    this.ballstate = BallState.ending;
                    break;
                
                default:
                    break;
            }
        }

        protected void DropFromPosition(Vector3 vec)
        {
            this.GetComponent<Rigidbody>().velocity = Vector3.zero;
            this.GetComponent<Rigidbody>().useGravity = true;
            this.GetComponent<Rigidbody>().solverIterationCount = 255;
            this.GetComponent<Renderer>().enabled = true;
            float rand = UnityEngine.Random.Range(scaleMin, scaleMax);
            this.transform.localScale = Vector3.one * rand * this.gameScale;
        }

        protected void HideAtPosition(Vector3 vec)
        {
            this.GetComponent<Renderer>().enabled = false;
            this.GetComponent<Rigidbody>().velocity = Vector3.zero;
            this.GetComponent<Rigidbody>().useGravity = false;
            this.GetComponent<Rigidbody>().solverIterationCount = 1;
            this.transform.position = vec;
        }

        protected override void Update()
        {
            switch(ballstate)
            {
                case BallState.starting:
                    this.HideAtPosition(this.currentPosition);
                    this.ballstate = BallState.started;
                    break;

                case BallState.started:
                    this.DropFromPosition(this.currentPosition);
                    this.ballstate = BallState.entering;
                    break;

                case BallState.entering:
                    this.gameObject.GetComponent<MeshRenderer>().material.color = this.InBoundsBallColor;
                    this.Fade = this.FadeTime;
                    this.GetComponent<Renderer>().material = this.OpaqueMaterial;
                    this.GetComponent<Renderer>().material.color = this.InBoundsBallColor;
                    this.ballstate = BallState.entered;
                    break;

                case BallState.entered:
                    this.GetComponent<Renderer>().enabled = this.isTracking;

                    if(this.CheckOutOfBounds(this.scanBounds, this.transform.position))
                    {
                        this.ballstate = BallState.exiting;
                        if(this.BallExitClip != null && this.isTracking && this.firstBounce)
                        {
                            AudioSource.PlayClipAtPoint(this.BallExitClip, this.transform.position);
                        }
                    }
                    break;

                case BallState.exiting:
                    this.gameObject.GetComponent<Renderer>().material = this.AlphaMaterial;
                    this.gameObject.GetComponent<Renderer>().material.color = this.OutBoundsBallColor;
                    this.ballstate = BallState.exited;
                    break;

                case BallState.exited:
                    if(this.CheckOutOfBounds(this.scanBounds, this.transform.position))
                    {
                        Color color = this.OutBoundsBallColor;
                        float alpha = (this.Fade -= Time.deltaTime) / this.FadeTime;
                        color.a = alpha;
                        this.GetComponent<Renderer>().material.color = color;
                        
                        if(alpha <= 0f)
                        {
                            this.ballstate = BallState.ending;
                        }
                    }
                    else
                    {
                        if(this.BallEnterClip != null && this.isTracking && this.firstBounce)
                        {
                            AudioSource.PlayClipAtPoint(this.BallEnterClip, this.transform.position);
                        }
                        this.ballstate = BallState.entering;
                    }
                    break;

                case BallState.ending:
                    this.HideAtPosition(this.currentPosition);
                    this.ballstate = BallState.ended;
                    break;

                case BallState.ended:
                    if(this.Restart)
                    {
                        this.ballstate = BallState.starting;
                        this.Restart = false;
                    }
                    break;
            }
        }
    }
}

