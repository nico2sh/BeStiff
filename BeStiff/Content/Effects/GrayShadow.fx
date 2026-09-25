#include "Common.fxh"

texture AlphaMap;
sampler AlphaMapSampler : register(s1) = sampler_state
{
	Texture = <AlphaMap>;
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

// Brightness left where the light map is black (set from code: MonoGame
// drops HLSL default values).
float DarkLevel;

// Desaturates and dims the sprite where the light map is dark.
float4 GrayShadowPS(PixelInput input) : COLOR0
{
	float4 light = tex2D(AlphaMapSampler, input.TexCoord);
	float4 color = tex2D(IncomingSampler, input.TexCoord);
	float brightness = max(light.r, max(light.g, light.b));
	float gray = dot(color.rgb, float3(0.3, 0.59, 0.11));
	float4 result;
	// Premultiplied colour: scaling rgb alone darkens without changing coverage.
	result.rgb = lerp(gray.xxx, color.rgb, brightness) * lerp(DarkLevel, 1.0, brightness);
	result.a = color.a;
	return result;
}

technique GrayShadowMapShader
{
	pass Pass0
	{
		PixelShader = compile PS_SHADERMODEL GrayShadowPS();
	}
}
