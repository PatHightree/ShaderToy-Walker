// Support for Editor.RequiresConstantRepaint()
#if UNITY_4_5 || UNITY_4_6 || UNITY_5_0
#define AVPROWINDOWSMEDIA_UNITYFEATURE_EDITORAUTOREFRESH
#endif
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

//-----------------------------------------------------------------------------
// Copyright 2012-2015 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

[CustomEditor(typeof(AVProWindowsMediaMovie))]
public class AVProWindowsMediaMovieEditor : Editor
{
	private AVProWindowsMediaMovie _movie;
	private bool _showAlpha;

	#if AVPROWINDOWSMEDIA_UNITYFEATURE_EDITORAUTOREFRESH
	public override bool RequiresConstantRepaint()
	{
		return (_movie != null && _movie._editorPreview && _movie.MovieInstance != null);
	}
	#endif

#if UNITY_EDITOR_WIN
	private static void ShowInExplorer(string itemPath)
	{
		itemPath = System.IO.Path.GetFullPath(itemPath.Replace(@"/", @"\"));   // explorer doesn't like front slashes
		if (System.IO.File.Exists(itemPath))
		{
			System.Diagnostics.Process.Start("explorer.exe", "/select," + itemPath);
		}
	}
#endif

	public override void OnInspectorGUI()
	{
		_movie = (this.target) as AVProWindowsMediaMovie;
		
		EditorGUILayout.Separator();
		EditorGUILayout.LabelField("Load Options", EditorStyles.boldLabel);
		//DrawDefaultInspector();
		_movie._folder = EditorGUILayout.TextField("Folder", _movie._folder);
		_movie._filename = EditorGUILayout.TextField("Filename", _movie._filename);

#if UNITY_EDITOR_WIN

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel(" ");
		if (GUILayout.Button ("Show File"))
		{
			string filePath = Path.Combine(_movie._folder, _movie._filename);
			ShowInExplorer(filePath);
		}
		EditorGUILayout.EndHorizontal();
#endif
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Colour Format");
		_movie._colourFormat = (AVProWindowsMediaMovie.ColourFormat)EditorGUILayout.EnumPopup(_movie._colourFormat);
		EditorGUILayout.EndHorizontal();
		_movie._useDisplaySync = EditorGUILayout.Toggle("Use Display Sync", _movie._useDisplaySync);
		_movie._useAudioDelay = EditorGUILayout.Toggle("Use Audio Delay", _movie._useAudioDelay);
		_movie._useAudioMixer = EditorGUILayout.Toggle("Use Audio Mixer", _movie._useAudioMixer);
		
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		EditorGUILayout.LabelField("Start Options", EditorStyles.boldLabel);
		_movie._loadOnStart = EditorGUILayout.Toggle("Load On Start", _movie._loadOnStart);
		_movie._playOnStart = EditorGUILayout.Toggle("Play On Start", _movie._playOnStart);
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();

		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		EditorGUILayout.LabelField("Performance", EditorStyles.boldLabel);
		_movie._ignoreFlips = EditorGUILayout.Toggle("Ignore Flips", _movie._ignoreFlips);
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		
		_movie._loop = EditorGUILayout.Toggle("Loop", _movie._loop);
		//_movie._editorPreview = EditorGUILayout.Toggle("Editor Preview", _movie._editorPreview);		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Audio Volume");
		_movie._volume = EditorGUILayout.Slider(_movie._volume, 0.0f, 1.0f);
		EditorGUILayout.EndHorizontal();
		
		
		GUILayout.Space(8.0f);

        SerializedProperty tps = serializedObject.FindProperty("_clips");
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(tps, new GUIContent("Clips"), true);
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }

        GUILayout.Space(8.0f);

		AVProWindowsMedia media = _movie.MovieInstance;
		if (media != null)
		{
			GUI.enabled = (_movie != null && _movie.MovieInstance != null);
			_movie._editorPreview = EditorGUILayout.Foldout(_movie._editorPreview, "Video Preview");

			GUI.enabled = true;
			if (_movie._editorPreview && _movie.MovieInstance != null)
			{
				{
					Texture texture = _movie.OutputTexture;
					if (texture == null)
						texture = EditorGUIUtility.whiteTexture;

					float ratio = (float)texture.width / (float)texture.height;


					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					Rect textureRect = GUILayoutUtility.GetRect(Screen.width/2, Screen.width/2, (Screen.width / 2) / ratio, (Screen.width / 2) / ratio);
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();

					_showAlpha = GUILayout.Toggle(_showAlpha, "Show Alpha Channel");

					Matrix4x4 prevMatrix = GUI.matrix;
					if (_movie.MovieInstance.RequiresFlipY)
					{
						GUIUtility.ScaleAroundPivot(new Vector2(1f, -1f), new Vector2(0, textureRect.y + (textureRect.height / 2)));
					}

					if (!_showAlpha)
						GUI.DrawTexture(textureRect, texture, ScaleMode.ScaleToFit);
					else
						EditorGUI.DrawTextureAlpha(textureRect, texture, ScaleMode.ScaleToFit);

					GUI.matrix = prevMatrix;
				
					GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Select Texture", GUILayout.ExpandWidth(false)))
					{
						Selection.activeObject = texture;
					}
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();

					if (Application.isPlaying)
					{			
						GUILayout.Label(string.Format("{0}x{1} @ {2}fps {3} secs", media.Width, media.Height, media.FrameRate.ToString("F2"), media.DurationSeconds.ToString("F2")));		
					}
					
					if (media.FramesTotal > 30)
					{
						GUILayout.Label("Displaying at " + media.DisplayFPS.ToString("F1") + " fps");
					}
					else
					{
						GUILayout.Label("Displaying at ... fps");	
					}
				}
			
				if (Application.isPlaying && _movie.enabled)
				{
					if (media != null)
					{
						GUILayout.Space(8.0f);
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.PrefixLabel("Audio Balance");
						media.AudioBalance = EditorGUILayout.Slider(media.AudioBalance, -1.0f, 1.0f);
						EditorGUILayout.EndHorizontal();
						
						EditorGUILayout.LabelField("Frame:");
						EditorGUILayout.BeginHorizontal();
						if (GUILayout.Button("<", GUILayout.ExpandWidth(false)))
						{
							media.PositionFrames--;
						}
						uint currentFrame = media.PositionFrames;
						if (currentFrame != uint.MaxValue)
						{
							int newFrame = EditorGUILayout.IntSlider((int)currentFrame, 0, (int)media.DurationFrames);
							if (newFrame != currentFrame)
							{
								media.PositionFrames = (uint)newFrame;
							}
						}
						if (GUILayout.Button(">", GUILayout.ExpandWidth(false)))
						{
							media.PositionFrames++;
						}
						EditorGUILayout.EndHorizontal();

                        if (_movie.NumClips > 0)
                        {
                            EditorGUILayout.Separator();
                            EditorGUILayout.LabelField("Clips", EditorStyles.boldLabel);
                            for (int i = 0; i < _movie.NumClips; i++)
                            {
                                GUILayout.BeginHorizontal();
                                string clipName = _movie.GetClipName(i);
                                GUILayout.Label(clipName);
                                if (GUILayout.Button("Loop"))
                                {
                                    _movie.PlayClip(clipName, true);
                                }
                                GUILayout.EndHorizontal();
                            }
                            if (GUILayout.Button("Reset Clip"))
                                _movie.ResetClip();
                            EditorGUILayout.Separator();
                        }

						if (!media.IsPlaying)
						{
							if (GUILayout.Button("Unpause Stream"))
							{
								_movie.Play();
							}						
						}
						else
						{
							if (GUILayout.Button("Pause Stream"))
							{
								_movie.Pause();
							}
						}
#if !AVPROWINDOWSMEDIA_UNITYFEATURE_EDITORAUTOREFRESH
						this.Repaint();
#endif
					}
				}
			}
		}

		if (GUI.changed)
		{
			EditorUtility.SetDirty(_movie);
		}		
	}
}