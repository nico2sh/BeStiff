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

texture ObjectsMap;
sampler ObjectsSampler : register(s2) = sampler_state
{
	Texture = <ObjectsMap>;
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture HeroMap;
sampler HeroSampler : register(s3) = sampler_state
{
	Texture = <HeroMap>;
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

// Hides noise indicators behind lit objects and behind the hero.
float4 AlphaNoisesPS(PixelInput input) : COLOR0
{
	float4 light = tex2D(AlphaMapSampler, input.TexCoord);
	float4 objects = tex2D(ObjectsSampler, input.TexCoord);
	float4 hero = tex2D(HeroSampler, input.TexCoord);
	float4 color = tex2D(IncomingSampler, input.TexCoord);
	float brightness = max(light.r, max(light.g, light.b));
	float occlusion = saturate(objects.a * brightness * 1.25 + hero.a);
	// Premultiplied output (see AlphaMap.fx).
	return color * (1.0 - occlusion);
}

technique AlphaNoisesShader
{
	pass Pass0
	{
		PixelShader = compile PS_SHADERMODEL AlphaNoisesPS();
	}
}
