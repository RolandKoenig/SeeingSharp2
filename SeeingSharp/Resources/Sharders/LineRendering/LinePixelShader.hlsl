//-----------------------------------------------------------------------------
//Include all common items
#include "../_mainInclude.hlsl"

//-----------------------------------------------------------------------------
//PixelShader implementation for line rendering
float4 main(PSInputLine input) : SV_Target
{
    return DiffuseColor;
}