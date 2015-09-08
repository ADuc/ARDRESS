/*
  This file is part of the Structure SDK.
  Copyright © 2015 Occipital, Inc. All rights reserved.
  http://structure.io
*/

using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.EventSystems;

namespace StructureAR
{
    static public class Menu
    {
		[MenuItem("StructureAR/Create/StructureAR Canvas", false, 1)]
		static void AddStructureARCanvas()
		{
			GameObject structureARCanvasObject = GameObject.Find ("Shared StructureAR Canvas");
			if (!structureARCanvasObject)
			{
				structureARCanvasObject = PrefabUtility.InstantiatePrefab(
						Resources.Load ("Prefabs/Shared StructureAR Canvas") as GameObject
					) as GameObject;
			}
	
			if (GameObject.FindObjectOfType<EventSystem>() == null)
			{
				GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem));
				eventSystem.AddComponent<StandaloneInputModule>();
			}
		}

        [MenuItem("StructureAR/Create/Manager", false, 1)]
        static void AddGameManager()
        {

            GameObject structureGameObject = GameObject.Find("Manager");
            if(structureGameObject == null)
            {
                structureGameObject = new GameObject("Manager");
            }

            Manager structureManagerComponent = structureGameObject.AddComponent<Manager>();
            structureManagerComponent.ShowDebugLog = false;

            GameObject mainCamera = GameObject.Find("Main Camera");
            if(mainCamera == null)
            {
                mainCamera = new GameObject("Main Camera");
               	structureManagerComponent.MainCamera = mainCamera.AddComponent<Camera>();
            }
            else
            {
                structureManagerComponent.MainCamera = mainCamera.GetComponent<Camera>();
            }
			// Create settings of Camera to be suitable for passthrough video.
			Camera cam = mainCamera.GetComponent<Camera>();
			Debug.Log ("Prepping Camera!");
			cam.projectionMatrix = Matrix4x4.Perspective(45.0f, 1.0f, 0.3f, 1000.0f); 
			cam.depth = 0;

            if(mainCamera.GetComponent<PinchToScale>() == null)
            {
                mainCamera.AddComponent<PinchToScale>();
            }

            Buttons sb = mainCamera.GetComponent<Buttons>();
            if(sb == null)
            {
                sb = mainCamera.AddComponent<Buttons>();
            }
        }

        [MenuItem("StructureAR/Create/GroundPlane", false, 2)]
        static void AddGroundPlane()
        {
            GameObject groundPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            groundPlane.name = "GroundPlane";
            groundPlane.transform.position = Vector3.zero;
            groundPlane.transform.localScale = new Vector3(10, 1, 10);
            GameObject.DestroyImmediate(groundPlane.GetComponent<MeshCollider>());
            BoxCollider bc = groundPlane.AddComponent<BoxCollider>();
            bc.center = new Vector3(0, -5, 0);
            bc.size = new Vector3(10, 10, 10);
            MeshRenderer meshRenderer = groundPlane.GetComponent<MeshRenderer>();
            meshRenderer.material = Resources.Load<Material>(@"Materials/TransparentInvisible");
        }
		
        [MenuItem("StructureAR/Documentation", false, 3)]
        static void Help()
        {
            Application.OpenURL("http://structure.io/developers");
        }				
    }
}
