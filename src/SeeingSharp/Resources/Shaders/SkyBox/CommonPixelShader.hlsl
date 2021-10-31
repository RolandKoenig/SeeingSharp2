//-----------------------------------------------------------------------------
//Include all common items
#include "../_mainInclude.hlsl"

//-----------------------------------------------------------------------------
//PixelShader implementation
float4 main(PSInputStandard input) : SV_Target
{
	float4 result = ObjectCubeTexture.Sample(ObjectTextureSampler, input.pos3D);
	result.a = 1.0;
	return result;
}