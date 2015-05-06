using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CG_test : MonoBehaviour {
	
	public Material material;
	public Texture2D testTexture;
	public Vector2 ViewportOffset = new Vector2(-1, -1);
	public Vector2 ViewportScale = new Vector2(2, 2);
	
	// Called by the camera to apply the image effect
	void OnRenderImage (RenderTexture source, RenderTexture destination) {
		material.SetTexture ("_MainTex", testTexture);
//		material.SetVector("iViewportOffset", ViewportOffset);
//		material.SetVector("iViewportScale", ViewportScale);
//		material.SetVector("iCamPos", transform.position);
//		material.SetVector("iCamRight", transform.right);
//		material.SetVector("iCamUp", transform.up);
//		material.SetVector("iCamForward", transform.forward);
		Graphics.Blit (source, destination, material);
	}
}
