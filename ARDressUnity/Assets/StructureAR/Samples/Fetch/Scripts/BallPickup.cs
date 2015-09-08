/*
  This file is part of the Structure SDK.
  Copyright © 2015 Occipital, Inc. All rights reserved.
  http://structure.io
*/

using UnityEngine;
using System;
using System.Collections;
using StructureAR;

namespace HoverCat
{
    /// <summary>
    /// Ball pickup.
    /// Powerup for giving the HoverCat extra jump height.
    /// </summary>
    public class BallPickup : Pickups
    {
        #region VARIABLES
            #region publicLocals
        public GameObject PickupEffect;
        public float GroundOffset;
//        public float StartBallHeight;
            #endregion

            #region privateLocals
        private bool isActive;
        //private Vector3 CurrentBallPosition;
        private float EndBallHeight;
            #endregion
        #endregion

        #region UNITY_METHODS

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
            
			if (this.gameObject.transform.position.y > this.EndBallHeight)
            {
				Vector3 lowerPosition = this.gameObject.transform.position;
				lowerPosition.y -= Time.deltaTime;
				this.gameObject.transform.position = lowerPosition;
            }
        }
        #endregion

        #region BALLPICKUP_METHODS
        //check if there's anything under the ball before it lands
        protected float FindDropEndHeight()
        {
			Ray ray = new Ray(this.gameObject.transform.position, Vector3.down);

            if (!Physics.Raycast(ray))
            {
                return this.GroundOffset;
            }

            //find the highest ray intersection
            float endHeight = Mathf.NegativeInfinity;
            RaycastHit[] hits = Physics.RaycastAll(ray);
            foreach (RaycastHit hit in hits)
            {
                if (hit.point.y > endHeight)
                {
                    endHeight = hit.point.y;
                }
            }
            return endHeight + this.GroundOffset;
        }

        //find a new place to begin dropping from.
        protected Vector3 RandomDropStartPosition()
        {
            float randomX = UnityEngine.Random.Range(-1.0f, 1.0f);
            float y = this.DropStartHeight;
            float randomZ = UnityEngine.Random.Range(-0.5f, 0.5f);
            Vector3 newLocation = new Vector3(randomX, y, randomZ);
            return newLocation;
        }
        #endregion

        #region EVENTCOMMAND_OVERRIDES
		//send the ball home
		public override void Home()
		{
			this.gameObject.transform.position = this.RandomDropStartPosition();
			this.EndBallHeight = this.FindDropEndHeight();
		}

        #endregion
    }
}
