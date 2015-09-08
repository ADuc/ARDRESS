/*
  This file is part of the Structure SDK.
  Copyright © 2015 Occipital, Inc. All rights reserved.
  http://structure.io
*/

using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;
using StructureAR;

// Define the attribute that will be used to tag static methods as callbacks, so that they will be compiled AOT and callable from iOS apps running on the il2cpp backend.
[AttributeUsage (AttributeTargets.Method)]
public sealed class MonoPInvokeCallbackAttribute : Attribute
{
	public MonoPInvokeCallbackAttribute (Type t) {}
}

public class StructureARPlugin
{
    /* Interface to native implementation */
    public static Vector3 cameraPosition;
    public static Quaternion cameraRotation;
    public static Vector3 cameraScale;
    public static Matrix4x4 projectionMatrix;
    public static float[] depthBuffer;

    public delegate void StructureARPluginEventHandler(object sender,StructureARPluginEventArgs args);
    public static StructureARPluginEventHandler StructureEvent;
    
    [DllImport ("__Internal")]
    private static extern void _initStructureAR();
    
    [DllImport ("__Internal")]
    private static extern bool _isStructureConnected();
    
    [DllImport ("__Internal")]
    private static extern ScannerState _getScannerState();
    
    [DllImport ("__Internal")]
    private static extern void _startScanning();
    
    [DllImport ("__Internal")]
    private static extern void _doneScanning();
    
    [DllImport ("__Internal")]
    private static extern void _resetScanning();
    
    [DllImport ("__Internal")]
    private static extern void _setCameraTexture(int tex);

    [DllImport ("__Internal")]
    private static extern void _handlePinchScale(float scale);
    
    [DllImport ("__Internal")]
    private static extern void _UnityPreRenderEvent();
    
    [DllImport ("__Internal")]
    private static extern void _UnityPostRenderEvent();
    
    [DllImport ("__Internal")]
    private static extern void _getMeshObj(ref MeshObjPlugin mesh);

	// Callback types.    
	public delegate void Callback_void ();
	public delegate void Callback_bool (bool arg0);
	public delegate void Callback_IntPtr (IntPtr arg0);
	public delegate void Callback_IntPtr_int_int (IntPtr arg0, int arg1, int arg2);
	public delegate void Callback_float_float_float (float arg0, float arg1, float arg2);
	public delegate void Callback_float_float_float_float (float arg0, float arg1, float arg2, float arg3);
	
	[DllImport ("__Internal")]
	private static extern void _setCallbacks (
		Callback_void                    notifySensorConnected,
		Callback_void                    notifySensorDisConnected,
		Callback_void                    notifyCameraAccessRequired,
		Callback_void                    notifySensorNeedsCharging,
		Callback_bool                    notifyTrackingStatus,
		Callback_IntPtr_int_int          setDepthBuffer,
		Callback_float_float_float       setCameraPosition,
		Callback_float_float_float_float setCameraRotation,
		Callback_float_float_float       setCameraScale,
		Callback_IntPtr                  setCameraProjectionMatrix,
		Callback_void                    notifyMeshReady
	);

    /// <summary>
    /// The plugin will call this function when the sensor is plugged into your ios device
    /// </summary>

	[MonoPInvokeCallback (typeof (Callback_void))]
    public static void notifySensorConnected()
    {
        if (StructureARPlugin.StructureEvent != null)
        {
            StructureARPlugin.StructureEvent(new object(), new StructureARPluginEventArgs(StructureARPluginEvent.SensorConnected));
        }
    }
    
    /// <summary>
    /// The plugin will call this when the sensor is disconnected.
    /// the usual action is to disable your camera movement updates.
    /// </summary>
	[MonoPInvokeCallback (typeof (Callback_void))]
    public static void notifySensorDisConnected()
    {
        if (StructureARPlugin.StructureEvent != null)
        {
            StructureARPlugin.StructureEvent(new object(), new StructureARPluginEventArgs(StructureARPluginEvent.SensorDisconnected));
        }
    }

    /// <summary>
    /// If the color camera access is not granted when the app starts up
    /// then you need to know how to access the color camera on the iOS device
    /// </summary>
	[MonoPInvokeCallback (typeof (Callback_void))]
    public static void notifyCameraAccessRequired()
    {
        if (StructureARPlugin.StructureEvent != null)
        {
            StructureARPlugin.StructureEvent(new object(), new StructureARPluginEventArgs(StructureARPluginEvent.CameraAccessRequired));
        }
    }

    /// <summary>
    /// when the battery runs low or when it's plugged in and it's battery is running low this will
    /// be called, you should handle having the sensor plugged in, but not be able to stream
    /// positional data from it
    /// </summary>
	[MonoPInvokeCallback (typeof (Callback_void))]
    public static void notifySensorNeedsCharging()
    {
        if (StructureARPlugin.StructureEvent != null)
        {
            StructureARPlugin.StructureEvent(new object(), new StructureARPluginEventArgs(StructureARPluginEvent.SensorNeedsCharging));
        }
    }
    
    /// <summary>
    /// this function will be called by the sensor on every new frame the plugin gets updating you with a new
    /// status. if the tracking is good then you'll get a trackingOK true, if it's not good then
    /// the update will come in saying trackingOK false.
    /// </summary>
    /// <param name="trackingOK">If set to <c>true</c> tracking O.</param>
	[MonoPInvokeCallback (typeof (Callback_bool))]
    public static void notifyTrackingStatus(bool trackingOK)
    {
        if (StructureARPlugin.StructureEvent != null)
        {
            if (trackingOK)
            {
                StructureARPlugin.StructureEvent(new object(), new StructureARPluginEventArgs(StructureARPluginEvent.TrackingFound));
            } else
            {
                StructureARPlugin.StructureEvent(new object(), new StructureARPluginEventArgs(StructureARPluginEvent.TrackingLost));
            }
        }
    }

    public static bool depthReady;

	[MonoPInvokeCallback (typeof (Callback_IntPtr_int_int))]
    public static void setDepthBuffer(IntPtr pointer, int width, int height)
    {
        depthReady = false;
        depthBuffer = new float[width * height];
        Marshal.Copy(pointer, depthBuffer, 0, width * height);
        depthReady = true;
    }
    
    /// <summary>
    /// the plug in updates this function with new vectors every time it's got an update
    /// to share with unity this happens in the updateCameraPoseUnity:(GLKMatrix4)m method
    /// in the StructureAR.mm file
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="z">The z coordinate.</param>
	[MonoPInvokeCallback (typeof (Callback_float_float_float))]
    public static void setCameraPosition(float x, float y, float z)
    {
        StructureARPlugin.cameraPosition = new Vector3(x, y, z);
    }
    
    /// <summary>
    /// quaternion returned from the plugin
    /// the conversion for this happens in StructureAR.mm int he updateCameraPoseUnity method
    /// </summary>
    /// <param name="qx">Qx.</param>
    /// <param name="qy">Qy.</param>
    /// <param name="qz">Qz.</param>
    /// <param name="qw">Qw.</param>
	[MonoPInvokeCallback (typeof (Callback_float_float_float_float))]
    public static void setCameraRotation(float qx, float qy, float qz, float qw)
    {
        StructureARPlugin.cameraRotation = new Quaternion(qx, qy, qz, qw);
    }
    
    /// <summary>
    /// same updates from the StructureAR.mm methods
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="z">The z coordinate.</param>
	[MonoPInvokeCallback (typeof (Callback_float_float_float))]
    public static void setCameraScale(float x, float y, float z)
    {
        StructureARPlugin.cameraScale = new Vector3(x, y, z);
    }

    /// <summary>
    /// not sure if this is called. actually, pretty sure that we can ditch this method since it's not called anywhere.
    /// </summary>
    /// <param name="pointer">Pointer.</param>
	[MonoPInvokeCallback (typeof (Callback_IntPtr))]
    public static void setCameraProjectionMatrix(IntPtr pointer)
    {
        float[] matrix = new float[16];
        
        Marshal.Copy(pointer, matrix, 0, 16);
        
        projectionMatrix.m00 = matrix[0];
        projectionMatrix.m10 = matrix[1];
        projectionMatrix.m20 = matrix[2];
        projectionMatrix.m30 = matrix[3];
        
        projectionMatrix.m01 = matrix[4];
        projectionMatrix.m11 = matrix[5];
        projectionMatrix.m21 = matrix[6];
        projectionMatrix.m31 = matrix[7];
        
        projectionMatrix.m02 = matrix[8];
        projectionMatrix.m12 = matrix[9];
        projectionMatrix.m22 = matrix[10];
        projectionMatrix.m32 = matrix[11];
        
        projectionMatrix.m03 = matrix[12];
        projectionMatrix.m13 = matrix[13];
        projectionMatrix.m23 = matrix[14];
        projectionMatrix.m33 = matrix[15];
        
        if(StructureARPlugin.StructureEvent != null)
        {
            StructureARPlugin.StructureEvent(new object(), new StructureARPluginEventArgs(StructureARPluginEvent.UpdateProjectionMatrix));
        }
    }

    /// <summary>
    /// When the event is sent to the plugin that the scanning is complete,
    /// the plugin begins to copy the mesh data into the unity scene, when
    /// the copy is finished this method is called so that you can know
    /// when to display the scanned mesh
    /// </summary>
	[MonoPInvokeCallback (typeof (Callback_void))]
    public static void notifyMeshReady()
    {
        if(StructureARPlugin.StructureEvent != null)
        {
            StructureARPlugin.StructureEvent(new object(), new StructureARPluginEventArgs(StructureARPluginEvent.ScannedMeshReady));
        }
    }

	/* Public interface for use inside C# / JS code */
	
	public StructureARPlugin()
	{
		StructureARPlugin.cameraPosition = new Vector3(0, 0, 0);
		StructureARPlugin.cameraRotation = new Quaternion(0, 0, 0, 0);
		StructureARPlugin.cameraScale = new Vector3(1, 1, 1);
	}

	public static void setCallbacks ()
	{
		if (Application.platform != RuntimePlatform.IPhonePlayer)
			return;

		_setCallbacks(
			notifySensorConnected,
			notifySensorDisConnected,
			notifyCameraAccessRequired,
			notifySensorNeedsCharging,
			notifyTrackingStatus,
			setDepthBuffer,
			setCameraPosition,
			setCameraRotation,
			setCameraScale,
			setCameraProjectionMatrix,
			notifyMeshReady
		);
	}

    // CallBack from unity to plugin
    // optional functions that Unity can call from the plugin
    // to get position rotation and scale.
    public Vector3 getCameraPosition()
    {   
        return StructureARPlugin.cameraPosition;
    }
    
    public Quaternion getCameraRotation()
    {
        return StructureARPlugin.cameraRotation;
    }
    
    public Vector3 getCameraScale()
    {
        return StructureARPlugin.cameraScale;
    }
    
    public static void initStructureAR(Matrix4x4 defaultProjectionMatrix)
    {
        StructureARPlugin.projectionMatrix = defaultProjectionMatrix;
        // Call plugin only when running on real device
        if(Application.platform == RuntimePlatform.IPhonePlayer)
        {
            _initStructureAR();
        }
    }
    
    public static bool isStructureConnected()
    {
        if(Application.platform == RuntimePlatform.IPhonePlayer)
        {
            return _isStructureConnected();
        }
        else
        {
            return false;
        }
    }
    
    public static ScannerState getScannerState()
    {
        // Call plugin only when running on real device
        if(Application.platform == RuntimePlatform.IPhonePlayer)
        {
            return _getScannerState();
        }
        else
        {
            return ScannerState.ScannerStateUnknown;
        }
    }
    
    public static void startScanning()
    {
        // Call plugin only when running on real device
        if(Application.platform == RuntimePlatform.IPhonePlayer)
        {
            _startScanning();
        }
    }
    
    public static void resetScanning()
    {
        // Call plugin only when running on real device
        if(Application.platform == RuntimePlatform.IPhonePlayer)
        {
            _resetScanning();
        }
    }
    
    public static void doneScanning()
    {
        // Call plugin only when running on real device
        if(Application.platform == RuntimePlatform.IPhonePlayer)
        {
            _doneScanning();
        }
    }
    
    public static void handlePinchScale(float scale)
    {
        if(Application.platform == RuntimePlatform.IPhonePlayer)
        {
            _handlePinchScale(scale);
        }
    }
    
    public static void CallPreRenderEvent()
    {
        if(Application.platform == RuntimePlatform.IPhonePlayer)
        {
            _UnityPreRenderEvent();
        }
    }
    
    public static void CallPostRenderEvent()
    {
        if(Application.platform == RuntimePlatform.IPhonePlayer)
        {
            _UnityPostRenderEvent();
        }
    }
    
    struct MeshObjPlugin
    {
        public IntPtr vertices;
        public IntPtr normals;
        public IntPtr triangles;
        public int numberOfMeshVertices;
        public int numberOfMeshFaces;
        public float meshOffsetX;
        public float meshOffsetY;
        public float meshOffsetZ;
    };
    
    static void convertVertices(ref float[] verticesIn, ref Vector3[] verticesOut, Vector3 verticesOffset)
    {
        int length = verticesIn.Length / 3;
        verticesOut = new Vector3[length];
        for(int i = 0; i < length; i++)
        {
            verticesOut[i] = new Vector3( (verticesIn[i * 3] - verticesOffset.x),
                                         -(verticesIn[i * 3 + 1] - verticesOffset.y),
                                          (verticesIn[i * 3 + 2] - verticesOffset.z));
        }
    }
    
    static void convertNormals(ref float[] normalsIn, ref Vector3[] normalsOut)
    {
        int length = normalsIn.Length / 3;
        normalsOut = new Vector3[length];
        for(int i = 0; i < length; i++)
        {
            normalsOut[i] = new Vector3((normalsIn[i * 3]),
                                        -(normalsIn[i * 3 + 1]),
                                        (normalsIn[i * 3 + 2]));
        }
    }

    static void convertFaces(ref short[] facesIn, ref int[] facesOut)
    {
        int numFaces = facesIn.Length/3;
        
        facesOut = new int[numFaces*3];
        
        for(int i = 0; i < numFaces; i++)
        {
            facesOut[3*i  ] = facesIn[3*i+2];
            facesOut[3*i+1] = facesIn[3*i+1];
            facesOut[3*i+2] = facesIn[3*i  ];
        }
    }
    
    static void convertMeshObj(ref MeshObjPlugin meshObjPlugin, ref Mesh mesh)
    {
        int numVertices = meshObjPlugin.numberOfMeshVertices;
        int numFaces = meshObjPlugin.numberOfMeshFaces;

        if(numVertices == 0 || numFaces == 0)
        {
            mesh = null;
            Debug.Log("Mesh doesn't contain any vertices or faces!");
            return;
        }
        
        float[] tmpVertices = new float[numVertices * 3];
        float[] tmpNormals = new float[numVertices * 3];
        short[] tmpTriangles = new short[numFaces * 3];
        
        Marshal.Copy(meshObjPlugin.vertices, tmpVertices, 0, numVertices * 3);
        Marshal.Copy(meshObjPlugin.normals, tmpNormals, 0, numVertices * 3);
        Marshal.Copy(meshObjPlugin.triangles, tmpTriangles, 0, numFaces * 3);
        
        Vector3[] vertices = null;
        Vector3[] normals = null;
        int[] triangles = null;
        
        convertVertices(ref tmpVertices, ref vertices, 
                    new Vector3(meshObjPlugin.meshOffsetX, 
                                meshObjPlugin.meshOffsetY, 
                                meshObjPlugin.meshOffsetZ));

        convertNormals(ref tmpNormals, ref normals);
        
        convertFaces(ref tmpTriangles, ref triangles);
        
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.triangles = triangles;
    }
    
    /// <summary>
    /// this is called after the mesh ready notification from the ObjectLoader
    /// class where the mesh is assigned to the gameObject's meshfilter
    /// component attached to the GameObject.
    /// </summary>
    /// <param name="mesh">Mesh.</param>
    public static void getMeshObj(ref Mesh mesh)
    {
        if(Application.platform == RuntimePlatform.IPhonePlayer)
        {
            MeshObjPlugin meshObjPlugin = new MeshObjPlugin();

            // this calls the plugin for the latest mesh once we're done
            // scanning the scene for a mesh.
            _getMeshObj(ref meshObjPlugin);
            
            // this is the rather slow method call which can take a second or two
            // to marshal copy form the plugin to the unity scene
            convertMeshObj(ref meshObjPlugin, ref mesh);
        }
        else
        {
            Debug.Log("not supported");
        }
    }

    public static void setCameraTexture(int tex)
    {
        if(Application.platform == RuntimePlatform.IPhonePlayer)
        {
            _setCameraTexture(tex);
        }
    }
}
