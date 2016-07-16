Shader "GUI/3D Dissolving Text Shader" 
{
	Properties 
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_DissolveTex ("Dissolve (GREYSCALE)", 2D) = "white" {}
		_Cutoff ("Cutoff", Range (0.0, 1.0)) = 0
	}
	
	SubShader 
	{
		Tags { "Queue"="Overlay" "RenderType"="TransparentCutout" }
		LOD 200	
		
		//Lighting Off
		//Cull Off
		ZTest Always
		ZWrite Off 
		Alphatest Greater [_Cutoff]
		CGPROGRAM
			#pragma surface surf Lambert alphatest:_Cutoff

			sampler2D _MainTex;
			sampler2D _DissolveTex;
			fixed4 _Color;

			struct Input 
			{
				float2 uv_MainTex;
			};

			void surf (Input IN, inout SurfaceOutput o) 
			{
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				float4 a = tex2D(_DissolveTex, IN.uv_MainTex);
				o.Albedo = c.rgb;
				o.Alpha = DecodeFloatRGBA(a);
			}
		ENDCG
	}

	Fallback "VertexLit"

	// SubShader
	// {

		// Tags
		// { 
		// "Queue"="Transparent" 
		// "IgnoreProjector"="True" 
		// "RenderType"="Transparent" 
		// }
		
		// Cull Off
		// ZTest Always
		// ZWrite Off 
		// Fog { Mode Off }
		// Blend SrcAlpha OneMinusSrcAlpha

		// Pass 
		// {	
			// CGPROGRAM
			// #pragma vertex vert
			// #pragma fragment frag
			// #pragma fragmentoption ARB_precision_hint_fastest

			// #include "UnityCG.cginc"

			// struct appdata_t {
				// float4 vertex : POSITION;
				// float2 texcoord : TEXCOORD0;
			// };

			// struct v2f {
				// float4 vertex : POSITION;
				// float2 texcoord : TEXCOORD0;
			// };

			// sampler2D _MainTex;
			// uniform float4 _MainTex_ST;
			// uniform fixed4 _Color;
			
			// v2f vert (appdata_t v)
			// {
				// v2f o;
				// o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				// o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				// return o;
			// }

			// fixed4 frag (v2f i) : COLOR
			// {
				// fixed4 col = _Color;
				// col.a *= tex2D(_MainTex, i.texcoord).a;
				// return col;
			// }
			// ENDCG 
		// }
	// } 	

	// SubShader 
	// {
		// Tags 
		// { 
		// "Queue"="Transparent" 
		// "IgnoreProjector"="True" 
		// "RenderType"="Transparent" 
		// }
		// Cull Off
		// ZTest Always
		// ZWrite Off
		// Fog { Mode Off }
		// Blend SrcAlpha OneMinusSrcAlpha
		// Pass 
		// {
			// Color [_Color]
			// SetTexture [_MainTex] 
			// {
				// combine primary, texture * primary
			// }
		// }
	// }


}
