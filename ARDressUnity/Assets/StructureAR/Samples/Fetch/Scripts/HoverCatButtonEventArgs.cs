/*
  This file is part of the Structure SDK.
  Copyright © 2015 Occipital, Inc. All rights reserved.
  http://structure.io
*/

using System;
namespace StructureAR
{
	public class HoverCatButtonEventArgs
	{
		public HoverCatButtonEvent catButtonEvent;
		public HoverCatButtonEventArgs(HoverCatButtonEvent catButtonEvent)
		{
			this.catButtonEvent = catButtonEvent;
		}
	}
}