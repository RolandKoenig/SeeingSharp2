//-----------------------------------------------------------------------------
//Include all common items
#include "../_mainInclude.hlsl"

//-----------------------------------------------------------------------------
//VertexShader implementation
PSInputLine main(VSInputLine input) 
{
    PSInputLine output = (PSInputLine)0;

	float4x4 localWorldViewProj = mul(World, ViewProj);
    output.pos = mul(float4(input.pos.xyz, 1.0), localWorldViewProj);

    return output;
}
