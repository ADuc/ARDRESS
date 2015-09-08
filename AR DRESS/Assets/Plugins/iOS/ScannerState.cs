/*
  This file is part of the Structure SDK.
  Copyright © 2015 Occipital, Inc. All rights reserved.
  http://structure.io
*/

using System;
namespace StructureAR
{
    public enum ScannerState
    {
        ScannerStateUnknown = -1, 
        
        // Defining the volume to scan
        ScannerStateCubePlacement = 0,
        
        // Scanning
        ScannerStateScanning,
        
        // Visualizing the mesh
        ScannerStateViewing,
        
        NumStates
    }
}