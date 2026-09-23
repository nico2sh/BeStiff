#include "Common.fxh"

float4 ShadowColor;

// Screen tint driven by the hero's state (ShadowColor: black = none, red when
// hurt, blue in bullet time). The incoming texture is the Krypton light map;
// the tint is strongest where the hero cannot see (dark or occluded) and
// fades out towards the lit area. It never hides the background: visibility
// of objects is handled by the object mask, dimming by the light falloff.
float4 AlphaShadowPS(PixelInput input) : COLOR0
{
	float4 light = tex2D(IncomingSampler, input.TexCoord);
	float brightness = max(light.r, max(light.g, light.b));
	float strength = max(ShadowColor.r, max(ShadowColor.g, ShadowColor.b));
	float tint = saturate(1.0 - brightness) * strength * 0.5;
	// Premultiplied output for SpriteBatch.
	return float4(ShadowColor.rgb * tint, tint);
}

technique AlphaShadowMapShader
{
	pass Pass0
	{
		PixelShader = compile PS_SHADERMODEL AlphaShadowPS();
	}
}
