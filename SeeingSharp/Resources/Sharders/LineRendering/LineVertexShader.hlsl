//-----------------------------------------------------------------------------
//Include all common items
#include "../_mainInclude.hlsl"

//-----------------------------------------------------------------------------
//VertexShader implementation
PSInputLine main(VSInputLine input) 
{
    PSInputLine output = (PSInputLine)0;
    
    //Calculate standard values
    output.pos = mul(float4(input.pos.xyz, 1.0), WorldViewProj);

    return output;
}
