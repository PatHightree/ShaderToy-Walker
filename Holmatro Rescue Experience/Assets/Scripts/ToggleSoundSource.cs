using UnityEngine;
using System.Collections;

public class ToggleSoundSource : MonoBehaviour {
	public GameObject SoundSource;
	
	private bool _active;
	private Transform _centerEye;
	private Renderer _renderer;
	private Collider _collider;

	// Use this for initialization
	void Start () {
		_centerEye = GameObject.FindGameObjectWithTag("Center eye").transform;
		_renderer = GetComponent<Renderer>();
		_collider = GetComponent<Collider>();
	}

	private void Update() {
		Ray ray = new Ray(_centerEye.position, _centerEye.forward);
		RaycastHit hit = new RaycastHit();
		if (Physics.Raycast(ray, out hit) && hit.collider == _collider) {
			_renderer.material.color = _active ? Color.red : new Color(0.5f, 0, 0);
			if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Fire1")) {
				_active = !_active;
				SoundSource.SetActive(_active);
			}
		}
		else
			_renderer.material.color = _active ? Color.yellow : Color.gray;
	}

	void OnMouseDown() {
	}
}
