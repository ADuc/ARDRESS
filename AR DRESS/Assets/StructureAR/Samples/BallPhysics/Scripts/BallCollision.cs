/*
  This file is part of the Structure SDK.
  Copyright © 2015 Occipital, Inc. All rights reserved.
  http://structure.io
*/

using System;
using System.Collections;
using UnityEngine;
using StructureAR;

namespace BallGame
{
    public class BallCollision : MonoBehaviour
    {        
        public AudioClip BounceClip;
        public AudioClip BallExitClip;
        public AudioClip BallEnterClip;
        public GameObject PuffEffect;
        public static int BounceClipCount;
        const int MaxClipsPlaying = 16;
        protected enum BallState
        {
            starting = 0,
            started,
            entering,
            entered,
            exiting,
            exited,
            ending,
            ended
        }
        protected BallState ballstate;
        protected bool isTracking;
        public Color InBoundsBallColor;
        public Color OutBoundsBallColor;
        public float FadeTime;
        protected float Fade;
        public Material AlphaMaterial;
        public Material OpaqueMaterial;
        protected float gameScale;

        public Vector3 startPosition;
        protected Vector3 currentPosition;

        public bool Restart;
        public Bounds scanBounds;
        public bool firstBounce;
        protected virtual void Start()
        {
            this.ballstate = BallState.ending;
            this.Restart = false;
            scanBounds.max = new Vector3(1, 1, 1);
            scanBounds.min = new Vector3(-1, 0, -1);
            Manager.StructureARGameEvent += HandleStructureARGameEvent;
            PinchToScale.TouchEvent += HandleScanVolumeChangeEvent;
            BallLauncher.DropBallsEvent += HandleDropBallsEvent;
            this.GetComponent<AudioSource>().priority = 0;//most important
            this.gameObject.name = "Sphere";
            this.GetComponent<Rigidbody>().solverIterationCount = 255;//checking at max
            this.GetComponent<Rigidbody>().detectCollisions = true;
            this.gameScale = 1.0f;
        }

        public virtual void HandleDropBallsEvent()
        {
            this.ballstate = BallState.starting;
            this.firstBounce = false;
        }
        
        protected virtual void HandleStructureARGameEvent(object sender, GameEventArgs args)
        {
            this.isTracking = args.isTracking;
            switch(args.gameState)
            {
                case SensorState.Playing:
                    if(args.isTracking)
                    {
                        this.ballstate = BallState.starting;
                        this.GetComponent<Renderer>().enabled = true;
                    }
                    else
                    {
                        this.GetComponent<Renderer>().enabled = false;
                    }
                    break;
                
				case SensorState.DeviceNotReady:
				case SensorState.CameraAccessRequired:
				case SensorState.DeviceNeedsCharging:
                case SensorState.DeviceReady:
                case SensorState.Scanning:
                    this.GetComponent<Renderer>().enabled = false;
                    this.ballstate = BallState.ending;
                    break;
                    
                default:
                    break;
            }
        }

        protected virtual void HandleScanVolumeChangeEvent(ScaleEventArgs args)
        {
            Vector3 min = new Vector3(-args.scale, 0, -args.scale);
            this.scanBounds.min = min;
            Vector3 max = new Vector3(args.scale, args.scale, args.scale);
            this.scanBounds.max = max;
			
            this.gameScale = args.scale;
            this.currentPosition = this.startPosition * args.scale;
        }

        protected bool CheckOutOfBounds(Bounds box, Vector3 vec)
        {
            return(vec.x < box.min.x || 
                vec.z < box.min.z ||
                vec.x > box.max.x ||
                vec.z > box.max.z);
        }

        protected virtual void Update()
        {
            switch(this.ballstate)
            {
                case BallState.starting:
                    this.ballstate = BallState.started;
                    break;
                    
                case BallState.started:
                    this.ballstate = BallState.entering;
                    break;
                    
                case BallState.entering:
                    this.gameObject.GetComponent<MeshRenderer>().material.color = this.InBoundsBallColor;
                    this.Fade = this.FadeTime;
                    this.GetComponent<Renderer>().material = this.OpaqueMaterial;
                    this.GetComponent<Renderer>().material.color = this.InBoundsBallColor;
                    this.firstBounce = false;
                    this.ballstate = BallState.entered;
                    break;
                    
                case BallState.entered:
                    if(CheckOutOfBounds(this.scanBounds, this.transform.position))
                    {
                        if(this.BallExitClip != null && this.isTracking && this.firstBounce)
                        {
                            AudioSource.PlayClipAtPoint(this.BallExitClip, this.transform.position);
                        }
                        this.ballstate = BallState.exiting;
                    }
                    break;
                    
                case BallState.exiting:
                    this.gameObject.GetComponent<Renderer>().material = this.AlphaMaterial;
                    this.gameObject.GetComponent<Renderer>().material.color = this.OutBoundsBallColor;
                    this.ballstate = BallState.exited;
                    break;
                    
                case BallState.exited:
                    if(this.CheckOutOfBounds(this.scanBounds, this.transform.position))
                    {
                        Color color = this.OutBoundsBallColor;
                        float alpha = (this.Fade -= Time.deltaTime) / this.FadeTime;
                        color.a = alpha;
                        this.GetComponent<Renderer>().material.color = color;
                        
                        if(alpha <= 0f)
                        {
                            this.ballstate = BallState.ending;
                        }
                    }
                    else
                    {
                        if(this.BallEnterClip != null && this.isTracking && this.firstBounce)
                        {
                            AudioSource.PlayClipAtPoint(this.BallEnterClip, this.transform.position);
                        }
                        this.ballstate = BallState.entering;
                    }
                    break;
                    
                case BallState.ending:
                    this.transform.position = Vector3.up * 10;
                    this.GetComponent<Rigidbody>().useGravity = false;
                    this.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    this.ballstate = BallState.ended;
                    break;

                case BallState.ended:
                    if(this.Restart)
                    {
                        this.ballstate = BallState.starting;
                        this.Restart = false;
                    }
                    break;
            }
        }

        IEnumerator WaitForAudio()
        {
            BallCollision.BounceClipCount++;
            while(this.GetComponent<AudioSource>().isPlaying)
            {
                yield return null;
            }
            BallCollision.BounceClipCount--;
            yield return null;
        }

        IEnumerator KillPuff(GameObject puff, float time)
        {
            yield return new WaitForSeconds(time);
            Destroy(puff);
            yield return null;
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {	
            if(this.ballstate != BallState.entered ||
                collision.gameObject.name == "Sphere")
            {
                return;
            }

            if(this.BounceClip != null && !this.GetComponent<AudioSource>().isPlaying && BallCollision.BounceClipCount < BallCollision.MaxClipsPlaying)
            {
                this.GetComponent<AudioSource>().clip = this.BounceClip;
                this.GetComponent<AudioSource>().Play();
                StartCoroutine(WaitForAudio());
            }

            this.firstBounce = true;
            
            if(collision.contacts.Length > 0 && this.PuffEffect != null)
            {
                float hitPower = collision.relativeVelocity.magnitude;
                if(hitPower > 2.0f)
                {
                    Vector3 point = collision.contacts[0].point;
                    Quaternion normal = Quaternion.Euler(collision.contacts[0].normal);
                    GameObject puff = (GameObject)GameObject.Instantiate(this.PuffEffect, point, normal);
                    ParticleSystem ps = puff.GetComponent<ParticleSystem>();
                    ps.startSize = this.transform.localScale.x;
                    ps.startSpeed = hitPower * 0.125f;
                    StartCoroutine(KillPuff(puff, 0.3f));
                }
            }
        }
    }

}
