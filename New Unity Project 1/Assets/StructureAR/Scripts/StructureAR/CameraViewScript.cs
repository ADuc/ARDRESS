/*
  This file is part of the Structure SDK.
  Copyright © 2015 Occipital, Inc. All rights reserved.
  http://structure.io
*/

using UnityEngine;
using System.Collections;

namespace StructureAR
{
    class OnPreRenderBehaviour : MonoBehaviour
    {
        protected void OnPreRender()
        {
            //this tells the StructureAR plugin to
            //prep the render buffer
            #if UNITY_IPHONE
            StructureARPlugin.CallPreRenderEvent();
            GL.InvalidateState();
            #endif
        }
    }
    
    public class CameraViewScript
    {

        public Texture2D CameraTexture;
        public GameObject CameraObject;

        private Mesh makeQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            Mesh mesh = new Mesh();
            mesh.name = "QuadMesh";
            //make quads verts
            Vector3[] vertices = new Vector3[4];
            vertices[0] = a;
            vertices[1] = b;
            vertices[2] = c;
            vertices[3] = d;
            mesh.vertices = vertices;
            
            //make quad's triangles
            int[] triangles = new int[6];
            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 2;
            
            triangles[3] = 0;
            triangles[4] = 2;
            triangles[5] = 3;
            mesh.triangles = triangles;
            
            //make triangle's UVs
            Vector2[] uv = new Vector2 [4];
            Vector2 e = new Vector2(1, 0);
            Vector2 f = new Vector2(1, 1);
            Vector2 g = new Vector2(0, 1);
            Vector2 h = new Vector2(0, 0);
            uv[0] = e;
            uv[1] = f;
            uv[2] = g;
            uv[3] = h;
            
            mesh.uv = uv;
            mesh.RecalculateNormals();
            return mesh;
        }
        
        public CameraViewScript(Camera camera, float distance)
        {
            distance = 1f;
            this.CameraObject = new GameObject("OrthoCamera");

            float fWidth = (float)(1 * camera.aspect);
            float fHeight = (float)(1);
            Vector3 a = new Vector3(fWidth, fHeight, distance);
            Vector3 b = new Vector3(fWidth, -fHeight, distance);
            Vector3 c = new Vector3(-fWidth, -fHeight, distance);
            Vector3 d = new Vector3(-fWidth, fHeight, distance);
            
            //add mesh filter
            MeshFilter meshFilter = this.CameraObject.gameObject.AddComponent<MeshFilter>() as MeshFilter;
            meshFilter.mesh = makeQuad(a, b, c, d);
            
            this.CameraTexture = new Texture2D(640, 480, TextureFormat.RGBA32, false);
            StructureARPlugin.setCameraTexture(this.CameraTexture.GetNativeTextureID());
            MeshRenderer meshRenderer = this.CameraObject.gameObject.AddComponent<MeshRenderer>() as MeshRenderer;

            Material material = new Material(Shader.Find("CameraVideo"));
            material.color = Color.white;
            meshRenderer.material = material;
            meshRenderer.material.mainTexture = this.CameraTexture;

            // this camera render the iPad camera, we don't want it interferes with other Unity objects, so we move the position very far away
            this.CameraObject.AddComponent<Camera>();
            this.CameraObject.transform.position = new Vector3(0, -1000, 0);
            this.CameraObject.transform.localScale = new Vector3(1,-1,1);
            this.CameraObject.GetComponent<Camera>().depth = -1;
            this.CameraObject.GetComponent<Camera>().clearFlags = CameraClearFlags.Skybox;
            this.CameraObject.GetComponent<Camera>().enabled = true;
            this.CameraObject.GetComponent<Camera>().orthographic = true;
            this.CameraObject.GetComponent<Camera>().orthographicSize = 1.0f;
            this.CameraObject.GetComponent<Camera>().farClipPlane = 1.1f;
            this.CameraObject.GetComponent<Camera>().nearClipPlane = 0.9f;

            this.CameraObject.AddComponent<OnPreRenderBehaviour>();
        }

    }
}
