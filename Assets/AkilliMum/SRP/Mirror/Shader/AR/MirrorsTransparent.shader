Shader "AkilliMum/URP/AR/MirrorsTransparent"
{
    Properties
    {
        //new values
        [HideInInspector]_LeftOrCenterTexture("Reflection", 2D) = "white" { } //left or all
        _Noise("Noise", 2D) = "white" { }
        _ReflectionIntensity("Reflection Intensity", Float) = 0.5
        _WaveSize("Wave Size", Float) = 0
        _WaveSpeed("Wave Speed", Float) = 0
        _WaveDistortion("Wave Distortion", Float) = 0
    } 

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }
        LOD 300

        Pass
        {
            Name "ARMirrorsTransparent"
            Tags
            {
            }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _ALPHAPREMULTIPLY_ON

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex HiddenVertex
            #pragma fragment HiddenFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_LeftOrCenterTexture);       SAMPLER(sampler_LeftOrCenterTexture);
            TEXTURE2D(_Noise);       SAMPLER(sampler_Noise);
            float _ReflectionIntensity;
            float _WaveSize;
            float _WaveSpeed;
            float _WaveDistortion;

            struct Attributes
            {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 positionOS   : POSITION;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 screenPos    : TEXCOORD0;
                float3 positionWS   : TEXCOORD1;
            };

            Varyings HiddenVertex(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

                output.positionWS = vertexInput.positionWS;
                output.positionCS = vertexInput.positionCS;
                output.screenPos = ComputeScreenPos(vertexInput.positionCS);

                return output;
            }

            half4 HiddenFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 screenUV = (input.screenPos.xy) / (input.screenPos.w);

                //add waves
		        //half3 col_orig1 = SampleNormal(uv / _WaveSize + _WaveSpeed * _Time.y, TEXTURE2D_ARGS(_RivuletBump, sampler_RivuletBump), _BumpScale);
		        half3 col_orig1 = SAMPLE_TEXTURE2D (_Noise, sampler_Noise, 
                    screenUV / _WaveSize + _WaveSpeed * _Time.y);
                half3 col_orig2 = SAMPLE_TEXTURE2D (_Noise, sampler_Noise, 
                    screenUV / _WaveSize - _WaveSpeed * _Time.y);
                //SampleNormal(input.uv / _WaveSize + _WaveSpeed * _Time.y, TEXTURE2D_ARGS(_DropletMask, sampler_LinearRepeat), _BumpScale);
		        //SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv / _WaveSize + _WaveSpeed * _Time.y);
	        //half3 col_orig2 = SampleNormal(uv / _WaveSize - _WaveSpeed * _Time.y, TEXTURE2D_ARGS(_RivuletBump, sampler_RivuletBump), _BumpScale);
		        //half3 col_orig2 = SampleNormal(input.uv / _WaveSize - _WaveSpeed * _Time.y, TEXTURE2D_ARGS(_DropletMask, sampler_LinearRepeat), _BumpScale);
		        //SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv / _WaveSize - _WaveSpeed * _Time.y);

		        half3 wave1 = (col_orig1 * _WaveDistortion);
		        half3 wave2 = (col_orig2 * _WaveDistortion);

		        //surfaceData.normalTS = surfaceData.normalTS + (wave1 * wave2);
                float dis = distance(_WorldSpaceCameraPos, input.positionWS);
		        screenUV = screenUV + (wave1.xy * wave2.xy) * (1 / pow(dis,2));
               

                float4 reflection = SAMPLE_TEXTURE2D (_LeftOrCenterTexture, sampler_LeftOrCenterTexture, screenUV);
                
                //if pixel is black (reflection is not there; full transparent)
                if(reflection.r <= 0.001 && reflection.g <= 0.001 && reflection.b <= 0.001)
                    return half4(0, 0, 0, 0);
                else
                    return half4(reflection.rgb, _ReflectionIntensity);
            }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }

    FallBack "Hidden/InternalErrorShader"
}