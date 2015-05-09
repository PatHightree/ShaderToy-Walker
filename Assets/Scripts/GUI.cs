using UnityEngine;
using System.Collections;

public class GUI : MonoBehaviour {
	public float DoubleClickInterval = 0.3f;
	private float _lastClickTime = -1;
	private bool _areSettingsVisible;
	private GameObject _settingsPanel;
	
	public void Start() {
		_settingsPanel = GameObject.FindGameObjectWithTag("Settings panel");
		_settingsPanel.SetActive(false);
	}


	void Update () {
		if (Input.GetButtonDown("Fire1")) {
			if (Time.time - _lastClickTime < DoubleClickInterval) {
				//double click
				print("done:" + (Time.time - _lastClickTime).ToString());
				_areSettingsVisible = !_areSettingsVisible;
				_settingsPanel.gameObject.SetActive(_areSettingsVisible);
			} else {
				//normal click
				print("miss:" + (Time.time - _lastClickTime).ToString());
			}
			_lastClickTime = Time.time;
		}	
	}
}
