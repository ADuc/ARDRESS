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
    public class Hoop : Pickups
    {
		protected override void Start()
        {
            base.Start();
        }
        // Use this for initialization
		protected override void HandleHoverCatButtonEvent(object sender, HoverCatButtonEventArgs args)
        {
			base.HandleHoverCatButtonEvent(sender, args);
            if(args.catButtonEvent == HoverCatButtonEvent.ToggleHoop)
            {
                if(this.gameObject.activeSelf)
                {
                    this.Hide();
                }
                else
                {
                    this.Show();
                }
            }
        }

        public override void Hide()
        {
            base.Hide();
            GameLog.Log(this, "hiding hoop!");
        }

        public override void Show()
        {
			base.Show();
            GameLog.Log(this, "showing hoop!");
        }
    }
}

