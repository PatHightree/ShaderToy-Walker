Shader "ShaderToy walker/Simple test"
{    
    Properties
    {
    
    }
    SubShader
    {
        Tags { "Queue" = "Geometry" }
        Pass
            {            
                GLSLPROGRAM
                
				uniform vec2 iViewportOffset;
				uniform vec2 iViewportScale;
                uniform vec3 iCamRight;
                uniform vec3 iCamUp;
                uniform vec3 iCamForward;
                uniform vec3 iCamPos;
                
                #ifdef VERTEX  
                void main()
                {          
                    gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
                }
                #endif  
 
                #ifdef FRAGMENT
                #include "UnityCG.glslinc"
 
void main(void)
{
	vec2 p;
	p = iViewportOffset + iViewportScale * gl_FragCoord.xy / _ScreenParams.xy;
    p.x *= _ScreenParams.x/_ScreenParams.y;

	vec3 col = vec3(_ScreenParams.x, _ScreenParams.y, 0);
	
	gl_FragColor=vec4(col,1.0);
}

                #endif                          
                ENDGLSL        
            }
     }
    FallBack "Diffuse"
}