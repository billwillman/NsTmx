Shader "TiledMap_ETC1"
{
	Properties
	{
		_MainTex ("Tex", 2D) = "white" {}
		_AlphaTex ("Alpha Tex", 2D) = "black" {}
		_Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}

		Pass
		{
			Cull Off
			Lighting Off
			ZWrite Off
			Offset -1, -1
			Fog { Mode Off }
			blend SrcAlpha OneMinusSrcAlpha 

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			float4 _MainTex_ST;
			float4 _Color;

			struct appdata_t
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float4 color:COLOR;
			};

			struct v2f
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float4 color:COLOR;
			};

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				o.color = v.color * _Color;
				return o;
			}

			half4 frag (v2f IN) : COLOR
			{
				half4 col = tex2D(_MainTex, IN.texcoord);
				half4 alpha = tex2D(_AlphaTex, IN.texcoord);
				col.a = alpha.r;
				col *= IN.color;
				return col;
			}
			ENDCG

		}
	}
}
