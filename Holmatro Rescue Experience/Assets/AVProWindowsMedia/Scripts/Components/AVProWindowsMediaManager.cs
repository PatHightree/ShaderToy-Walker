// Support for using externally created native textures, from Unity 4.2 upwards
#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0
	#define AVPROWINDOWSMEDIA_UNITYFEATURE_EXTERNALTEXTURES
#endif

// Support for DirectX and OpenGL native texture updating, from Unity 4.0 upwards
#if UNITY_5_0 || UNITY_4_6 || UNITY_4_5 || UNITY_4_4 || UNITY_4_3 || UNITY_4_2 || UNITY_4_1 || UNITY_4_0_1 || UNITY_4_0
	#define AVPRO_UNITY_4_X
#endif

// Support for linear gamma, from Unity 3.5 upwards
#if AVPRO_UNITY_4_X || UNITY_3_5
	#define AVPRO_UNITY_LINEARGAMMA
#endif

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System;
using System.Text;
using System.Runtime.InteropServices;

//-----------------------------------------------------------------------------
// Copyright 2012-2015 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

[AddComponentMenu("AVPro Windows Media/Manager (required)")]
public class AVProWindowsMediaManager : MonoBehaviour
{
	private static AVProWindowsMediaManager _instance;

	// Format conversion
	public Shader _shaderBGRA32;
	public Shader _shaderYUY2;
	public Shader _shaderYUY2_709;
	public Shader _shaderUYVY;
	public Shader _shaderYVYU;
	public Shader _shaderHDYC;
	public Shader _shaderNV12;
	public Shader _shaderCopy;
	public Shader _shaderHap_YCoCg;
	
	private bool _isInitialised;
	[HideInInspector] public bool _useExternalTextures = false;
	
	//-------------------------------------------------------------------------

	public static AVProWindowsMediaManager Instance  
	{
		get
		{
			if (_instance == null)
			{
				_instance = (AVProWindowsMediaManager)GameObject.FindObjectOfType(typeof(AVProWindowsMediaManager));
				if (_instance == null)
				{
					Debug.LogError("AVProWindowsMediaManager component required");
					return null;
				}
				else
				{
					if (!_instance._isInitialised)
						_instance.Init();
				}
			}
			
			return _instance;
		}
	}
	
	//-------------------------------------------------------------------------
	
	void Awake()
	{
		if (!_isInitialised)
		{
			_instance = this;
			Init();
		}
	}
	
	void OnDestroy()
	{
		Deinit();
	}
			
	protected bool Init()
	{
		try
		{
			if (AVProWindowsMediaPlugin.Init())
			{
				Debug.Log("[AVProWindowsMedia] version " + AVProWindowsMediaPlugin.GetPluginVersion().ToString("F2") + " initialised");
			}
			else
			{
				Debug.LogError("[AVProWindowsMedia] failed to initialise.");
				this.enabled = false;
				Deinit();
				return false;
			}
		}
		catch (System.DllNotFoundException e)
		{
			Debug.Log("[AVProWindowsMedia] Unity couldn't find the DLL, did you move the 'Plugins' folder to the root of your project?");
			throw e;
		}

		GetConversionMethod();
		SetUnityFeatures();
		
//		StartCoroutine("FinalRenderCapture");
		_isInitialised = true;

		return _isInitialised;
	}

	private void SetUnityFeatures()
	{
		#if !AVPROWINDOWSMEDIA_UNITYFEATURE_EXTERNALTEXTURES
		_useExternalTextures = false;
		#endif
		AVProWindowsMediaPlugin.SetUnityFeatures(_useExternalTextures);
	}

	private void GetConversionMethod()
	{
		bool swapRedBlue = false;
		if (SystemInfo.graphicsDeviceVersion.StartsWith("Direct3D 11"))
        {
#if UNITY_5_0
			// DX11 has red and blue channels swapped around
			if (!SystemInfo.SupportsTextureFormat(TextureFormat.BGRA32))
				swapRedBlue = true;
#else
            swapRedBlue = true;
#endif
        }

		if (swapRedBlue)
		{
			Shader.DisableKeyword("SWAP_RED_BLUE_OFF");
			Shader.EnableKeyword("SWAP_RED_BLUE_ON");
		}
		else
		{
			Shader.DisableKeyword("SWAP_RED_BLUE_ON");
			Shader.EnableKeyword("SWAP_RED_BLUE_OFF");
		}
		
		Shader.DisableKeyword("AVPRO_GAMMACORRECTION");
		Shader.EnableKeyword("AVPRO_GAMMACORRECTION_OFF");
#if AVPRO_UNITY_LINEARGAMMA
		if (QualitySettings.activeColorSpace == ColorSpace.Linear)
		{
			Shader.DisableKeyword("AVPRO_GAMMACORRECTION_OFF");
			Shader.EnableKeyword("AVPRO_GAMMACORRECTION");
		}
#endif	
	}
	
	private IEnumerator FinalRenderCapture()
	{
		while (Application.isPlaying)
		{				
			GL.IssuePluginEvent(AVProWindowsMediaPlugin.PluginID | (int)AVProWindowsMediaPlugin.PluginEvent.UpdateAllTextures);
			yield return new WaitForEndOfFrame();
		}
	}

	void Update()
	{
		GL.IssuePluginEvent(AVProWindowsMediaPlugin.PluginID | (int)AVProWindowsMediaPlugin.PluginEvent.UpdateAllTextures);
	}
	
	public void Deinit()
	{
		// Clean up any open movies
		AVProWindowsMediaMovie[] movies = (AVProWindowsMediaMovie[])FindObjectsOfType(typeof(AVProWindowsMediaMovie));
		if (movies != null && movies.Length > 0)
		{
			for (int i = 0; i < movies.Length; i++)
			{
				movies[i].UnloadMovie();
			}
		}
		
		_instance = null;
		_isInitialised = false;
		
		AVProWindowsMediaPlugin.Deinit();
	}

	public Shader GetPixelConversionShader(AVProWindowsMediaPlugin.VideoFrameFormat format, bool useBT709)
	{
		Shader result = null;
		switch (format)
		{
		case AVProWindowsMediaPlugin.VideoFrameFormat.YUV_422_YUY2:
			result = _shaderYUY2;
			if (useBT709)
				result = _shaderYUY2_709;
			break;
		case AVProWindowsMediaPlugin.VideoFrameFormat.YUV_422_UYVY:
			result = _shaderUYVY;
			if (useBT709)
				result = _shaderHDYC;
			break;
		case AVProWindowsMediaPlugin.VideoFrameFormat.YUV_422_YVYU:
			result = _shaderYVYU;
			break;
		case AVProWindowsMediaPlugin.VideoFrameFormat.YUV_422_HDYC:
			result = _shaderHDYC;
			break;
		case AVProWindowsMediaPlugin.VideoFrameFormat.YUV_420_NV12:
			result = _shaderNV12;
			break;
		case AVProWindowsMediaPlugin.VideoFrameFormat.Hap_RGB:
			result = _shaderCopy;
			break;
		case AVProWindowsMediaPlugin.VideoFrameFormat.Hap_RGBA:
			result = _shaderCopy;
			break;
		case AVProWindowsMediaPlugin.VideoFrameFormat.Hap_RGB_HQ:
			result = _shaderHap_YCoCg;
			break;
		case AVProWindowsMediaPlugin.VideoFrameFormat.RAW_BGRA32:
			result= _shaderBGRA32;
			break;
		default:
			Debug.LogError("[AVProWindowsMedia] Unknown pixel format '" + format);
			break;
		}
		return result;
	}
}