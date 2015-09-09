/*
  This file is part of the Structure SDK.
  Copyright © 2015 Occipital, Inc. All rights reserved.
  http://structure.io
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace StructureAR
{
    public struct HitPoint
    {
        public Vector3 point;
        public List<Vector3> vertices;
        public float stopRange;
        public float currentRange;
    }

    public class Edge
    {
        public Edge(Vector3 _a, Vector3 _b)
        {
            a = _a;
            b = _b;

        }
        public Vector3 a { get; set; }
        public Vector3 b { get; set; }
    }

    // Custom comparer for the Edge class
    public class EdgeComparer : IEqualityComparer<Edge>
    {
        public bool Equals(Edge x, Edge y)
        {
            
            //Check whether any of the compared objects is null.
            if(Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;
            
            //Check if edge contains the same two vertices
            return (x.a == y.a && x.b == y.b) || (x.a == y.b && x.b == y.a);
        }

        public int GetHashCode(Edge edge)
        {
            //Check whether the object is null
            if(Object.ReferenceEquals(edge, null))
                return 0;
            
            //Get hash code for vertex a
            int hashEdgeA = edge.a.GetHashCode();
            
            //Get hash code for vertex b
            int hashEdgeB = edge.b.GetHashCode();
            
            //Calculate the hash code for the edge
            return hashEdgeA ^ hashEdgeB;
        }

    }

    public class Wireframe : MonoBehaviour
    {

        public Color lineColor = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        public float hitRange = 0.2f;
        public Vector3[] Vertices;
        public float DecayRate = 0.96f;
        public float initialStartRange = 0.25f;
        public float initialStopRange = 0.1f;
        
        private Material lineMaterial;
        private List<HitPoint> HitList;
        private HashSet<Edge> edgeSet;
        private List<Edge> edgeArray;

        private float gameScale = 1.0f;
        public bool TestHit;
        public bool TestReset;

        void Start()
        {
            this.ClearMesh();
            this.ConstructMesh();
            this.HitList = new List<HitPoint>();
            this.lineMaterial = Resources.Load<Material>(@"Materials/WireframeMaterial");

            Manager.StructureARGameEvent += HandleStructureARGameEvent;
            PinchToScale.TouchEvent += HandleScaleEvent;
			GameLog.Log(" ----------------Wireframe----Start------------------------------------ ");
        }

        void HandleStructureARGameEvent(object sender, GameEventArgs args)
        {
            switch(args.gameState)
            {
                case SensorState.Reset:
                    break;
                case SensorState.Playing:
                    break;
            }
        }

        void HandleScaleEvent(ScaleEventArgs args)
        {
            this.gameScale = args.scale;
        }

        public void ClearMesh()
        {
            this.Vertices = null;
            this.isMeshComplete = false;

            if (this.HitList != null)
			{
                this.HitList.Clear();
			}
        }

        private bool isMeshComplete;
        public void ConstructMesh()
        {
            MeshFilter filter = gameObject.GetComponent<MeshFilter>();
            while(filter == null)
            {
                return;
            }
			GameLog.Log(" ----------------Wireframe----ConstructMesh------------------------------------ ");
            // Generate set of all edges, don't repeat edges
            this.Vertices = filter.mesh.vertices;
            int[] triangles = filter.mesh.triangles;
            HashSet<Edge> edgeSet = new HashSet<Edge>(new EdgeComparer());
            for(int i = 0; i+2 < triangles.Length; i+=3)
            {
                Vector3 a = this.Vertices[triangles[i]];
                Vector3 b = this.Vertices[triangles[i + 1]];
                Vector3 c = this.Vertices[triangles[i + 2]];

                Edge first = new Edge(a, b);
                Edge second = new Edge(b, c);
                Edge third = new Edge(c, a);
                
                edgeSet.Add(first);
                edgeSet.Add(second);
                edgeSet.Add(third);
            }

            // Populate an array of all the edges
            edgeArray = new List<Edge>();
            foreach(Edge e in edgeSet)
            {
                edgeArray.Add (e);
            }

            this.isMeshComplete = true;

            GameLog.Log("Construction complete");


            VisualizeMesh();

            return;
        }

        Vector3 ToWorld(Vector3 vec)
        {
            return gameObject.transform.TransformPoint(vec);
        }

        void Update()
        {
            if(this.TestHit)
            {
                this.AddHitPoint(Vector3.zero, 0.3f, 0.1f);
                this.TestHit = false;
            }

            if(this.TestReset)
            {
                this.ClearMesh();
                if(!this.isMeshComplete)
                {
                    this.ConstructMesh();
                }
                this.TestReset = false;
            }
        }

        public void VisualizeMesh()
        {
            // Use only the first collision point as mesh effect seed
            float startRange = 15.0f * this.gameScale;
            float stopRange = 0.2f * this.gameScale;

            AddHitPoint(new Vector3(0.0f, 0.0f, 0.0f), startRange, stopRange);
        }

        protected void OnCollisionEnter(Collision collision)
        {
            // Use only the first collision point as mesh effect seed
            float startRange = this.initialStartRange * this.gameScale;
            float stopRange = this.initialStopRange * this.gameScale;
            
            this.AddHitPoint(collision.contacts[0].point, startRange, stopRange);
        }

        public void AddHitPoint(Vector3 point, float startRange, float endRange)
        {
            if(!this.isMeshComplete)
            {
                return;
            }   
            HitPoint hp = new HitPoint();
            hp.point = point;
            
            hp.stopRange = endRange;
            hp.currentRange = startRange;
            
            List<Vector3> verticesInRange = new List<Vector3>();
            foreach(Edge e in edgeArray)
            {   
                float distance = (ToWorld(e.a) - hp.point).magnitude;
                if(distance < hp.currentRange)
                {
                    verticesInRange.Add(ToWorld(e.a));
                    verticesInRange.Add(ToWorld(e.b));
                }               
            }           
            hp.vertices = verticesInRange;
            
            this.HitList.Add(hp);
            
        }
		bool flag = true;
        void OnRenderObject()
        {
            if(!this.isMeshComplete)
            {
                return;
            }

            if(lineMaterial== null)
            {
                GameLog.Log("no line material");
                return;
            }

            if(HitList == null)
                return;
            if(HitList.Count <= 0)
                return;
			if (flag) {
				GameLog.Log (" ----------------Wireframe----OnRenderObject------------------------------------ ");
				flag = false;
			}
            lineMaterial.SetPass(0);
            GL.Color(lineColor);
            GL.Begin(GL.LINES);

            for(int i = 0; i < this.HitList.Count; i++)
            {
                HitPoint hp = this.HitList[i];
                for(int h = 0; h < hp.vertices.Count; h+=2)
                {
                    Vector3 a = hp.vertices[h];
                    float distance = (a - hp.point).magnitude;
                    if(distance < hp.currentRange)
                    {
                        Vector3 b = hp.vertices[h + 1];
                        GL.Vertex(a);
                        GL.Vertex(b);
                    }
                    else 
                    {
                        this.HitList[i].vertices.RemoveRange(h, 2);
                    }
                }

                hp.currentRange *= this.DecayRate;

                this.HitList[i] = hp;

                if(hp.currentRange < hp.stopRange)
                {
                    this.HitList.RemoveAt(i);
                }

            }
            GL.End();
        }
    }
}