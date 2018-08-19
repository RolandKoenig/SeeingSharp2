// This file contains all default functions used across all
// object rendered with this graphics engine

// A function that generate a color gradient by a given input color and texture coordinate
float4 ApplyColorGradient(float4 inputColor, float2 texCoord)
{
	// Map texture coordinate to range 0..1
	texCoord.x = clamp(texCoord.x, 0.0, 1.0);
	texCoord.y = clamp(texCoord.y, 0.0, 1.0);

	// Calculate gradient factors
    float texCoordFactor = texCoord.x * 0.1f + texCoord.y;
    float changeFactor = (abs((texCoordFactor % 2) - 1) / 15) * GradientFactor;
	float4 outputColor;
    outputColor.x = inputColor.x + changeFactor;
    outputColor.y = inputColor.y + changeFactor;
    outputColor.z = inputColor.z + changeFactor;
	outputColor.w = inputColor.w;
    
	return outputColor;
}

// Changes the given input color so that a border will be generated based on texture coordinates
float4 ApplyColorBorders(float4 inputColor, float distanceToCamera, float2 texCoord)
{
	float inputAlpha = inputColor.a;

	// Calculate local factors depending on camera distance
	float distanceFactor = (distanceToCamera / 5);
	float locBorderPart = max(BorderPart, BorderPart * distanceFactor);
	float locBorderMultiplyer = min(BorderMultiplyer, 50 / distanceToCamera);

	// Map texture coordinate to range 0..1
	texCoord.x = texCoord.x % 1;
	texCoord.y = texCoord.y % 1;

	// Find out border area
	float blubx1 = texCoord.x;
	float blubx2 = 1 - texCoord.x;
	float bluby1 = texCoord.y;
	float bluby2 = 1 - texCoord.y;
	float blub = min(min(blubx1, blubx2), min(bluby1, bluby2));
	
	// Calculate the factor, used for generating the border
	float lineFactor = max(0.0f, locBorderPart - blub) * locBorderMultiplyer;

	// Calculate result color
	float4 output = inputColor - (inputColor * lineFactor * BorderFactor);
	output.a = inputAlpha;

	// Return result color
	return output;
}

// Applies the accentuation effect
float4 ApplyAccentuation(float4 input)
{
	float inputAlpha = input.a;

	float locAccentuationFactor = (1 - clamp(ObjectAccentuationFactor, 0, 1)) * Accentuation;

	float4 inputGrayScaled = dot(input, float4(0.3, 0.59, 0.11, 0.0));
	float grayScaleLightFactor = (1 - inputGrayScaled.r) / 1.3;
	inputGrayScaled = inputGrayScaled + grayScaleLightFactor;

	float4 output = inputGrayScaled * locAccentuationFactor + (1 - locAccentuationFactor) * input;
	output.a = inputAlpha;

	return output;
}


//Calculates the dot product
float DotProduct(float3 lightPos, float3 pos3D, float3 normal)
{
    float3 lightDir = normalize(pos3D - lightPos);
    return dot(-lightDir, normal);    
}