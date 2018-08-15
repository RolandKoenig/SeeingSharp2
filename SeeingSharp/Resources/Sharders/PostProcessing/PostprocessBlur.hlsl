//-----------------------------------------------------------------------------
//Include all common items
#include "_structures.hlsl"

Texture2D     SpriteTexture        : register(t0);
SamplerState  SpriteTextureSampler : register(s0);

// Buffer with per object data
cbuffer cbPerObject : register (b2)
{
	float		BlurIntensity;
	float		BlurOpacity;
	float2      Dummy2;
}

//-----------------------------------------------------------------------------
//PixelShader implementation
float4 main(PSInputPostprocess input) : SV_Target
{
	// Get original texture color
	float4 textureColor = SpriteTexture.Sample(SpriteTextureSampler, input.tex);

	// Calculate blured color from texture
	float4 outputColor = 0;
	outputColor += SpriteTexture.Sample(SpriteTextureSampler, float2(clamp(input.tex.x + 0.004 * BlurOpacity, 0, 1), clamp(input.tex.y - 0.004 * BlurOpacity, 0, 1))) * 0.1;
	outputColor += SpriteTexture.Sample(SpriteTextureSampler, float2(clamp(input.tex.x + 0.003 * BlurOpacity, 0, 1), clamp(input.tex.y - 0.003 * BlurOpacity, 0, 1))) * 0.2;
	outputColor += SpriteTexture.Sample(SpriteTextureSampler, float2(clamp(input.tex.x + 0.002 * BlurOpacity, 0, 1), clamp(input.tex.y - 0.002 * BlurOpacity, 0, 1))) * 0.4;
	outputColor += SpriteTexture.Sample(SpriteTextureSampler, float2(clamp(input.tex.x - 0.003 * BlurOpacity, 0, 1), clamp(input.tex.y + 0.003 * BlurOpacity, 0, 1))) * 0.2;
	outputColor += SpriteTexture.Sample(SpriteTextureSampler, float2(clamp(input.tex.x - 0.004 * BlurOpacity, 0, 1), clamp(input.tex.y + 0.004 * BlurOpacity, 0, 1))) * 0.1;

	// Merge colors based on given blue intensity
	outputColor = outputColor * BlurIntensity + (textureColor * (1 - BlurIntensity));
	outputColor.a = min(outputColor.a, BlurOpacity);

	// Return found color
	return outputColor;
}