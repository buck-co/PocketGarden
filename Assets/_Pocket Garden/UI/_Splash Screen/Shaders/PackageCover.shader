
Shader "PocketGarden/PackageCover"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1) 
		_MainTex("Cover Texture", 2D) = "white" {}
		_Shine("Shine Texture", 2D) = "white" {}
		_Mask("Mask Shine Texture", 2D) = "white" {}
		_PlasticReflection("Plastic Reflection Texture", 2D) = "white" {}
		_HolographicTexture("Holographic Texture", 2D) = "white" {}
		_PlasticMask("Plastic Mask Texture", 2D) = "white" {}
        _Tilt ("Tilt Number", Float) = 0.0
        _Turn ("Turn Number", Float) = 0.0
        _AmountDistorted ("Distortion Number", Float) = 0.0
        _CutOff("Alpha Cutoff", Range(0, 1)) = 0.5 
	}
	SubShader{
		Tags{"Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout"}

		Pass{
			Tags{"LightMode" = "ForwardBase"}

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag	

			#include "Lighting.cginc"

			fixed4 _Color;
			sampler2D _MainTex;
			sampler2D _Shine;
			sampler2D _Mask;
			sampler2D _PlasticReflection;
			sampler2D _PlasticMask;
			sampler2D _HolographicTexture;
			float4 _MainTex_ST;
			float _Tilt;
			float _Turn;
			float _AmountDistorted;
            fixed _CutOff;

			struct a2v {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				float3 worldNormal : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
				float2 uv : TEXCOORD2;
			};

			v2f vert(a2v v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target{
				fixed3 worldNormal = normalize(i.worldNormal);
				fixed3 worldLightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));

                fixed4 texColor = tex2D(_MainTex,i.uv);
                fixed4 shineColor = tex2D(_Shine,i.uv);
                fixed4 maskShine = tex2D(_Mask,i.uv+float2(_Turn,0));
                fixed4 plastic = tex2D(_PlasticReflection,i.uv);
                fixed4 holographic = tex2D(_HolographicTexture,i.uv+float2(_Turn,0));
				// fixed4 holographicPattern = lerp(holographic)
                fixed4 plasticMask = tex2D(_PlasticMask,i.uv+float2(0,_Tilt*0.5));
				fixed4 plasticEffect = lerp(float4(0,0,0,0), plastic*holographic*0.35, plasticMask);
                fixed4 shineEffect = lerp(texColor, shineColor, maskShine);

				clip(texColor.a - _CutOff);
				return fixed4(plasticEffect+shineEffect);
			}

			ENDCG
		}
	}
	FallBack "Transparent/CutOut/VertexLit"
}