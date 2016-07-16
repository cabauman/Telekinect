Shader "Dissolve/Diffuse" 
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
		Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
		LOD 200
		
		Cull Off

		CGPROGRAM
			//Need the alphatest here instead of as a pass or subshader tag - this is undocumented, if the alphatest is not here, then lighting for the fragment shader will not work!
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
}
