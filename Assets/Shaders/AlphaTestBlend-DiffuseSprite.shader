Shader "Transparent/Cutout/DiffuseSpriteBlend" 
{
	Properties 
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_Cutoff ("Alpha cutoff", Range(0.001,0.99)) = 0.5
	}

	SubShader 
	{
		Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
		LOD 200
			
		//Lighting Off
		Cull Off
		Alphatest Greater [_Cutoff]
		Blend SrcAlpha OneMinusSrcAlpha
			
		Pass
		{
			
		CGPROGRAM
			//#pragma surface surf Lambert alphatest:_Cutoff
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			fixed4 _Color;

			struct v2f 
			{ 
				float4 pos : SV_POSITION;
				float2  uv : TEXCOORD1;
			};
			
			uniform float4 _MainTex_ST;

			v2f vert( appdata_base v )
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				half4 c = tex2D (_MainTex, i.uv);
				return c * _Color;
			}

		ENDCG
		}
	}

	Fallback "Transparent/Cutout/VertexLit"
}
