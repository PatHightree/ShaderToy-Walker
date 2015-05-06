Shader "ShaderToy walker/CG_test" {
	Properties {
		//_Color ("Color", Color) = (1,1,1,1)
		//_MainTex ("Render input", 2D) = "white" {}
		//_Glossiness ("Smoothness", Range(0,1)) = 0.5
		//_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		ZTest Always Cull Off ZWrite Off Fog { Mode Off }
		Pass {	
			CGPROGRAM
			
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			//sampler2D _MainTex;

			struct v2f {
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};
			
			v2f vert( appdata_img v ) 
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv =  v.texcoord.xy;
				return o;
			}
	
			fixed4 frag(v2f IN) : SV_Target {
				return half4(IN.uv, 0, 1);
			
//				half4 c = tex2D(_MainTex, IN.uv);
//				return c;
			}
			ENDCG
		}
	} 
}
