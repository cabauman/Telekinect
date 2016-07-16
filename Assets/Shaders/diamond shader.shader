Shader "FX/Diamond"
{
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_ReflectTex ("Reflection Texture", Cube) = "dummy.jpg" {
			TexGen CubeReflect
		}
		_RefractTex ("Refraction Texture", Cube) = "dummy.jpg" {
			TexGen CubeReflect
		}
		_AlphaTex ("Alpha texture (GREYSCALE)", 2D) = "white" {}
		_Cutoff ("Cutoff", Range (0.0, 1.0)) = 0
	}	
	SubShader 
	{
		Tags 
		{
			"Queue" = "Transparent"
		}
		
		// First pass - here we render the backfaces of the diamonds. Since those diamonds are more-or-less
		// convex objects, this is effectively rendering the inside of them
		Pass 
		{
			Color (0,0,0,0)
			Offset  -1, -1
			Cull Front
			AlphaTest Greater [_Cutoff]
			ZWrite Off
			SetTexture [_RefractTex] 
			{
				constantColor [_Color]
				combine texture * constant , primary
			}
			SetTexture [_ReflectTex] 
			{
				combine previous , previous +- texture
			}
			SetTexture[_AlphaTex]
			{
				constantColor(0, 0, 0, 0)
				combine previous + constant, texture
			}
		}

		// Second pass - here we render the front faces of the diamonds.
		Pass 
		{
			Fog { Color (0,0,0,0)}
			ZWrite on
			Blend One One
			AlphaTest Greater [_Cutoff]
			
			SetTexture [_RefractTex] 
			{
				constantColor [_Color]
				//combine texture * constant
			}
			SetTexture [_ReflectTex]
			{
				combine texture + previous, previous +- texture
			}
			SetTexture [_AlphaTex]
			{
				constantColor(0,0,0,0)
				combine previous + constant, texture
			}
		}
	}

	// Older cards. Here we remove the bright specular highlight
	SubShader {
		// First pass - here we render the backfaces of the diamonds. Since those diamonds are more-or-less
		// convex objects, this is effectively rendering the inside of them
		Pass {
			Color (0,0,0,0)
			Cull Front
			SetTexture [_RefractTex] {
				constantColor [_Color]
				combine texture * constant, primary
			}
		}

		// Second pass - here we render the front faces of the diamonds.
		Pass {
			Fog { Color (0,0,0,0)}
			ZWrite on
			Blend One One
			SetTexture [_RefractTex] {
				constantColor [_Color]
				combine texture * constant
			}
		}
	}

	// Ancient cards without cubemapping support
	// We could use a 2D refletction texture, but the chances of getting one of these cards are slim, so we won't bother.
	SubShader {
		Pass {
			Color [_Color]
		}
	}
}