/*
  This file is part of the Structure SDK.
  Copyright © 2015 Occipital, Inc. All rights reserved.
  http://structure.io
*/

using System;
public enum StructureARPluginEvent
{
    SensorConnected,
	SensorDisconnected,
	CameraAccessRequired,
	SensorNeedsCharging,
    UpdateProjectionMatrix,
    ScannedMeshReady,
    TrackingFound,
    TrackingLost
}
