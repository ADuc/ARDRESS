/*
  This file is part of the Structure SDK.
  Copyright © 2015 Occipital, Inc. All rights reserved.
  http://structure.io
*/

using UnityEngine;
using System.Collections;
using StructureAR;

public class SimpleObject : MonoBehaviour
{
    public bool isTracking;
    public Bounds scanBounds;

    // Use this for initialization
    void Start()
    {
        Manager.StructureARGameEvent += HandleStructureARGameEvent;
        PinchToScale.TouchEvent += HandleScanVolumeChangeEvent;
        scanBounds.max = new Vector3(1, 1, 1);
        scanBounds.min = new Vector3(-1, 0, -1);

    }

    protected void HandleStructureARGameEvent(object sender, GameEventArgs args)
    {
        this.isTracking = args.isTracking;
        switch(args.gameState)
        {
            case SensorState.DeviceNotReady:
			case SensorState.CameraAccessRequired:
			case SensorState.DeviceNeedsCharging:
            case SensorState.DeviceReady:
            case SensorState.Playing:
            case SensorState.Reset:
            case SensorState.Scanning:
            case SensorState.WaitingForMesh:
                break;
        }
    }

    protected virtual void HandleScanVolumeChangeEvent(ScaleEventArgs args)
    {
        Vector3 min = new Vector3(-args.scale, 0, -args.scale);
        this.scanBounds.min = min;
        Vector3 max = new Vector3(args.scale, args.scale, args.scale);
        this.scanBounds.max = max;
    }
	
    // Update is called once per frame
    void Update()
    {
	
    }
}
