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

// Darkens and fades the hero where objects are drawn over him.
float4 HeroDrawPS(PixelInput input) : COLOR0
{
	float4 objects = tex2D(AlphaMapSampler, input.TexCoord);
	float4 color = tex2D(IncomingSampler, input.TexCoord);
	// Premultiplied output: darken and fade together.
	float fade = 1.0 - 0.5 * objects.a;
	float4 result;
	result.rgb = color.rgb * (1.0 - objects.a) * fade;
	result.a = color.a * fade;
	return result;
}

technique BlackShadowMapShader
{
	pass Pass0
	{
		PixelShader = compile PS_SHADERMODEL HeroDrawPS();
	}
}
