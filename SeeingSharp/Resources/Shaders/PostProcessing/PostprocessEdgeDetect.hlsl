//-----------------------------------------------------------------------------
//Include all common items
#include "_structures.hlsl"

Texture2D     SpriteTexture        : register(t0);
SamplerState  SpriteTextureSampler : register(s0);

// Helper function
float getGray(float4 c)
{
	return c.a;
}

// Buffer with per object data
cbuffer cbPerObject : register (b2)
{
	float		Opacity;
	float2      ScreenPixelSize;
	float       Thickness;
	float       Threshold;
	float3      BorderColor;
	float       OriginalColorAlpha;
	float3		Dummy;
}

//-----------------------------------------------------------------------------
//PixelShader implementation
float4 main(PSInputPostprocess input) : SV_Target
{
	// Edge detection using kernel filter method
	//  see http://digitalerr0r.wordpress.com/2009/03/22/xna-shader-programming-tutorial-7-toon-shading/
	//  Other tutorials at http://digitalerr0r.wordpress.com/tutorials/
	//  More info about kernel filter method: http://en.wikipedia.org/wiki/Kernel_(image_processing)

	// Get normal output color from source texture / configured opacity
	float4 outputColor = SpriteTexture.Sample(SpriteTextureSampler, input.tex);
	outputColor.a = min(Opacity, outputColor.a);
	clip(outputColor.a - 0.01);

	// Remember original color
	float4 originalColor = outputColor;
		originalColor.a = OriginalColorAlpha;

	// Set complete color to 1 (ensure one big single-colored block)
	outputColor.rgb = 1;

	// Perofrm edge detection using kernel filter method
	float2 ox = float2(Thickness / ScreenPixelSize.x, 0.0);
	float2 oy = float2(0.0, Thickness / ScreenPixelSize.y);
	float2 uv = input.tex.xy;
	float2 PP = uv - oy;
	float4 CC = SpriteTexture.Sample(SpriteTextureSampler, PP - ox); float g00 = getGray(CC);
	CC = SpriteTexture.Sample(SpriteTextureSampler, PP);    float g01 = getGray(CC);
	CC = SpriteTexture.Sample(SpriteTextureSampler, PP + ox); float g02 = getGray(CC);
	PP = uv;
	CC = SpriteTexture.Sample(SpriteTextureSampler, PP - ox); float g10 = getGray(CC);
	CC = SpriteTexture.Sample(SpriteTextureSampler, PP);    float g11 = getGray(CC);
	CC = SpriteTexture.Sample(SpriteTextureSampler, PP + ox); float g12 = getGray(CC);
	PP = uv + oy;
	CC = SpriteTexture.Sample(SpriteTextureSampler, PP - ox); float g20 = getGray(CC);
	CC = SpriteTexture.Sample(SpriteTextureSampler, PP);    float g21 = getGray(CC);
	CC = SpriteTexture.Sample(SpriteTextureSampler, PP + ox); float g22 = getGray(CC);
	float K00 = -1;
	float K01 = -2;
	float K02 = -1;
	float K10 = 0;
	float K11 = 0;
	float K12 = 0;
	float K20 = 1;
	float K21 = 2;
	float K22 = 1;
	float sx = 0;
	float sy = 0;
	sx += g00 * K00;
	sx += g01 * K01;
	sx += g02 * K02;
	sx += g10 * K10;
	sx += g11 * K11;
	sx += g12 * K12;
	sx += g20 * K20;
	sx += g21 * K21;
	sx += g22 * K22;
	sy += g00 * K00;
	sy += g01 * K10;
	sy += g02 * K20;
	sy += g10 * K01;
	sy += g11 * K11;
	sy += g12 * K21;
	sy += g20 * K02;
	sy += g21 * K12;
	sy += g22 * K22;
	float dist = sqrt(sx*sx + sy*sy);
	float result = 1;
	if (dist>Threshold)
	{
		result = 0;
	}
	else
	{
		return originalColor;
	}

	// Generate output (clip unseen pixels)
	outputColor.xyz = BorderColor.xyz;
	outputColor.a = 1.0 - result.x;
	clip(outputColor.a - 0.1);

	return outputColor;
}