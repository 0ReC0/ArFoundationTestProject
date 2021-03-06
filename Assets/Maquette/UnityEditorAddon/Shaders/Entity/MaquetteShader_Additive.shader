Shader "Maquette/DefaultEntity/Additive" 
{
	Properties
	{
		_Color("Main Color", Color) = (1,1,1,1)
		_Brightness("Brightness", Range(0.0, 10.0)) = 1.0
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
	}
	SubShader
	{
		Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}

		ZWrite Off
		Blend SrcAlpha One

		Pass 
		{
			Name "DEFAULT"

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			// Enable gpu instancing variants.
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			fixed4 _MainTex_ST;
			fixed _Brightness;

			UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
			UNITY_INSTANCING_BUFFER_END(Props)

			struct appdata
			{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;

				// Need this for basic functionality.
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;

				// Need this for basic functionality.
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			v2f vert(appdata v)
			{
				v2f o;

				// Need this for basic functionality.
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

				return o;
			}		

			min16float4 frag(v2f i) : SV_Target 
			{
				// Need this for basic functionality.
				UNITY_SETUP_INSTANCE_ID(i);

				min16float4 col = tex2D(_MainTex, i.uv);
				col *= (min16float4)UNITY_ACCESS_INSTANCED_PROP(Props, _Color) * _Brightness;

				return col;
			}
			ENDCG
		}
	}
}
