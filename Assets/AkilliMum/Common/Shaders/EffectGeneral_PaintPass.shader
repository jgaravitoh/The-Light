// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "AkilliMum/Particles/Effect/General_PaintPass" 
{

	Properties 
	{
		[ToggleUI] _UseScreenTex("Use Opaque Texture", Float) = 0.0
		
		[ToggleUI] _UseTempTex("Use Temp Textures", Float) = 0.0
		
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		
		_MainTex ("Particle Texture", 2D) = "white" {}
		
		_ColorStrength ("Color strength", Float) = 1.0
		
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0

		//[Header(Blending)]
		// https://docs.unity3d.com/ScriptReference/Rendering.BlendMode.html
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlendEx("_SrcBlendEx (default = SrcAlpha)", Float) = 5 // 5 = SrcAlpha
		[Enum(UnityEngine.Rendering.BlendMode)]_DstBlendEx("_DstBlendEx (default = OneMinusSrcAlpha)", Float) = 10 // 10 = OneMinusSrcAlpha


		//[Header(ZTest)]
		// https://docs.unity3d.com/ScriptReference/Rendering.CompareFunction.html
		// default need to be Disable, because we need to make sure decal render correctly even if camera goes into decal cube volume, although disable ZTest by default will prevent EarlyZ (bad for GPU performance)
		[Enum(UnityEngine.Rendering.CompareFunction)]_ZTestEx("_ZTest (default = Disable or LessEqual", Float) = 0 //0 = disable

		//[Header(Cull)]
		// https://docs.unity3d.com/ScriptReference/Rendering.CullMode.html
		// default need to be Front, because we need to make sure decal render correctly even if camera goes into decal cube
		[Enum(UnityEngine.Rendering.CullMode)]_CullEx("_Cull (default = Back)", Float) = 2 //1 = Front, 2 Back

		[Toggle(_ZWriteEx)] _ZWriteEx("_ZWriteEx (default = On)", Float) = 1
	}

Category {
	Tags
	{
		//"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" 
		"Queue" = "Transparent"
		"IgnoreProjector" = "True"
		"RenderType" = "Transparent"
		"LightMode" = "CarPaint"
	}
	Blend[_SrcBlendEx][_DstBlendEx]
	ZWrite[_ZWriteEx]
	Cull[_CullEx]
	/*Blend SrcAlpha One
	Cull Off */
	Lighting Off 
	/*ZWrite Off */
	Fog { Mode Off}
	
	SubShader {
		Pass {

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_particles

			#include "UnityCG.cginc"

			float _UseScreenTex;
			float _UseTempTex;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _TintColor;
			fixed _ColorStrength;
			float _InvFade;

			sampler2D _CameraDepthTexture;
		
			float4 _TempOpaqueTexture_ST;
			sampler2D _TempOpaqueTexture;

			sampler2D _CameraOpaqueTexture;

			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 uv : TEXCOORD0;
				//#ifdef SOFTPARTICLES_ON
				float4 projPos : TEXCOORD1;
				//#endif
			};
		
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.projPos = ComputeScreenPos (o.vertex);
				#ifdef SOFTPARTICLES_ON
				COMPUTE_EYEDEPTH(o.projPos.z);
				#endif
				o.color = v.color;
				o.uv = TRANSFORM_TEX(v.uv,_MainTex);
				return o;
			}

			float rand(float2 uv) {
 
				float a = dot(uv, float2(92., 80.));
				float b = dot(uv, float2(41., 62.));
    
				float x = sin(a) + cos(b) * 51.;
				return frac(x);
			}
			
			fixed4 frag (v2f i) : COLOR
			{
				#ifdef SOFTPARTICLES_ON
				float sceneZ = LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos))));
				float partZ = i.projPos.z;
				float fade = saturate (_InvFade * (sceneZ-partZ));
				i.color.a *= fade;
				#endif

				if(_UseScreenTex > 0.5)
				{
					float2 screenCoord = ((i.projPos.xy) / (i.projPos.w)) * _ScreenParams.xy;
					float2 screenUV = screenCoord.xy / _ScreenParams.xy;
					float4 alpha;
					if(_UseTempTex > 0.5)
					{
						alpha = tex2D( _TempOpaqueTexture, screenUV);
					}
					else
					{
						alpha = tex2D( _CameraOpaqueTexture, screenUV);
					}
					//float4 alpha = tex2D( _CameraOpaqueTexture, screenUV);
					//return alpha;
					//gray based fraction
					float grayColor = dot(alpha.rgb, float3(0.3, 0.59, 0.11));
					//float2 rnd = float2(rand(iuv+alpha.r), rand(iuv+alpha.g)) *  grayColor;
					float2 rnd1 = float2(rand(alpha.r), rand(alpha.g));
					float2 rnd2 = float2(rand(alpha.b), rand(alpha.r));
					// float2 rnd1 = float2(rand(iuv.r*.005), rand(iuv.g*.001));
					// float2 rnd2 = float2(rand(iuv.r*.001), rand(iuv.g*.005));
					float2 rnd = (rnd1 - rnd2) * grayColor;
					float2 fraction = 1 - rnd;
					//float4 color = tex2D( _OpaqueTextureCustomPass, screenUV * fraction);
					float4 color;
					if(_UseTempTex > 0.5)
					{
						color = tex2D( _TempOpaqueTexture, screenUV * fraction);
					}
					else
					{
						color = tex2D( _CameraOpaqueTexture, screenUV * fraction);
					}
					i.color = color;
					i.color.a = tex2D(_MainTex, i.uv).a;
					//i.color.rgb *= (1 - grayColor);

					//return i.color * tex2D(_MainTex, i.uv) * _ColorStrength;
					return i.color * _TintColor * _ColorStrength;
				}
				
				return _TintColor * tex2D(_MainTex, i.uv) * _ColorStrength;
			}
			ENDCG 
		}
	}	
}
}
