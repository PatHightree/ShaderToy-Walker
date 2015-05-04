using System.Collections.Generic;
using TBE_3DCore;
using UnityEngine;
using System.Collections;

public class SyncStart : MonoBehaviour {
	public AVProWindowsMediaMovie MoviePlayer;
	public List<AudioSource> AudioSources = new List<AudioSource>();
	public List<TBE_Source> TbeSources = new List<TBE_Source>();
	private bool _isPlaying;

	void Start() {
#if UNITY_EDITOR
		MoviePlayer._folder = "Builds/" + MoviePlayer._folder;
#endif
		//TbeSources.ForEach(s => s.SetActive(false));
	}
	void Update () {
		if (Input.GetKeyDown(KeyCode.Space)) {
			if (MoviePlayer.MovieInstance.IsPlaying)
				Stop();
			else
				Play();
		}
	}

	public void Play() {
		MoviePlayer.Play();
		AudioSources.ForEach(s => s.Play());
		TbeSources.ForEach(s => s.Play());
	}
	public void Stop() {
		MoviePlayer.MovieInstance.Pause();
		MoviePlayer.MovieInstance.Rewind();
		AudioSources.ForEach(s => s.Stop());
		TbeSources.ForEach(s => s.Stop());
	}
}
