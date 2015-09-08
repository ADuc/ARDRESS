/*
  This file is part of the Structure SDK.
  Copyright © 2015 Occipital, Inc. All rights reserved.
  http://structure.io
*/

using System;
namespace StructureAR
{
    public class ButtonEventArgs : EventArgs
    {
        public ButtonEvent button;
        public SensorState toState;
        public ButtonEventArgs(ButtonEvent buttonEvent, SensorState toState)
        {
            this.button = buttonEvent;
            this.toState = toState;
        }
    }

    public class ScaleEventArgs: EventArgs
    {
        public float scale;
        public ScaleEventArgs(float newScale)
        {
            this.scale = newScale;
        }

    }
}
