//-----------------------------------------------------------------------------
//Include all common items
#include "../_mainInclude.hlsl"

//-----------------------------------------------------------------------------
//PixelShader implementation
float4 main(PSInputStandard input) : SV_Target
{
	float4 result = ObjectColor;
	result.a = Opacity;
	
	// Calculate fade effect (if configured)
	float factor = 1.0 - FadeIntensity * ((float)(abs((Time % 1000) - 500)) / 750.0);
	result.rgb = result.rgb * factor;

	return result;
}