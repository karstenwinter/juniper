Shader "Solid Diffuse with Shadow" {

	Properties {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
		_Cutoff ("Alpha Cutoff", Range (0,1)) = 0.5
	}
	

	SubShader {
		Tags {
				"Queue"="Geometry" 
				"IgnoreProjector"="True" 
				"RenderType"="TransparentCutOut"
				"PreviewType"="Plane"
				"CanUseSpriteAtlas"="True"
		}
		Cull Off
		LOD 300

		CGPROGRAM
			#pragma version 2.0
			#pragma surface surf Lambert addshadow alphatest:_Cutoff
			
			CBUFFER_START(UnityPerDrawSprite)
			    fixed4 _RendererColor;
			    fixed2 _Flip;
			    float _EnableExternalAlpha;
			CBUFFER_END

			struct appdata_t
			{
			    float4 vertex   : POSITION;
			    float4 color    : COLOR;
			    float2 texcoord : TEXCOORD0;
			    UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
			    float4 vertex   : SV_POSITION;
			    fixed4 color    : COLOR;
			    float2 texcoord : TEXCOORD0;
			    UNITY_VERTEX_OUTPUT_STEREO
			};

			inline float4 UnityFlipSprite(in float3 pos, in fixed2 flip)
			{
			    return float4(pos.xy * flip, pos.z, 1.0);
			}

			v2f SpriteVert(appdata_t IN)
			{
			    v2f OUT;

			    UNITY_SETUP_INSTANCE_ID (IN);
			    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

			    OUT.vertex = UnityFlipSprite(IN.vertex, _Flip);
			    OUT.vertex = UnityObjectToClipPos(OUT.vertex);
			    OUT.texcoord = IN.texcoord;
			    OUT.color = IN.color * _RendererColor;

			    #ifdef PIXELSNAP_ON
			    OUT.vertex = UnityPixelSnap (OUT.vertex);
			    #endif

			    return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			fixed4 _Color;
			
			struct Input {
				float2 uv_MainTex;
				float2 uv_BumpMap;
			};
			void surf (Input IN, inout SurfaceOutput o) {
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex)
				 			 * _RendererColor * _Color;
				o.Albedo = c.rgb;
				o.Alpha = c.a;
			}

			fixed4 SampleSpriteTexture (float2 uv)
			{
			    fixed4 color = tex2D (_MainTex, uv);

			#if ETC1_EXTERNAL_ALPHA
			    fixed4 alpha = tex2D (_AlphaTex, uv);
			    color.a = lerp (color.a, alpha.r, _EnableExternalAlpha);
			#endif

			    return color;
			}

			fixed4 SpriteFrag(v2f IN) : SV_Target
			{
			    fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
			    c.rgb *= c.a;
			    return c;
			}
		ENDCG
	}

	Fallback "Transparent/Cutout/Diffuse"
}