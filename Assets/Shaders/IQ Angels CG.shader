Shader "ShaderToy walker/IQ Angels CG" {
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
	
			
			
			// Created by inigo quilez - iq/2013
			// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.


			float hash( float n )
			{
			    return frac(sin(n)*1751.5453);
			}

			float3 hash3( float n )
			{
			    return frac(sin(float3(n,n+1.0,n+2.0))*float3(43758.5453123,22578.1459123,19642.3490423));
			}

			// ripped from Kali's Lonely Tree shader
			float3x3 rotationMat(float3 v, float angle)
			{
			    float c = cos(angle);
			    float s = sin(angle);
			    //return float3x3(c + (1.0 - c) * v.x * v.x, (1.0 - c) * v.x * v.y - s * v.z, (1.0 - c) * v.x * v.z + s * v.y,
			    //            (1.0 - c) * v.x * v.y + s * v.z, c + (1.0 - c) * v.y * v.y, (1.0 - c) * v.y * v.z - s * v.x,
			    //            (1.0 - c) * v.x * v.z - s * v.y, (1.0 - c) * v.y * v.z + s * v.x, c + (1.0 - c) * v.z * v.z);
			    return float3x3(c + (1.0 - c) * v.x * v.x, (1.0 - c) * v.x * v.y + s * v.z, (1.0 - c) * v.x * v.z - s * v.y,
			                (1.0 - c) * v.x * v.y - s * v.z, c + (1.0 - c) * v.y * v.y, (1.0 - c) * v.y * v.z + s * v.x,
			                (1.0 - c) * v.x * v.z + s * v.y, (1.0 - c) * v.y * v.z - s * v.x, c + (1.0 - c) * v.z * v.z);
			}

			float3 axis = normalize( float3(-0.3,-1.,-0.4) );

			float2 map( float3 p )
			{
			    // animation
			    float atime = _Time[1]+12.0;
			    float2 o = floor( 0.5 + p.xz/50.0  );
			    float o1 = hash( o.x*57.0 + 12.1234*o.y );
			    float f = sin( 1.0 + (2.0*atime + 31.2*o1)/2.0 );
			    p.y -= 2.0*(atime + f*f);
			    p = fmod( (p+25.0)/50.0, 1.0 )*50.0-25.0;
			    if( abs(o.x)>0.5 )  p += (-1.0 + 2.0*o1)*10.0;
			    float3x3 roma = rotationMat(axis, 0.34 + 0.07*sin(31.2*o1+2.0*atime + 0.1*p.y) );
			    
			    // modelingt
			    for( int i=0; i<16; i++ ) 
			    {
			        p = mul(abs(p), roma);
			        p.y-= 1.0;
			    }
			    float d = length(p*float3(1.0,0.1,1.0))-0.75;
			    float h = 0.5 + p.z;
			    return float2( d, h );
			}

			float3 intersect( in float3 ro, in float3 rd )
			{
			    float maxd = 140.0;
			    float precis = 0.001;
			    float h=precis*2.0;
			    float t = 0.0;
			    float d = 0.0;
			    float m = 1.0;
			    for( int i=0; i<128; i++ )
			    {
			        if( abs(h)<precis||t>maxd ) break;
			        
			        t += h;
			        float2 res = map( ro+rd*t );
			        h = 0.8*min( res.x, 5.0 );
			        d = res.y;
			    }
			    if( t>maxd ) m=-1.0;
			    return float3( t, d, m );
			}

			float3 calcNormal( in float3 pos )
			{
			    float3 eps = float3(0.2,0.0,0.0);
			    return normalize( float3(
			        map(pos+eps.xyy).x - map(pos-eps.xyy).x,
			        map(pos+eps.yxy).x - map(pos-eps.yxy).x,
			        map(pos+eps.yyx).x - map(pos-eps.yyx).x ) );
			}

			float softshadow( in float3 ro, in float3 rd, float mint, float k )
			{
			    float res = 1.0;
			    float t = mint;
			    for( int i=0; i<48; i++ )
			    {
			        float h = map(ro + rd*t).x;
			        h = max( h, 0.0 );
			        res = min( res, k*h/t );
			        t += clamp( h, 0.01, 0.5 );
			    }
			    return clamp(res,0.0,1.0);
			}

			float calcAO( in float3 pos, in float3 nor )
			{
			    float totao = 0.0;
			    for( int aoi=0; aoi<16; aoi++ )
			    {
			        float3 aopos = -1.0+2.0*hash3(float(aoi)*213.47);
			        aopos *= sign( dot(aopos,nor) );
			        aopos = pos + aopos*0.5;
			        float dd = clamp( map( aopos ).x*4.0, 0.0, 1.0 );
			        totao += dd;
			    }
			    totao /= 16.0;
			    return clamp( totao*totao*1.5, 0.0, 1.0 );
			}

			float3 lig = normalize(float3(-0.5,0.7,-1.0));


			float3 render( in float3 ro, in float3 rd )
			{
			    // render
			    float3 bgc = 0.6*float3(0.8,0.9,1.0)*(0.5 + 0.3*rd.y);
			    float3 col = bgc;
			    
			    // raymarch
			    float3 tmat = intersect(ro,rd);
			    if( tmat.z>-0.5 )
			    {
			        // geometry
			        float3 pos = ro + tmat.x*rd;
			        float3 nor = calcNormal(pos);
			        
			        // material
			        float3 mate = 0.5 + 0.5*lerp( sin( float3(1.2,1.1,1.0)*tmat.y*3.0 ),
			                                  sin( float3(1.2,1.1,1.0)*tmat.y*6.0 ),
			                                  1.0-abs(nor.y) );
			        
			        // lighting
			        float occ = calcAO( pos, nor );
			        float amb = 0.8 + 0.2*nor.y;
			        float dif = max(dot(nor,lig),0.0);
			        float bac = max(dot(nor,normalize(float3(-lig.x,0.0,-lig.z))),0.0);
			        float sha = 0.0; if( dif>0.001 ) sha=softshadow( pos, lig, 0.1, 32.0 );
			        float fre = pow( clamp( 1.0 + dot(nor,rd), 0.0, 1.0 ), 2.0 );
			        
			        // lights
			        float3 brdf = float3(0,0,0);
			        brdf += 1.0*dif*float3(1.00,0.90,0.65)*pow(float3(sha,sha,sha),float3(1.0,1.2,1.5));
			        brdf += 1.0*amb*float3(0.05,0.05,0.05)*occ;
			        brdf += 1.0*bac*float3(0.03,0.03,0.03)*occ;
			        brdf += 1.0*fre*float3(1.00,0.70,0.40)*occ*(0.2+0.8*sha);
			        brdf += 1.0*occ*float3(1.00,0.70,0.30)*occ*max(dot(-nor,lig),0.0)*pow(clamp(dot(rd,lig),0.0,1.0),64.0)*tmat.y*2.0;
			        
			        // surface-light interacion
			        col = mate * brdf;
			        
			        // fogt
			        col = lerp( col, bgc, clamp(1.0-1.2*exp(-0.0002*tmat.x*tmat.x ),0.0,1.0) );
			    }
			    else
			    {
			        // sunt
			        float3 sun = float3(1.0,0.8,0.5)*pow( clamp(dot(rd,lig),0.0,1.0), 32.0 );
			        col += sun;
			    }
			    
			    // sun scatter
			    col += 0.6*float3(0.2,0.14,0.1)*pow( clamp(dot(rd,lig),0.0,1.0), 5.0 );
			    
			    // postprocessing
			    
			    // gamma
			    col = pow( col, float3(0.45,0.45,0.45) );
			    
			    // contrast/brightness
			    col = 1.3*col-0.1;
			    
			    // tint
			    col *= float3( 1.0, 1.04, 1.0);

			    return col;
			}

			fixed4 frag(v2f IN) : SV_Target {
				float2 p;
				p = IN.uv;
			    float2 m = float2(0.5,0.5);
			    //if( iMouse.z>0.0 ) m = iMouse.xy/iResolution.xy;
			    
			    // camera
			    float an = 2.5 + 0.12*_Time[1] - 6.2*m.x;
			    float cr = 0.3*cos(0.2*_Time[1]);
			    float3 ro = float3(15.0*sin(an),12.0-24.0*m.y,15.0*cos(an));
			    float3 ta = float3( 0.0, 2.0, 0.0 );
			    float3 ww = normalize( ta - ro );
			    float3 uu = normalize( cross(ww,float3(sin(cr),cos(cr),0.0) ) );
			    float3 vv = normalize( cross(uu,ww));
			    float r2 = p.x*p.x*0.32 + p.y*p.y;
			    p *= (7.0-sqrt(37.5-11.5*r2))/(r2+1.0);
			    float3 rd = normalize( p.x*uu + p.y*vv + 1.5*ww );
			    
			    float3 col = render( ro, rd );
			    
			    // vigneting
			    //col *= pow( 16.0*q.x*q.y*(1.0-q.x)*(1.0-q.y), 0.1 );
			    
			    return float4( col, 1.0 );
			}



			void mainVR( out float4 fragColor, in float2 fragCoord, in float3 fragRayOri, in float3 fragRayDir )
			{
			    float3 ro = fragRayOri + float3( 3.0, 0.0, 14.0 );
			    float3 rd = fragRayDir;

			    float3 col = render( ro, rd );
			    
			    fragColor = float4( col, 1.0 );
			}

			ENDCG
		}
	} 
}
