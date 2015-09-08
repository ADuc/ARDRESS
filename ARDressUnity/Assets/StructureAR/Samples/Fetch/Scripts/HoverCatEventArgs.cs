/*
  This file is part of the Structure SDK.
  Copyright © 2015 Occipital, Inc. All rights reserved.
  http://structure.io
*/

using System;
using UnityEngine;
using StructureAR;

namespace HoverCat
{
    public class HoverCatEventArgs : GameEventArgs
    {
        public HoverCatEvent catEvent;
        public bool isSuperJumpping;
        public bool isSuperSpeeding;
        public Vector3 CatPosition;
        public Collider OtherCollider;
        public Vector3 OtherColliderHitPoint;

        public HoverCatEventArgs(SensorState gameState, bool trackingIsGood) : base(gameState, trackingIsGood)
        {
            this.catEvent = HoverCatEvent.StateChange;
        }

        public HoverCatEventArgs(SensorState gameState, bool trackingIsGood, HoverCatEvent catEvent) : base(gameState, trackingIsGood)
        {
            this.catEvent = catEvent;
        }
    }
}
