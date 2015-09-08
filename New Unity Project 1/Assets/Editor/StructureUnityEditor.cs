using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Text;
using System.Xml;
using System.IO;
using System.Threading;

using Debug = UnityEngine.Debug;

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using PlistEntry = System.Collections.Generic.KeyValuePair<string, UnityEditor.iOS.Xcode.PlistElement>; 

[InitializeOnLoad]
public class StructurePlugin : MonoBehaviour
{
	// See: http://forum.unity3d.com/threads/4-6-ios-64-bit-beta.290551/page-11
	enum IOSArchitectures : int
	{
		Armv7 = 0,
		Arm64 = 1,
		Universal = 2,
	};

	static StructurePlugin ()
	{
		// Set the iOS target version to iOS 8.
		PlayerSettings.iOS.targetOSVersion = iOSTargetOSVersion.iOS_8_0;

		// Select universal (armv7 & arm64) as the target architecture.
		PlayerSettings.SetPropertyInt("Architecture", (int)IOSArchitectures.Universal, BuildTargetGroup.iOS);

		EditorApplication.update += Update;
	}
	
	static void Update ()
	{
		// Called once per frame, by the editor.
	}
	
	[PostProcessBuildAttribute]
	public static void OnPostprocessBuild (BuildTarget target, string pathToBuiltProject)
	{
		if (target != BuildTarget.iOS)
			return;

		Debug.Log("Structure framework iOS post process build script for build project: " + pathToBuiltProject);

		// Unity Version Checking
		#if UNITY_4_0
		Debug.Log ("Unity 4 not supported by Structure SDK 0.5");
		#endif
		
		if (Application.unityVersion.StartsWith("5.0.0"))
			Debug.Log ("Structure SDK only supports Unity 5.0.1+");

	}

	
	[MenuItem ("Structure Sensor/Website...")]
	static void Launch ()
	{
		Application.OpenURL("http://structure.io");
	}
}

#endif
