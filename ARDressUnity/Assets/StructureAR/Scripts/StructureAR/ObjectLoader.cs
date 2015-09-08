/*
  This file is part of the Structure SDK.
  Copyright © 2015 Occipital, Inc. All rights reserved.
  http://structure.io
*/

using UnityEngine;
using System.Collections;

namespace StructureAR
{
    /// <summary>
    /// Object loader.
    /// This is the base class for the object mesh that contains and manages
    /// the scanned mesh data from the plugin.
    /// You could add some additional functions or change the material that's
    /// added by overriding the LoadObject function
    /// </summary>
    public class ObjectLoader
    {
        // Mesh is a data container inside of the
        // MeshFilter object that's attached to the gameObject.
        // this class should be changed over to a monoBehaviour
        // so each gameObject can have it's own scanned mesh.
        // this would allow for multiple objects to be scanned into
        // a scene
        private Mesh objectMesh;

        public void ClearMesh(GameObject gameObject)
        {
            MeshFilter meshFilter = (MeshFilter)gameObject.GetComponent<MeshFilter>() as MeshFilter;
            if(meshFilter != null)
            {
                meshFilter.mesh.Clear();
            }
        }

        public IEnumerator LoadObject(GameObject gameObject)
        {
            //clear the old mesh and replace it with a new one
            this.objectMesh = new Mesh();

            // The LoadObject method is called from the manager
            // and we ask the plugin to update a gameObject's mesh
            StructureARPlugin.getMeshObj(ref this.objectMesh);

            if(this.objectMesh == null)
            {
                yield return null;
            }
            else
            {
                GameLog.Log("getting mesh object from plugin...");

                // check if there is allready a MeshFilter present, if not add one
                MeshFilter meshFilter = (MeshFilter)gameObject.GetComponent<MeshFilter>() as MeshFilter;

                //if we were not able to get the meshFilter component of this game object then
                //we shuld add one here
                if(meshFilter == null)
                {
                    //adding the mesh filter here
                    meshFilter = gameObject.AddComponent<MeshFilter>() as MeshFilter;
                }

                //make sure that the mesh on the object's mesh filter is this one.
                meshFilter.mesh = this.objectMesh;
                
                //check if there is allready a MeshRenderer present, if not add one
                MeshRenderer meshRenderer = (MeshRenderer)gameObject.GetComponent<MeshRenderer>() as MeshRenderer;
                if(meshRenderer == null)
                {
                    meshRenderer = gameObject.AddComponent<MeshRenderer>() as MeshRenderer;
                    Material mat = this.AssignMaterial(@"Materials/TransparentInvisible");
                    if(mat != null)
                    {
                        meshRenderer.material = mat;
                    }
                }

                //more of the same with the collider.
                MeshCollider meshCollider = (MeshCollider)gameObject.GetComponent<MeshCollider>() as MeshCollider;
                if(meshCollider == null)
                {
                    meshCollider = gameObject.AddComponent<MeshCollider>() as MeshCollider;
                    meshCollider.sharedMesh = meshFilter.mesh;
                }
                else
                {
                    meshCollider.sharedMesh = this.objectMesh;
                }

                this.objectMesh.RecalculateBounds();
                this.objectMesh.RecalculateNormals();

                yield return new WaitForFixedUpdate();
            }
        }

        public virtual Material AssignMaterial(string path)
        {
            Material m = Resources.Load<Material>(path);
            if(m != null)
            {
                return m;
            }
            else
            {
                GameLog.Log("no material found at path.");
                return null;
            }
        }
    }
}
