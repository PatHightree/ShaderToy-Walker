using DG.Tweening;
using UnityEngine;
using System.Collections;

public class CrashTumble : MonoBehaviour {
	public float Duration = 3;
	public float Speed = 5;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown("Fire2")) {
			StartCoroutine(DoRotation());
		}
	}

	private IEnumerator DoRotation() {
		Vector3 axis = new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
		float time = 0;

		while (time < Duration) {
			//transform.rotation = Quaternion.identity;
			transform.RotateAround(transform.position, axis, Speed);
			time += Time.deltaTime;
			yield return true;
		}
	}
}
