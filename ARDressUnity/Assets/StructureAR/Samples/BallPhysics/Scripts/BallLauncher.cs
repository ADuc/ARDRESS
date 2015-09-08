/*
  This file is part of the Structure SDK.
  Copyright © 2015 Occipital, Inc. All rights reserved.
  http://structure.io
*/

// <summary>
// 
// Launch Balls into the scene.
// Assign me to the camera.
//
// </summary>

using System;
using System.Collections;
using UnityEngine;
using StructureAR;

namespace BallGame
{
    public class BallLauncher : MonoBehaviour
    {     
		private Camera MainCamera;

		public GameObject TossBall;
        public AudioClip TossBallClip;

        // Launcher members
        private GameObject[] BallAmmo;
        private int BallIndex;
		private Vector3 TossDirection;
		private float TossSpeed;
		private Vector3 ballScale;

        // State-related members
        private bool isTracking;
		private SensorState gameState;
        public delegate void DropBallsEventHandler();
        public static event DropBallsEventHandler DropBallsEvent;
		private float gameScale = 1.0f;

        void HandleStructureARGameEvent(object sender, GameEventArgs args)
        {      
			this.gameState = args.gameState;
			this.isTracking = args.isTracking;

			bool showUI = true;
			if (!this.isTracking || ( this.gameState != SensorState.Playing))
			{
				showUI = false;			
			}
			this.gameObject.SetActive(showUI);
        }

		void HandleScanVolumeChangeEvent(ScaleEventArgs args)
		{
			this.gameScale = args.scale;
		}
		
        void Start()
        {
            Manager.StructureARGameEvent += HandleStructureARGameEvent;
			PinchToScale.TouchEvent += HandleScanVolumeChangeEvent;

			//initial call to set state, regardless of whether Structure is plugged in.
			HandleStructureARGameEvent(null, new GameEventArgs(SensorState.DeviceNotReady, false));

            this.MainCamera = Camera.main;
			
            this.BallAmmo = new GameObject[10];
            for(int i = 0; i < 10; ++i)
            {
                GameObject ball = (GameObject)GameObject.Instantiate(this.TossBall);
                BallAmmo[i] = ball;
				this.ballScale = ball.transform.localScale;
			}
        }

		public void LaunchBall()
		{
			int next = BallIndex + 1;
			BallIndex = next % 10;
			GameObject nextBall = BallAmmo[BallIndex];
			BallCollision bc = nextBall.GetComponent<BallCollision>();
			bc.HandleDropBallsEvent();
			nextBall.transform.position = MainCamera.transform.position;
			nextBall.transform.localScale = this.ballScale * gameScale;
			nextBall.GetComponent<Rigidbody>().velocity = Vector3.zero;
			nextBall.GetComponent<Rigidbody>().AddForce(this.MainCamera.transform.forward * (nextBall.GetComponent<Rigidbody>().drag * 2), ForceMode.Impulse);
			nextBall.GetComponent<Rigidbody>().solverIterationCount = 255;
			nextBall.GetComponent<Rigidbody>().useGravity = true;
			nextBall.GetComponent<Renderer>().enabled = true;
			
			if(this.TossBallClip != null)
			{
				AudioSource.PlayClipAtPoint(this.TossBallClip, this.transform.position);
			}
		}  

		public void DropBalls()
		{
			// Send the reset event to the scene balls
			if(DropBallsEvent != null)
			{
				DropBallsEvent();
			}
		} 
    }
}

