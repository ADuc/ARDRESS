/*
  This file is part of the Structure SDK.
  Copyright © 2015 Occipital, Inc. All rights reserved.
  http://structure.io
*/

using UnityEngine;
using System.Collections;
using StructureAR;

namespace BallGame
{
    //generate a bunch of balls in the scene
    public class BallGenerator : MonoBehaviour
    {
        public GameObject BallPrefab;
        public int xCount, yCount;
        public float xSpacing, ySpacing;
        public float scaleMin, scaleMax;
        // Use this for initialization
        void Start()
        {
            if(BallPrefab == null)
            {
                GameLog.Log("no BallPrefab Assigned.");
                return;
            }

            float xStart, yStart;
            xStart = ((xSpacing * xCount) * -0.5f) + this.gameObject.transform.position.x;
            yStart = ((ySpacing * yCount) * -0.5f) + this.gameObject.transform.position.z;

            for(int x = 0; x < xCount; ++x)
            {
                for(int y = 0; y < yCount; ++y)
                {
                    float xCord = x * this.xSpacing + (this.xSpacing / 2);
                    float yCord = y * this.ySpacing + (this.ySpacing / 2);
                    Vector3 ballPosition = new Vector3(xStart + xCord, this.gameObject.transform.position.y, yStart + yCord);
                    GameObject ball = GameObject.Instantiate(BallPrefab) as GameObject;
                    ball.GetComponent<BallPhysics>().scaleMin = this.scaleMin;
                    ball.GetComponent<BallPhysics>().scaleMax = this.scaleMax;
                    ball.transform.parent = this.gameObject.transform;
                    ballPosition.y += (y * 0.5f) + UnityEngine.Random.Range(-0.25f, 0.25f);
                    ballPosition.y += (x * 0.5f);
                    ball.transform.position = ballPosition;
                    Rigidbody rb = ball.GetComponent<Rigidbody>();
                    rb.drag = 4f;
                    rb.velocity = Vector3.zero;
                    rb.useGravity = false;
                    ball.name = "Sphere";
                }
            }
        }
    }
}
