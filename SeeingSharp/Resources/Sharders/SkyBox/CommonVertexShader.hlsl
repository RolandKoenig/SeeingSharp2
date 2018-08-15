//-----------------------------------------------------------------------------
//Include all common items
#include "../_mainInclude.hlsl"

//-----------------------------------------------------------------------------
//VertexShader implementation
PSInputStandard main(VSInputStandard input) 
{
    PSInputStandard output = (PSInputStandard)0;

    // Calculate only position (we need nothing more for skyboxes)
	float4x4 localWorldViewProj = ViewProj;
    output.pos = mul(float4(input.pos.xyz + CameraPosition, 1.0), localWorldViewProj).xyww;
	output.pos3D = input.pos.xyz;

    return output;
}
