/*
  This file is part of the Structure SDK.
  Copyright © 2015 Occipital, Inc. All rights reserved.
  http://structure.io
*/

using System;
using UnityEngine;

namespace HoverCat
{
    public class Pickups : HoverCatObject
    {
        public GameObject PickupMesh;
        public float SpinSpeed = 5.0f;
        public float DropStartHeight = 2.0f;
        protected float DropEndHeight;

        protected virtual void Update()
        {
            this.PickupMesh.transform.Rotate(new Vector3(0, this.SpinSpeed, 0));
        }
    }
}