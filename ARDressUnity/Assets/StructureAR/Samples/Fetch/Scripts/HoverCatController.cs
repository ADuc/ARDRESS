/*
  This file is part of the Structure SDK.
  Copyright © 2015 Occipital, Inc. All rights reserved.
  http://structure.io
*/

using UnityEngine;
using System.Collections;
using StructureAR;

namespace HoverCat
{
    /// <summary>
    /// Hover cat controller.
    /// used to move the cat around and do tricks!
    /// react when the cat hits the ball as well.
    /// also plays sounds and particle effects.
    /// </summary>
    public class HoverCatController : HoverCatObject
    {
        #region adjustableParameters
        //objects
        public GameObject Cat;
        public Camera mCamera;
        public GameObject BlobShadow;
        public GameObject HopPuff;

        //values
        public bool SuperHop;
        public float speedBoost = 5f;
        public float hoverSpeed = 15f;
        public float flipSpeed = 25f;
        public float hopPowerupTime = 3.0f;
        public float HopHeight = 5f;
        public float HopBonus = 2f;
        #endregion

        #region AudioClips
        public AudioClip HardBump;
        public AudioClip JumpNormal;
        public AudioClip JumpSuper;
        public AudioClip CatReset;
        public AudioClip PowerupGet;
        public AudioClip PowerDown;
        public AudioClip CatFlip;
        public AudioClip CatSpeedBoost;
        public AudioClip NormalBoost;
        #endregion

        #region privateLocals
        private float HopPower;
        private float HopTimer;
        private bool FlippingCat;
        private Quaternion LastRotation;
        private int FlipCount;
        private float TimeBetweenHops = 0.25f;
        private bool isMovable;
        private bool isTracking;
        #endregion

        #region INIT_OVERRIDES
        // Use this for initialization
        protected override void Start()
        {
            base.Start();

            if(this.GetComponent<ParticleSystem>() != null)
            {
                this.GetComponent<ParticleSystem>().startColor = Color.magenta;
                GameLog.Log(this.ToString() + this.GetComponent<ParticleSystem>().ToString());
            }

            if(this.Cat != null)
            {
                this.Cat.transform.parent = null;
            }

            if(this.BlobShadow != null)
            {
                //use a standardasset blob projector, didn't want to include a
                //blob shadow projector in this packaged set of assets for this
                //this example.
                this.BlobShadow.transform.parent = null;
            }

            this.HopPower = this.HopHeight;
            GameLog.Log(this.ToString() + " started.");
        }
        #endregion

        #region EVENTCOMMAND_OVERRIDES

        public override void Freeze()
        {
            base.Freeze();
            this.isMovable = false;
            GameLog.Log(this, "Freezing hover cat.");
        }

        public override void Hide()
        {
            base.Hide();
            this.Cat.SetActive(false);//hide child object
            this.GetComponent<ParticleSystem>().Stop();
            GameLog.Log(this, "Hiding hover cat.");
        }

        public override void Home()
        {
            base.Home();
            this.Cat.transform.position = this.HomePosition; 
            this.PositionCatAndShadow(this.HomePosition);
            GameLog.Log(this, "Sending hovercat to hiding position");
        }

        public override void Show()
        {
            base.Show();
            this.Cat.SetActive(true);
            this.GetComponent<ParticleSystem>().Play();
            this.isMovable = true;
            GameLog.Log(this, "Showing hover cat.");
        }
        #endregion

        #region COLLISION_EVENTS
        protected virtual void OnCollisionEnter(Collision collision)
        {
            BallPickup bp = collision.gameObject.GetComponent<BallPickup>();
            if(bp != null)
            {
                GameLog.Log(this, "got powerup!");
                this.StartHopPowerup();
				bp.Home();//tell the powerup to find a new place to fall.
            }
            this.LandCat();
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            Hoop hoop = other.gameObject.GetComponent<Hoop>();
            if(hoop != null)
            {
                GameLog.Log(hoop, "jumped through a hoop!!!");
            }
        }
        
        private void LandCat()
        {
            if(this.HopPuff != null)
            {
                Vector3 tadBelow = this.gameObject.transform.position;
                tadBelow.y -= 0.03f;
                GameObject.Instantiate(this.HopPuff, tadBelow, Quaternion.identity);
            }

            if(this.HardBump != null)
            {
                AudioSource.PlayClipAtPoint(this.HardBump, this.gameObject.transform.position);
            }
            this.Cat.transform.rotation = this.LastRotation;
            this.FlippingCat = false;
            this.FlipCount = 0;
        }
        #endregion

        #region MovementControls
        // Update is called once per frame
        void Update()
        {
            if(this.isMovable)
            {
                this.MoveHoverCat();
            }
        }

        private void MoveHoverCat()
        {
            Vector3 target = this.gameObject.transform.position;
            Vector3 position = this.gameObject.transform.position;
            this.HopTimer -= Time.deltaTime;
            bool moveToTouchTarget = false;

            if(Input.touchCount > 0)
            {
                switch(Input.touches[0].phase)
                {
                    case TouchPhase.Began:
                        this.PlayBoost(Input.touches[0].tapCount > 1);
                        break;

                    case TouchPhase.Stationary:
                    case TouchPhase.Moved:

                        Ray ray = this.mCamera.ScreenPointToRay(Input.touches[0].position);
                        RaycastHit[] hits = Physics.RaycastAll(this.mCamera.transform.position, ray.direction);

                        float distance = Mathf.Infinity;
                        foreach(RaycastHit hit in hits)
                        {
                            if(hit.distance < distance)
                            {
                                target = hit.point;
                                if(this.Cat != null)
                                {
                                    target.y = this.Cat.transform.position.y + 0.05f;
                                    Vector3 lookAt = position - target;
                                    this.Cat.transform.rotation = Quaternion.LookRotation(lookAt == Vector3.zero ? Vector3.forward : lookAt);
                                    if(!this.FlippingCat)
                                    {
                                        this.LastRotation = this.Cat.transform.rotation;
                                    }
                                }
                                target = hit.point;
                            }
                        }
                        moveToTouchTarget = true;
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        moveToTouchTarget = false;
                        break;
                }
            }

            //adding a second finger or mre will make the hovercat hop
            if(Input.touchCount > 1)
            {
                this.Hop();
            }

            if(moveToTouchTarget)
            {
                this.Push(position, (target - position).normalized, target, Input.touches[0].tapCount > 1);
            }

            if(this.FlippingCat)
            {
                this.Cat.transform.Rotate(new Vector3(this.flipSpeed, 0, 0), Space.Self);
            }

            this.PositionCatAndShadow(position);
        }

        protected void PositionCatAndShadow(Vector3 position)
        {
            Vector3 pos = position;
            pos.y -= 0.05f;
            this.Cat.transform.position = pos;
            
            if(this.BlobShadow != null)
            {
                Vector3 blobPosition = position;
                blobPosition.y += 0.25f;
                this.BlobShadow.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
                this.BlobShadow.transform.position = blobPosition;
            }
        }

        //adds a force to the hovercat to move him
        public void Push(Vector3 position, Vector3 dir, Vector3 target, bool doubleTap)
        {

            float distance = (position - target).magnitude;
            float minMax = distance > 1.5f ? 1.5f : distance < 1.0f ? 1.0f : distance;

            if(doubleTap && this.SuperHop)
            {
                float multiplier = this.speedBoost * minMax;
                this.GetComponent<Rigidbody>().AddForce(dir * multiplier, ForceMode.Force);
            }
            else
            {
                float multiplier = this.hoverSpeed * minMax;
                this.GetComponent<Rigidbody>().AddForce(dir * multiplier, ForceMode.Force);
            }
        }

        //adds a hop up force to the hovercat
        public void Hop()
        {
            //prevent flying
            if(this.HopTimer < 0.0f && this.FlipCount < 2)
            {
                this.GetComponent<Rigidbody>().AddForce(new Vector3(0, this.HopPower, 0), ForceMode.Impulse);
                this.HopTimer = this.TimeBetweenHops;
                this.FlippingCat = true;
                this.FlipCount++;

                AudioSource.PlayClipAtPoint(this.SuperHop ? this.JumpSuper : this.JumpNormal, this.gameObject.transform.position);
            }
        }
        #endregion

        #region GameEffects
        public void StartHopPowerup()
        {
            StartCoroutine(HopPowerup(this.hopPowerupTime));
        }

        private IEnumerator HopPowerup(float powerupTime)
        {
            this.GetComponent<ParticleSystem>().startColor = Color.green;
            this.GetComponent<ParticleSystem>().emissionRate = 50;
            this.HopPower = this.HopHeight + this.HopBonus;
            this.SuperHop = true;

            if(this.PowerupGet != null)
            {
                AudioSource.PlayClipAtPoint(this.PowerupGet, this.gameObject.transform.position);
            }

            yield return new WaitForSeconds(powerupTime);

            this.GetComponent<ParticleSystem>().startColor = Color.magenta;
            this.GetComponent<ParticleSystem>().emissionRate = 30;
            this.HopPower = this.HopHeight;
            this.SuperHop = false;

            if(this.PowerupGet != null)
            {
                AudioSource.PlayClipAtPoint(this.PowerDown, this.gameObject.transform.position);
            }
        }
        #endregion

        #region SoundEffects
        public void PlayBoost(bool super)
        {
            if(this.SuperHop && super)
            {
                AudioSource.PlayClipAtPoint(this.CatSpeedBoost, this.gameObject.transform.position);
            }
            else
            {
                AudioSource.PlayClipAtPoint(this.NormalBoost, this.gameObject.transform.position);
            }
        }
        #endregion

    }
}
