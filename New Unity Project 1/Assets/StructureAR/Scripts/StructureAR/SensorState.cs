/*
  This file is part of the Structure SDK.
  Copyright © 2015 Occipital, Inc. All rights reserved.
  http://structure.io
*/

using System;
namespace StructureAR
{
    public enum SensorState
    {
        DeviceNotReady,
        CameraAccessRequired,
        DeviceNeedsCharging,
        DeviceReady,
        Scanning,
        WaitingForMesh,
        Playing,
        Reset
    }
}