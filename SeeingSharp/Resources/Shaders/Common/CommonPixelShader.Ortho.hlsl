//-----------------------------------------------------------------------------
//Include all common items
#include "../_mainInclude.hlsl"

//-----------------------------------------------------------------------------
//PixelShader implementation
PSOutput main(PSInputStandard input)
{
	// Query texture color first
	float4 textureColor = {0.0, 0.0, 0.0, 0.0};
	if(Texture0Factor + input.texFact > 0.0)
	{
		textureColor = ObjectTexture.Sample(ObjectTextureSampler, input.tex.xy);
	}
	textureColor.a = clamp(textureColor.a * Texture0Factor + AddToAlpha, 0.0, 1.0);

	// Calculate the pixel color based on vertex colors and border logic
	float4 pixelColor = ApplyColorBorders(input.col, 5.0, input.tex);

	// Calculate final pixel color using texture color
	pixelColor = textureColor * textureColor.a + pixelColor * (1 - textureColor.a);

	// Clip current pixel based on alpha value
	clip(textureColor.a - ClipFactor - input.texFact);

	// Apply Accentuation effect on pixel color
	pixelColor = ApplyAccentuation(pixelColor);

	// Calculate simple lightning effect
	float3 cameraNormal = { View._13, View._23, View._33 };
	float diffuseLightingFactor = dot(-cameraNormal, input.normal);
	diffuseLightingFactor = saturate(diffuseLightingFactor);
	diffuseLightingFactor *= LightPower;
	diffuseLightingFactor = 1.0 - (1.0 - diffuseLightingFactor) / StrongLightFactor;

	// Calculate final output color
	pixelColor = pixelColor * clamp(diffuseLightingFactor + Ambient, 0.0, 1.0);
	pixelColor.a = Opacity;

	// Write and return output structure
	PSOutput output = (PSOutput)0;
	output.color = pixelColor;
	output.normalDepth.xyz = input.normal.xyz;
	output.normalDepth.a = 0.0;

	return output;
}