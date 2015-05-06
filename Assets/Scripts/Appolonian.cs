using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Appolonian : MonoBehaviour {

	public Material material;
	public RenderTexture OutputRT;
	public Vector2 ViewportOffset = new Vector2(-1, -1);
	public Vector2 ViewportScale = new Vector2(2, 2);

	void Update () {
		material.SetVector("iViewportOffset", ViewportOffset);
		material.SetVector("iViewportScale", ViewportScale);
		material.SetVector("iCamPos", transform.position);
		material.SetVector("iCamRight", transform.right);
		material.SetVector("iCamUp", transform.up);
		material.SetVector("iCamForward", transform.forward);
		Graphics.Blit (null, OutputRT, material);
	}
}
