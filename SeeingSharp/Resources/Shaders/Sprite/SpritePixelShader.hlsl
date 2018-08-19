//-----------------------------------------------------------------------------
//Include all common items
#include "../_mainInclude.hlsl"

//-----------------------------------------------------------------------------
//PixelShader implementation
float4 main(PSInputStandard input ) : SV_Target
{
	float4 outputColor = ObjectTexture.Sample(ObjectTextureSampler, input.tex.xy);

    // Apply Accentuation effect
    outputColor = ApplyAccentuation(outputColor);

	// Apply per-object Opacity value
	outputColor.a = outputColor.a * Opacity;

    return outputColor;
}
