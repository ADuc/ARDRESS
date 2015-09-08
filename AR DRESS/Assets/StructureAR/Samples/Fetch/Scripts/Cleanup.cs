/*
  This file is part of the Structure SDK.
  Copyright © 2015 Occipital, Inc. All rights reserved.
  http://structure.io
*/

using UnityEngine;
using System.Collections;

namespace HoverCat
{

    public class Cleanup : MonoBehaviour
    {
        public float CountDown = 1.0f;
        void Update()
        {
            CountDown -= Time.deltaTime;
            if (CountDown <= 0)
            {
                Destroy(this.gameObject);
            }
        }
    }
}
