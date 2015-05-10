using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Appolonian : MonoBehaviour {

	public Material material;
	public RenderTexture OutputRT;
	[Range(0, 1)]
	public float WorldScale=0.5f;
	[Range(12, 20)]
	public float FractalAnim=20;

	private float _animDeadzone = 45;
	private float _animSpeed = 0.05f;

	void Update () {
		material.SetVector("iCamPos", transform.position * WorldScale);
		material.SetVector("iCamRight", transform.right);
		material.SetVector("iCamUp", transform.up);
		material.SetVector("iCamForward", transform.forward);

		CameraRollToFractalAnim ();
		material.SetFloat ("iFracAnim", FractalAnim);
		Graphics.Blit (null, OutputRT, material);
	}

	void CameraRollToFractalAnim ()
	{
		float roll =  Vector3.Angle (transform.right, Vector3.up) - 90;
		float sign = Mathf.Sign (roll);
		float change = Mathf.Clamp (Mathf.Abs(roll) - _animDeadzone, 0, 90) * Time.deltaTime;
		change *= sign;
		FractalAnim -= change * _animSpeed;
		FractalAnim = Mathf.Clamp (FractalAnim, 12, 20);
	}
}
