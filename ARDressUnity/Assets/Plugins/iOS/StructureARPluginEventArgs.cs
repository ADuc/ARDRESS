/*
  This file is part of the Structure SDK.
  Copyright © 2015 Occipital, Inc. All rights reserved.
  http://structure.io
*/

using System;

namespace StructureAR
{
	public class StructureARPluginEventArgs : EventArgs
    {
		public StructureARPluginEvent eventType;
		public StructureARPluginEventArgs(StructureARPluginEvent myEvent)
        {
            this.eventType = myEvent;
        }
    }
}
