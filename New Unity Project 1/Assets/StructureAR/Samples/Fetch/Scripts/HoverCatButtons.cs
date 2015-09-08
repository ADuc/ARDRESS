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
    public class HoverCatButtons : StructureAR.Buttons
    {
		public GameObject ResetObjectsButton;
		//By default, don't show the hoop
		public bool enableHoop = false;
		public GameObject ToggleHoopButton;

        #region GAMEMANAGER_EVENT
		public delegate void HoverCatButtonClickedHandler(object sender, HoverCatButtonEventArgs args);
        public static event HoverCatButtonClickedHandler HoverCatButtonClickedEvent;
        #endregion
        protected override void Start()
        {
			base.Start();
			if (!enableHoop)
			{
				GameObject hoop = GameObject.Find ("Hoop");
				hoop.SetActive(false);
			}
		}

        protected override void Update()
        {
			base.Update();

			switch (this.gameState)
            {
                case SensorState.Playing:
					this.ShowResetObjectsButton();
                    if (enableHoop)
                        this.ShowToggleHoopButton();
					else
						this.HideToggleHoopButton();
                    break;
                    
                default:
					this.HideResetObjectsButton();
					this.HideToggleHoopButton();
                    break;
            }
        }

		void ShowToggleHoopButton()
		{
			ToggleHoopButton.SetActive(true);
		}
		void HideToggleHoopButton()
		{
			ToggleHoopButton.SetActive(false);
		}

		public void ToggleHoopButtonClicked()
		{
			if (HoverCatButtonClickedEvent != null)
			{
				HoverCatButtonClickedEvent(this, new HoverCatButtonEventArgs(HoverCatButtonEvent.ToggleHoop));
			}
		}

		void ShowResetObjectsButton()
		{
			ResetObjectsButton.SetActive(true);
		}
		void HideResetObjectsButton()
		{
			ResetObjectsButton.SetActive(false);
		}

		public void ResetObjectsButtonClicked()
		{
			if (HoverCatButtonClickedEvent != null)
			{
				HoverCatButtonClickedEvent(this, new HoverCatButtonEventArgs(HoverCatButtonEvent.ResetObjects));
			}
		}

		public override void ActionButtonClicked()
		{
			if (HoverCatButtonClickedEvent != null)
			{
				HoverCatButtonClickedEvent(this, new HoverCatButtonEventArgs(HoverCatButtonEvent.ResetGame));
			}
		}
    }
}

