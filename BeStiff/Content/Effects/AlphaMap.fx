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

// Scales the sprite's alpha by the brightest channel of the light map.
float4 AlphaMapPS(PixelInput input) : COLOR0
{
	float4 light = tex2D(AlphaMapSampler, input.TexCoord);
	float4 color = tex2D(IncomingSampler, input.TexCoord);
	float brightness = max(light.r, max(light.g, light.b));
	// Premultiplied output: SpriteBatch adds RGB regardless of alpha, so the
	// mask must scale both or hidden objects show up as additive ghosts.
	float mask = saturate(brightness * 1.25);
	return color * mask;
}

technique AlphaMapShader
{
	pass Pass0
	{
		PixelShader = compile PS_SHADERMODEL AlphaMapPS();
	}
}
