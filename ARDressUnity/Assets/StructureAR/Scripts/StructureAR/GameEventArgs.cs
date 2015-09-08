/*
  This file is part of the Structure SDK.
  Copyright © 2015 Occipital, Inc. All rights reserved.
  http://structure.io
*/

using System;

namespace StructureAR
{
    public class GameEventArgs
    {
        public SensorState gameState;
        public bool isTracking;

    	public GameEventArgs(SensorState gameState, bool trackingIsGood)
        {
            this.gameState = gameState;
      		this.isTracking = trackingIsGood;
        }
    }
}

