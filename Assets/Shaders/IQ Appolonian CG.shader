// Created by inigo quilez - iq/2013
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
//
// I can't recall where I learnt about this fractal.
//
// Coloring and fake occlusions are done by orbit trapping, as usual (unless somebody has invented
// something new in the last 4 years that i'm unaware of, that is)

// Unity note: This shader only works when Unity is running in OpenGL!
// To do this, start the editor with -force-opengl on the command line.
 
Shader "ShaderToy walker/IQ Appolonian CG" {
	Properties {
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
            uniform float3 iCamRight;
            uniform float3 iCamUp;
            uniform float3 iCamForward;
            uniform float3 iCamPos;
            uniform float iFracAnim;

			struct v2f {
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};
			
			v2f vert( appdata_img v ) 
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				// [0,1] to [-1,1]
				o.uv =  float2(-1,-1) + float2(2, 2) * v.texcoord.xy;
				// 16:10 viewport aspect ratio
				o.uv *= float2(1.6, 1.0);
				return o;
			}
	
			float4 orb; 
			float ss;
			float map( float3 p )
			{
				float scale = 1.0;

				orb = float4(1000.0, 1000.0, 1000.0, 1000.0); 
				
				for( int i=0; i<8;i++ )
				{
					p = -1.0 + 2.0*frac(0.5*p+0.5);

					float r2 = dot(p,p);
					
			        orb = min( orb, float4(abs(p),r2) );
					
					float k = max(ss/r2,0.1);
					p     *= k;
					scale *= k;
				}
				
				return 0.25*abs(p.y)/scale;
			}
			
			float trace( in float3 ro, in float3 rd )
			{
				float maxd = 100.0;
				float precis = 0.001;
			    float h=precis*2.0;
			    float t = 0.0;
			    for( int i=0; i<200; i++ )
			    {
			        if( abs(h)<precis||t>maxd ) continue;//break;
			        t += h;
				    h = map( ro+rd*t );
			    }

			    if( t>maxd ) t=-1.0;
			    return t;
			}

			float3 calcNormal( in float3 pos )
			{
				float3  eps = float3(.0001,0.0,0.0);
				float3 nor;
				nor.x = map(pos+eps.xyy) - map(pos-eps.xyy);
				nor.y = map(pos+eps.yxy) - map(pos-eps.yxy);
				nor.z = map(pos+eps.yyx) - map(pos-eps.yyx);
				return normalize(nor);
			}

			fixed4 frag(v2f IN) : SV_Target {
				float2 p;
				p = IN.uv;

				ss = 1.1 + 0.5*smoothstep( -0.3, 0.3, cos(0.1*iFracAnim) );
			    //ss = 1.1 + 0.5*smoothstep( -0.3, 0.3, cos(0.1*0) );
				
				// camera navigation
				float3 ro = iCamPos;
				float3 cu = iCamRight;
				float3 cv = iCamUp;
				float3 cw = iCamForward;
				
				float3 rd = normalize( p.x*cu + p.y*cv + 2.0*cw );


			    // trace	
				float3 col = float3(0, 0, 0);
				float t = trace( ro, rd );
				if( t>0.0 )
				{
					float4 tra = orb;
					float3 pos = ro + t*rd;
					float3 nor = calcNormal( pos );
					
					// lighting
			        float3  light1 = float3(  0.577, 0.577, -0.577 );
			        float3  light2 = float3( -0.707, 0.000,  0.707 );
					float key = clamp( dot( light1, nor ), 0.0, 1.0 );
					float bac = clamp( 0.2 + 0.8*dot( light2, nor ), 0.0, 1.0 );
					float amb = (0.7+0.3*nor.y);
					float ao = pow( clamp(tra.w*2.0,0.0,1.0), 1.2 );

					float3 brdf  = 1.0*float3(0.40,0.40,0.40)*amb*ao;
						 brdf += 1.0*float3(1.00,1.00,1.00)*key*ao;
						 brdf += 1.0*float3(0.40,0.40,0.40)*bac*ao;

			        // material		
					float3 rgb = float3(1,1,1);
					rgb = lerp( rgb, float3(1.0,0.80,0.2), clamp(6.0*tra.y,0.0,1.0) );
					rgb = lerp( rgb, float3(1.0,0.55,0.0), pow(clamp(1.0-2.0*tra.z,0.0,1.0),8.0) );

					// color
					col = rgb*brdf*exp(-0.2*t);
				}

				col = sqrt(col);
				
				col = lerp( col, smoothstep( 0.0, 1.0, col ), 0.25 );
				
				return half4(col,1.0);
				
				//return half4(IN.uv, 0, 1);
			
//				half4 c = tex2D(_MainTex, IN.uv);
//				return c;
			}
			ENDCG
		}
	} 
}
