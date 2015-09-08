/*
  This file is part of the Structure SDK.
  Copyright © 2015 Occipital, Inc. All rights reserved.
  http://structure.io
*/

using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace StructureAR
{
    /// <summary>
    /// present the minimal number of button functions to the user with
    /// a simple class which doesn't do anything other than display some
    /// basic information about connectivity, tracking, and scanning.
    /// </summary>
    public class Buttons : MonoBehaviour
    {
		public GameObject DialogPanel;
		public Text DialogText;
		public GameObject ActionButton;
		public Text ActionButtonText;

		#region GAMEMANAGER_EVENT
        public delegate void ButtonClickedHandler(object sender,ButtonEventArgs args);
        public virtual event ButtonClickedHandler ButtonClickedEvent;
        #endregion

        #region PRIVATE_FIELDS
		protected SensorState gameState;
        protected bool trackingIsGood;
        #endregion

        #region UNITY_METHODS
        protected virtual void Start()
        {
			StartCoroutine_Auto(AssignGameEvent());
            Manager.StructureARGameEvent += HandleStructureARGameEvent;
        }

        protected virtual void Update()
        {
			switch (this.gameState)
            {
                case SensorState.DeviceNotReady:
                    this.ShowDialog("Please Connect Structure Sensor.");
					this.HideButton();	
                    break;

                case SensorState.CameraAccessRequired:
                    this.ShowDialog("This app requires camera access to capture color.\nAllow access by going to Settings -> Privacy -> Camera.");
					this.HideButton();	
					break;

                case SensorState.DeviceNeedsCharging:
                    this.ShowDialog("Please Charge Structure Sensor.");
					this.HideButton();	
					break;
				
			case SensorState.DeviceReady:
                    this.ShowDialog("Press \"Scan\" to Begin.");
                    this.ShowButton("Scan");
                    break;
                
                case SensorState.Scanning:
                    if (!this.trackingIsGood)
                    {
                        this.ShowDialog("Tracking Lost");
                        this.ShowButton("Re-Scan");
                    }
                    else
                    {
                        this.ShowDialog("Scanning...");
                        this.ShowButton("Done");
                    }
                    break;

                case SensorState.WaitingForMesh:
                    this.ShowDialog("Starting Game...");
					this.HideButton();	
					break;
				
			case SensorState.Playing:
                    if (!this.trackingIsGood)
                    {
                        this.ShowDialog("Tracking Lost");
                    } 
					else 
					{
						this.HideDialog();
					}
                    this.ShowButton("Re-Scan");
                    break;

                default:
					this.HideDialog();
					this.HideButton();	
					break;
			}
		}
        #endregion

        #region GAMEMANAGER_EVENTS
        //assign ourselves a structure manager.
        protected IEnumerator AssignGameEvent()
        {
            while (Manager._structureManager==null)
            {
                yield return new WaitForEndOfFrame();
            }
            //the manager has arrived, wait for instructions.
            this.ButtonClickedEvent += Manager._structureManager.HandleButtonClickedEvent;
            
            //set initial state on startup
            this.gameState = Manager._structureManager.gameState;
        }

        protected void HandleStructureARGameEvent(object sender, GameEventArgs args)
        {
            this.gameState = args.gameState;
            this.trackingIsGood = args.isTracking;
        }
        #endregion

		public virtual void ActionButtonClicked()
		{
			this.ButtonClickedEvent(this, new ButtonEventArgs(ButtonEvent.Button, this.gameState));
		}

        #region PROTECTED_METHODS
        protected void ShowDialog(string text)
        {
			DialogPanel.SetActive(true);
			DialogText.text = text;
        }

		protected void HideDialog()
		{
			DialogPanel.SetActive(false);
		}

        protected virtual void ShowButton(string buttonText)
        {
			ActionButton.SetActive(true);
			ActionButtonText.text = buttonText;
        }

		protected virtual void HideButton()
		{
			ActionButton.SetActive(false);
		}

        #endregion
    }
}
