using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Appolonian : MonoBehaviour {

	public Material material;
	public RenderTexture OutputRT;

	void Update () {
		material.SetVector("iCamPos", transform.position);
		material.SetVector("iCamRight", transform.right);
		material.SetVector("iCamUp", transform.up);
		material.SetVector("iCamForward", transform.forward);
		Graphics.Blit (null, OutputRT, material);
	}
}
