#define SM4
#include "Macros.fxh"


float4x4 World;
float4x4 View;
float4x4 Projection;

DECLARE_TEXTURE(Texture, 0);

#ifdef XBOX
	#define FXAA_360 1
#else
	#define FXAA_PC 1
#endif
#define FXAA_HLSL_5 1
#define FXAA_GREEN_AS_LUMA 1

#include "Fxaa3_11.fxh"

struct VertexShaderInput
{
	float4 position : SV_Position0;
	float4 color : COLOR0;
	float2 texCoord : TEXCOORD0;
};


//float4 PixelShaderFunction(float4 position : SV_Position, float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
VertexShaderInput VertexShaderFunction(VertexShaderInput input)
{
    float4 worldPosition = mul(input.position, World);
    float4 viewPosition = mul(worldPosition, View);
	input.position = mul(viewPosition, Projection);

	return input;
}

float2 InverseViewportSize;
float4 ConsoleSharpness;
float4 ConsoleOpt1;
float4 ConsoleOpt2;
float SubPixelAliasingRemoval;
float EdgeThreshold;
float EdgeThresholdMin;
float ConsoleEdgeSharpness;

float ConsoleEdgeThreshold;
float ConsoleEdgeThresholdMin;

// Must keep this as constant register instead of an immediate
float4 Console360ConstDir = float4(1.0, -1.0, 0.25, -0.25);

float4 PixelShaderFunction_FXAA(VertexShaderInput input) : SV_Target0
{
	//float4 theSample = tex2D(TheSampler, input.texCoords);
	FxaaTex theSample;
	theSample.smpl = TextureSampler;
	theSample.tex = Texture;

	float4 value = FxaaPixelShader(
		input.texCoord,
		float4(0,0,0,0),	// Not used in PC or Xbox 360
		theSample,
		theSample,			// *** TODO: For Xbox, can I use additional sampler with exponent bias of -1
		theSample,			// *** TODO: For Xbox, can I use additional sampler with exponent bias of -2
		InverseViewportSize,	// FXAA Quality only
		ConsoleSharpness,		// Console only
		ConsoleOpt1,
		ConsoleOpt2,
		SubPixelAliasingRemoval,	// FXAA Quality only
		EdgeThreshold,// FXAA Quality only
		EdgeThresholdMin,
		ConsoleEdgeSharpness,
		ConsoleEdgeThreshold,	// TODO
		ConsoleEdgeThresholdMin, // TODO
		Console360ConstDir
		);

	//return float4(1, 1, 1, 1);
	return value;
}

float4 PixelShaderFunction_Standard(VertexShaderInput input) : SV_Target0
{
	return SAMPLE_TEXTURE(Texture, input.texCoord) * input.color;
	//return float4(texCoords.x, texCoords.y, 1, 1);
	//return tex2D(TheSampler, texCoords);
	//return tex2D(TheSampler, texCoords);
}

TECHNIQUE(Standard, VertexShaderFunction, PixelShaderFunction_Standard)
TECHNIQUE(FXAA, VertexShaderFunction, PixelShaderFunction_FXAA)

