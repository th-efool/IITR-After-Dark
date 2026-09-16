Shader "Custom/X-Ray"
{
	Properties{
		_RimColor("Rim Color", Color) = (1,1,1,1)
		_RimPower("Rim Power", Range(0.5,8.0)) = 3.5
		_MainTex("Base (RGB) Gloss (A)", 2D) = "red" {}
	}
		SubShader{
			Tags { "Queue" = "Geometry" }

			// This pass is used for creating the hologram rim. It is setup in way that it paints the fragments only if this object is occluded.
			Pass {
				Tags { "Queue" = "Transparent" "RenderType" = "Transparent"}

				ZTest Greater // Do not hide this object behind others.
				ZWrite Off // Do not write to depth buffer because this is a semi-transparent pass
				Blend SrcAlpha OneMinusSrcAlpha // Blend to create a transparency effect.

				CGPROGRAM
		// Using vertex and fragment shaders
		#pragma vertex vert            
		#pragma fragment frag

		#include "UnityCG.cginc"

		struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
		};

		struct v2f
		{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 worldVertex : TEXCOORD1;
				float3 viewDir : TEXCOORD2;
				float3 worldNormal : NORMAL;
		};

		sampler2D _MainTex;

		float4 _MainTex_ST;

		float4 _RimColor;
		float _RimPower;


		v2f vert(appdata v)
		{
			v2f o;

			o.vertex = UnityObjectToClipPos(v.vertex);

			o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			o.worldVertex = mul(unity_ObjectToWorld, v.vertex);
			o.worldNormal = UnityObjectToWorldNormal(v.normal);
			o.viewDir = normalize(UnityWorldSpaceViewDir(o.worldVertex.xyz));

			//// float4 viewPos = UnityObjectToClipPos(pos);
			return o;
		}

		half4 frag(v2f i) : SV_Target
		{
			// Calculate the color of this fragment based on the normal of the fragment and view direction.
			half rim = 1.0 - saturate(dot(normalize(i.viewDir), i.worldNormal));
			fixed4 rimColor = _RimColor * pow(rim, _RimPower) * 10;
			half a = pow(rim, _RimPower);

			rimColor.a = a;

			return rimColor;
		}
		ENDCG
	}

		// This pass is used to render the object if it is not occluded by anything. It is taken from the bottom of https://docs.unity3d.com/Manual/SL-VertexFragmentShaderExamples.html.
		Pass
		{
			Tags {"LightMode" = "ForwardBase"}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			// compile shader into multiple variants, with and without shadows
			// (we don't care about any lightmaps yet, so skip these variants)
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
			// shadow helper functions and macros
			#include "AutoLight.cginc"

			struct v2f
			{
				float2 uv : TEXCOORD0;
				SHADOW_COORDS(1) // put shadows data into TEXCOORD1
				fixed3 diff : COLOR0;
				fixed3 ambient : COLOR1;
				float4 pos : SV_POSITION;
			};
			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				half3 worldNormal = UnityObjectToWorldNormal(v.normal);
				half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
				o.diff = nl * _LightColor0.rgb;
				o.ambient = ShadeSH9(half4(worldNormal,1));
				// compute shadows data
				TRANSFER_SHADOW(o)
				return o;
			}

			sampler2D _MainTex;

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
			// compute shadow attenuation (1.0 = fully lit, 0.0 = fully shadowed)
			fixed shadow = SHADOW_ATTENUATION(i);
			// darken light's illumination with shadow, keep ambient intact
			fixed3 lighting = i.diff * shadow + i.ambient;
			col.rgb *= lighting;
			return col;
		}
		ENDCG
	}

			// shadow casting support
			UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
	}
		FallBack "Diffuse"
}
