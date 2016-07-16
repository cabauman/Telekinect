Shader "Cook-Torrence_Bumped" 
{
	Properties 
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_BumpMap ("Normalmap", 2D) = "bump" {}
		_Roughness ("Surface Roughness", Range(0.001,1)) = 0.2
		_RefractionIndex ("Surface Refraction", Range(0,1)) = 0.2
		_SpecularAmount ("Specular contribution", Range (0,1)) = 0.5
	}
	
	SubShader 
	{
		Tags 
		{ 
			"RenderType"="Opaque" 
		}
		LOD 200

		CGPROGRAM
			#pragma surface surf CookTorrenceApprox
			#pragma debug
			#pragma target 3.0

			fixed4 _Color;
			sampler2D _MainTex;
			sampler2D _BumpMap;
			float  _Roughness;
			float  _RefractionIndex;
			float  _SpecularAmount;
			
			//defined in Lighting.cginc
			//fixed4 _LightColor0;
			//fixed4 _SpecColor;
			
			//An approximation of the Cook-Torrence lighting model.  Approximations by Schlick.
			inline fixed4 LightingCookTorrenceApprox (SurfaceOutput s, fixed3 lightDir, fixed3 viewDir, fixed atten)
			{
				s.Normal = normalize (s.Normal);
				lightDir = normalize (lightDir);
				viewDir = normalize (viewDir);
				float3 H = normalize (lightDir + viewDir);			//Blinn's highlight vector
				
				//float diff = max (0.0, dot (s.Normal, lightDir));		//Standard diffuse term (from Phong)
				
				float NdotH = max (0.0001, dot (s.Normal, H));
				float NdotV = max (0.0001, dot (s.Normal, viewDir));
				float NdotL = max (0.0001, dot (s.Normal, lightDir));
				float VdotH = max (0.0001, dot( viewDir, H));			//Avoid divide by zero
				
				//float NdotH = dot (s.Normal, H);
				//float NdotV = dot (s.Normal, viewDir);
				//float NdotL = dot (s.Normal, lightDir);
				//float VdotH = dot(viewDir, H);
				
				//Geometric attenuation factor (occlusion of light vector due to glancing angles off of rough microfaceted surfaces)
				//From Cook-Torrence (who seemed to have got it from Blinn, who got it from Torrence - small world)
				float G = min(1.0, min((2 * NdotH * NdotV) / VdotH, (2 * NdotH * NdotL) / VdotH)); 
				
				//Approximated Beckmann distribution Function (from Schlick)
				//float x = NdotH + _Roughness - 1;
				//float xsq = x * x;
				//float DlowerTerm = _Roughness * xsq - xsq + _Roughness * _Roughness;
				//float D = ( _Roughness * _Roughness * _Roughness * x ) / (NdotH * DlowerTerm * DlowerTerm);
				
				//Beckmann Distrubution Function (from cook-Torrence)
				float exponent = exp(-(pow(tan(acos(NdotH)),2.0) / (_Roughness * _Roughness)));
				float DlowerTerm = (_Roughness * _Roughness) * pow(NdotH, 4.0);
				float D = (exponent / DlowerTerm);

				//Approximated Fresnel term (from Schlick)
				float F = _RefractionIndex + (1 - _RefractionIndex) * pow((1 - VdotH), 5.0);
				
				//Denominator for BRDF
				//float denom = 3.141593 * NdotL * NdotV;							//the Cook-Torrence version of the denominator - causes infinite falloff (hard line at 90 degrees)
				float spec = ((F * D * G) / NdotV) * _SpecularAmount;	//specular contribution (from Blinn (the NdotV term) and Cook-Torrence)
				float diff = NdotL * (1 - _SpecularAmount);				//diffuse contribution
				
				fixed4 c;
				c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * spec) * (atten * 2);
				c.a = s.Alpha + _LightColor0.a * _SpecColor.a * spec * atten;
				return c;
			}	

			struct Input 
			{
				float2 uv_MainTex;
				float2 uv_BumpMap;
			};

			void surf (Input IN, inout SurfaceOutput o) 
			{
			
				fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
				o.Albedo = tex.rgb * _Color.rgb;
				o.Alpha = tex.a * _Color.a;
				o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
			}
		ENDCG
	}

	Fallback "Bumped Specular"
}