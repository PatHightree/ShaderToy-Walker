Shader "ShaderToy walker/IQ Appolonian CG" {
	Properties {
	}
	SubShader {
		ZTest Always Cull Off ZWrite Off Fog { Mode Off }
		Pass {	
			CGPROGRAM

			ENDCG
		}
	} 
}
