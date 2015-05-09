using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Appolonian : MonoBehaviour {

	public Material material;
	public RenderTexture OutputRT;
	[Range(0, 1)]
	public float WorldScale=0.5f;

	void Update () {
		material.SetVector("iCamPos", transform.position * WorldScale);
		material.SetVector("iCamRight", transform.right);
		material.SetVector("iCamUp", transform.up);
		material.SetVector("iCamForward", transform.forward);
		Graphics.Blit (null, OutputRT, material);
	}
}
