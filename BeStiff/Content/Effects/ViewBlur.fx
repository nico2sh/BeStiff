#include "Common.fxh"

// Unshadowed shape of the hero's light: sharp where bright, blurred where dark.
texture ViewMask;
sampler ViewMaskSampler : register(s1) = sampler_state
{
	Texture = <ViewMask>;
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

// Blur radius in texture coordinates (pixels / screen size). Set from code:
// MonoGame drops HLSL default values.
float2 BlurRadius;

// Mask brightness from which the view is fully sharp.
float SharpLevel;

static const float2 Taps[12] =
{
	float2(-0.326, -0.406), float2(-0.840, -0.074), float2(-0.696, 0.457),
	float2(-0.203, 0.621), float2(0.962, -0.195), float2(0.473, -0.480),
	float2(0.519, 0.767), float2(0.185, -0.893), float2(0.507, 0.064),
	float2(0.896, 0.412), float2(-0.322, -0.933), float2(-0.792, -0.598)
};

float4 ViewBlurPS(PixelInput input) : COLOR0
{
	float4 sharp = tex2D(IncomingSampler, input.TexCoord);
	float4 mask = tex2D(ViewMaskSampler, input.TexCoord);
	float view = smoothstep(0.0, SharpLevel, max(mask.r, max(mask.g, mask.b)));
	float4 blurred = sharp;
	for (int i = 0; i < 12; i++)
	{
		blurred += tex2D(IncomingSampler, input.TexCoord + Taps[i] * BlurRadius);
	}
	blurred /= 13.0;
	return lerp(blurred, sharp, view);
}

technique ViewBlurShader
{
	pass Pass0
	{
		PixelShader = compile PS_SHADERMODEL ViewBlurPS();
	}
}
