//-----------------------------------------------------------------------------
//Include all common items
#include "../_mainInclude.hlsl"

//-----------------------------------------------------------------------------
//PixelShader implementation
PSOutput main(PSInputStandard input)
{
    // Calculate final pixel color using texture color
	float4 pixelColor = input.col;

    // Apply Accentuation effect on pixel color
    pixelColor = ApplyAccentuation(pixelColor);

    // Calculate simple lightning effect
    float diffuseLightingFactor = DotProduct(CameraPosition, input.pos3D, input.normal);
    diffuseLightingFactor = saturate(diffuseLightingFactor);
    diffuseLightingFactor *= LightPower;
    diffuseLightingFactor = 1.0 - (1.0 - diffuseLightingFactor) / StrongLightFactor;

    // Calculate final output color
    pixelColor= pixelColor * clamp(diffuseLightingFactor + Ambient, 0.0, 1.0);
	pixelColor.a = Opacity;

	PSOutput output = (PSOutput)0;
	output.color = pixelColor;
	output.normalDepth.xyz = input.normal.xyz;
	output.normalDepth.a = 0.0;
	return output;
}