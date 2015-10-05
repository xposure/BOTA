//-----------------------------------------------------------------------------
// SpriteEffect.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#include "Macros.fxh"
DECLARE_TEXTURE(Texture, 0);

BEGIN_CONSTANTS

float4x4 World;
float4x4 View;
float4x4 Projection;

END_CONSTANTS

struct VSOutput
{
	float4 position		: SV_Position;
	float4 color		: COLOR0;
	float2 texCoord		: TEXCOORD0;
	float3 normal		: NORMAL0;
};

VSOutput SpriteVertexShader(float4 position	: SV_Position,
	float4 color : COLOR0,
	float3 normal: NORMAL0
	//float2 texCoord : TEXCOORD0
	)
{
	float4 worldPosition = mul(position, World);
	float4 viewPosition = mul(worldPosition, View);
	float4 screenPosition = mul(viewPosition, Projection);

	VSOutput output;
	output.position = screenPosition;
	output.color = color;
	//output.texCoord = texCoord;
	output.normal = normal;
	output.texCoord = position.xz;
	return output;
}


float4 SpritePixelShader(VSOutput input) : SV_Target0
{
	//return float4(1,1,1,1);
	float4 color;
	color = (SAMPLE_TEXTURE(Texture, input.texCoord) + input.color) / 2;
	color.a = 1;
	return color;
	//return input.color;
	//return SAMPLE_TEXTURE(Texture, input.texCoord) * input.color;
}

TECHNIQUE(SpriteBatch, SpriteVertexShader, SpritePixelShader);