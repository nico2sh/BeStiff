// Shared declarations for the Be Stiff post-processing effects.
// These pixel shaders are applied on top of SpriteBatch, so the vertex
// stage comes from MonoGame's built-in sprite effect.

#if OPENGL
	#define PS_SHADERMODEL ps_3_0
#else
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// Texture bound by SpriteBatch.Draw (register 0).
sampler IncomingSampler : register(s0);

struct PixelInput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TexCoord : TEXCOORD0;
};
