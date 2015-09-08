/*
  This file is part of the Structure SDK.
  Copyright © 2015 Occipital, Inc. All rights reserved.
  http://structure.io
*/

using System;
using System.Collections;
using UnityEngine;
using StructureAR;

namespace HoverCat
{
    public class HoverCatObject : MonoBehaviour
    {
        #region BASE_VARIABLES
        public HoverCatManager HoverGameManager;
        protected Vector3 HomePosition;
        #endregion

        #region INITIALIZATION_METHODS
        protected virtual void Start()
        {
			Manager.StructureARGameEvent += HandleStructureARGameEvent;
			HoverCatManager.HoverCatGameEvent += HandleHoverCatGameEvent;
			HoverCatButtons.HoverCatButtonClickedEvent += HandleHoverCatButtonEvent;

            this.HomePosition = this.gameObject.transform.position;
			this.Home();
            this.Freeze();
            this.Hide();
        }

        #endregion

        #region BASEEVENTCOMMANDS
        public virtual void Show()
        {
            if (this.gameObject.GetComponent<Rigidbody>() != null)
            {
                this.gameObject.GetComponent<Rigidbody>().useGravity = true;
            }

            this.gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            this.gameObject.SetActive(false);
        }

        public virtual void Freeze()
        {
            if (this.gameObject.GetComponent<Rigidbody>() != null)
            {
                this.gameObject.GetComponent<Rigidbody>().useGravity = false;
                this.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            }
        }

        public virtual void Home()
        {
            this.gameObject.transform.position = this.HomePosition;
        }
        #endregion

        #region BASEEVENTHANDLERS
        protected virtual void HandleStructureARGameEvent(object sender, GameEventArgs args)
        {
            GameLog.Log(sender, this.ToString() + ":" + args.gameState);
            switch (args.gameState)
            {
                case SensorState.Playing:
                    break;
                case SensorState.DeviceNotReady:
				case SensorState.CameraAccessRequired:
				case SensorState.DeviceNeedsCharging:
                case SensorState.DeviceReady:
                case SensorState.Scanning:
                    this.Hide();
                    this.Freeze();
                    break;
                    
                default:
                    break;
            }
        }

		protected virtual void HandleHoverCatButtonEvent(object sender, HoverCatButtonEventArgs args)
		{
			switch (args.catButtonEvent)
			{
				case HoverCatButtonEvent.ResetObjects:
					this.Freeze();
					this.Home();
					this.Show();
					break;
				case HoverCatButtonEvent.ResetGame:
					this.Freeze();
					this.Home();
					break;
					
			}
		}

        /// <summary>
        /// handles events coming in from the game manager
        /// </summary>
        protected virtual void HandleHoverCatGameEvent(object sender, HoverCatEventArgs args)
        {
            GameLog.Log(sender, this.ToString() + ":" + args.catEvent);
            switch (args.catEvent)
            {
                case HoverCatEvent.Show:
                    this.Show();
                    break;
                    
                case HoverCatEvent.Hide:
                    this.Freeze();
                    this.Hide();
                    break;
                    
                case HoverCatEvent.Reset:
                    this.Freeze();
                    this.Home();
                    this.Show();
                    break;
                    
                default:
                    break;
            }
        }
        #endregion
    }
}