sampler s;
sampler s1 : register(s1);
sampler s2 : register(s2);
sampler s3 : register(s3);
sampler s4 : register(s4);
float4x4 World;
float4x4 View;
float4x4 Projection;
float2 Viewport;

float BlockWidth;
float BlockHeight;
float AtlasWidth;
float AtlasHeight;

float NearDepth;
float FarDepth;
float DepthRange;

bool CullDark = false;
float DepthResolution;
float BorderThicknessPx = 1;
float BorderThickness = 0.0005; //0.001;
float2 BorderResolution;
float OutlineThreshold = 0.02; // if depth difference between target and surrounding pixels is larger than this, shade the pixel as part of the sprite's outline
float Zoom = 1;
float2 OutlineTolerance;
float2 GetBorderThickness()
{
	return float2(BorderThicknessPx / Viewport.x, BorderThickness / Viewport.y) * Zoom;
}

float3 PlayerGlobal;
float4 PlayerBoundingBox;
float PlayerDepth;
float RotCos;
float RotSin;
bool HideWalls;
float OcclusionRadius;
float MaxDrawLevel;
bool FogEnabled;
float FogLevel;
float4 AmbientLight;
float4 FogColor;
//float FogOffset;
float2 FogOffset;
float2 FogTextureSize;
float2 WaterTextureSize;
float2 WaterOffset;
float2 WaterOffset2;

float TileVertEnsureDraw;//for irregular blocks that extend upwards outside the cube and into the transparent area
// TODO: add effect parameters here.

float4 GetFogColor(float z)
{
	if (!FogEnabled)
		return float4(0, 0, 0, 0);
	//if (Player.Actor == null)
	//	return Color.Transparent;
	//var playerGlobal = Player.Actor.Transform.Global;// Player.Actor.Global;
	float playerz = floor(PlayerGlobal.z);
	if (playerz > 1)
		//if (z < playerGlobal.Z)
		if (z < playerz + FogLevel)
		{
			float d = abs(z - playerz + 1 - FogLevel); // the problem with that glitchy fog layer when jumping seems to be here, but i floor the player z now as a workaround anyway
			float fogDistance = 8;// 16; //5;
			d = clamp(d, 0, fogDistance) / fogDistance;
			float4 fog = float4(1, 1, 1, 1)*d;// float4(72 / 255, 61 / 255, 139 / 255, 1), d);
			//float4 fog = (1 - d)*float4(1, 1, 1, 1);
			float val = (byte)(d * 255);
			float4 finalFogColor = float4(fog.r, fog.g, fog.b, d);// val);
				//return float4(d, d, d, d);
			return finalFogColor;
		}
	return float4(0, 0, 0, 0);
}


struct MyVertexInput
{
	float4 Position : POSITION;
	float4 TexCoord : TEXCOORD0;
	float4 Fog : COLOR0;
	float4 Color : COLOR1;
	float4 Light : COLOR2;
	float4 BlockLight : TEXCOORD1;// COLOR2;
	float4 Water : TEXCOORD2;
	float4 Material : TEXCOORD3;
	float3 BlockCoords : TEXCOORD4;

	/*float Depth : TEXCOORD1;*/
};
struct MyVertexOutput
{
	float4 TexCoord : TEXCOORD0;
	float4 Position : POSITION;// POSITION0;
	float4 Fog : TEXCOORD4;
	float4 Color : COLOR0;
	float4 Light : COLOR1;
	float4 BlockLight : TEXCOORD2;// COLOR2;
	float Depth : TEXCOORD1;
	float4 ScreenPosition : TEXCOORD3;
	float4 Water : TEXCOORD5;
	float4 Material : TEXCOORD6;
	float3 BlockCoords : TEXCOORD7;
	// TODO: add vertex shader outputs such as colors and texture
	// coordinates here. These values will automatically be interpolated
	// over the triangle, and provided as input to your pixel shader.
};
struct VertexShaderInput
{
    float4 Position : POSITION;
	float4 TexCoord : TEXCOORD0;
	float4 Color : COLOR0;
	/*float4 Fog : COLOR0;
	float4 Color : COLOR1;*/


    // TODO: add input channels such as texture
    // coordinates and vertex colors here.
};
struct VertexShaderOutput
{
	float4 TexCoord : TEXCOORD0;
	float4 Position : POSITION;// POSITION0;
	float4 Color : COLOR0;
	//float4 Fog : COLOR0;
	//float4 Color : COLOR1;

	float Depth : TEXCOORD1;
    // TODO: add vertex shader outputs such as colors and texture
    // coordinates here. These values will automatically be interpolated
    // over the triangle, and provided as input to your pixel shader.
};
struct MyPixelOutput
{
	float4 Color : COLOR0;
	float4 Light : COLOR1;
	float4 Depth : COLOR2;
	float4 Fog : COLOR3;
	float DepthBuffer : DEPTH0;
};
MyVertexOutput MyChunkVertexShader(MyVertexInput input)
{
	MyVertexOutput output;
	output.ScreenPosition = float4((input.Position.xy - 0.5f) / Viewport, 0, 0);

	// these 4 lines is basically a projection matric multiplication
	/*input.Position.xy -= 0.5f;
	input.Position.xy /= Viewport;
	input.Position.xy *= float2(2, -2);
	input.Position.xy -= float2(1, -1);*/

	//output.Position = mul(input.Position, World);
	bool visible = input.BlockCoords.z < MaxDrawLevel;


	if (visible)
	{
		float4 worldPosition = mul(input.Position, World);
		float4 viewPosition = mul(worldPosition, View);
		output.Position = mul(viewPosition, Projection);
	}
	else
		output.Position = float4(0, 0, 0, 0);

	

	//output.Position = viewPosition;
	//output.Position = input.Position;// mul(viewPosition, Projection);
	//output.Position = mul(input.Position, World);

	//old output.Position.z /= (NearDepth - FarDepth);// ((FarDepth - NearDepth));
	//old output.Position.z = (FarDepth + output.Position.z) / (FarDepth + NearDepth);

	//output.Position.z = (output.Position.z - FarDepth) / (NearDepth - FarDepth);
	//output.Position.w /= (output.Position.z*2);
	output.TexCoord = input.TexCoord;
	//output.Position = visible ? output.Position : float4(0, 0, 0, 0);
	//output.Fog = FogEnabled ? input.Fog : float4(0,0,0,0);
	output.Fog = (FogEnabled && visible) ? GetFogColor(input.BlockCoords.z) : float4(0, 0, 0, 0);
	output.Color = input.Color;
	output.Light = input.Light;
	output.BlockLight = input.BlockLight;
	//output.Depth.x = 1 - input.Position.z / input.Position.w;
	//output.Depth.x = input.Position.z / input.Position.w;
	output.Depth.x = 1 - output.Position.z / output.Position.w; // w is 1 anyway
	//output.Depth.x = 1 - output.Position.z / output.Position.w;
	//output.Depth.x = output.Position.z;
	output.BlockCoords = input.BlockCoords;
	// TODO: add your vertex shader code here.
	output.Water = input.Water;
	output.Material = input.Material;
	//if (PlayerBoundingBox.x <= output.Position.x && output.Position.x < PlayerBoundingBox.z &&
	//	PlayerBoundingBox.y <= output.Position.y && output.Position.y < PlayerBoundingBox.w)
	//	output.Color = float4(0, 0, 0, 0);
	output.ScreenPosition = output.Position;
	return output;


	//output.Fog = FogEnabled ? input.Fog : float4(0,0,0,0);
	output.Fog = (FogEnabled && visible) ? GetFogColor(input.BlockCoords.z) : float4(0, 0, 0, 0);
	output.Color = visible ? input.Color : float4(0, 0, 0, 0);
	output.Light = visible ? input.Light : float4(0, 0, 0, 0);
	output.BlockLight = visible ? input.BlockLight : float4(0, 0, 0, 0);
	//output.Depth.x = 1 - input.Position.z / input.Position.w;
	//output.Depth.x = input.Position.z / input.Position.w;
	output.Depth.x = visible ? 1 - output.Position.z / output.Position.w : 2; // w is 1 anyway
	//output.Depth.x = 1 - output.Position.z / output.Position.w;
	//output.Depth.x = output.Position.z;
	output.BlockCoords = input.BlockCoords;
	// TODO: add your vertex shader code here.
	output.Water = visible ? input.Water : float4(0,0,0,0);
	output.Material = visible ? input.Material : float4(0, 0, 0, 0);
	return output;
}
MyVertexOutput MyVertexShader(MyVertexInput input)
{
	MyVertexOutput output;
	output.ScreenPosition = float4((input.Position.xy - 0.5f) / Viewport, 0, 0);

	// these 4 lines is basically a projection matrix multiplication
	input.Position.xy -= 0.5f;
	input.Position.xy /= Viewport;
	input.Position.xy *= float2(2, -2);
	input.Position.xy -= float2(1, -1);

	//float4 worldPosition = mul(input.Position, World);
	//float4 viewPosition = mul(worldPosition, View);
	//output.Position = mul(viewPosition, Projection);

	output.Position = input.Position;

	output.Position.z = (output.Position.z - FarDepth) / (NearDepth - FarDepth);
	//output.Position.w /= (output.Position.z*2);
	output.TexCoord = input.TexCoord;
	output.Fog = input.Fog;
	output.Color = input.Color;
	output.Light = input.Light;
	output.BlockLight = input.BlockLight;
	//output.Depth.x = 1 - input.Position.z / input.Position.w;
	//output.Depth.x = input.Position.z / input.Position.w;
	//output.Depth.x = output.Position.z / output.Position.w;
	output.Depth.x = 1 - output.Position.z / output.Position.w;
	output.BlockCoords = input.BlockCoords;

	// TODO: add your vertex shader code here.
	output.Water = input.Water;
	output.Material = input.Material;
	return output;
}
MyVertexOutput EntityMouseoverVertexShaderFunction(MyVertexInput input)
{
	MyVertexOutput output;
	output.ScreenPosition = float4((input.Position.xy - 0.5f) / Viewport, 0, 0);
	input.Position.xy -= 0.5f;
	input.Position.xy /= Viewport;
	input.Position.xy *= float2(2, -2);
	input.Position.xy -= float2(1, -1);

	output.Position = input.Position;
	output.TexCoord = input.TexCoord;
	output.Fog = input.Fog;
	output.Color = input.Color;
	output.Light = input.Light;
	output.BlockLight = input.BlockLight;

	output.Depth.x = output.Position.z / output.Position.w;
	output.BlockCoords = input.BlockCoords;

	output.Water = input.Water;
	output.Material = input.Material;
	return output;
}
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	input.Position.xy -= 0.5f;
	input.Position.xy /= Viewport;
	input.Position.xy *= float2(2, -2);
	input.Position.xy -= float2(1, -1);
	//input.Position.z = 2;// 0.999999999;
	//float4 worldPosition = mul(input.Position, World);
	//float4 viewPosition = mul(worldPosition, View);
	output.Position = input.Position;// mul(viewPosition, Projection);
	output.TexCoord = input.TexCoord;
	//output.Fog = input.Fog;
	output.Color = input.Color;
	//output.Depth.x = 1 - input.Position.z / input.Position.w;
	output.Depth.x = input.Position.z / input.Position.w;
    // TODO: add your vertex shader code here.

    return output;
}


float4 PixelShaderFunction(VertexShaderOutput input, out float depth : DEPTH0) : COLOR0
{
	// TODO: add your pixel shader code here.
	float4 color;
	color = tex2D(s, input.TexCoord.xy);
	clip(color.a == 0 ? -1 : 1);
//	clip(color.r == 1 && color.g == 0 && color.b == 0 ? -1 : 1); // i dont remember why i put that here, but now i dont want it to clip if i write a red color (like a entity flashing red when it gets attacked)
	depth = input.Depth.r;

	// get local UV
	float u = ((input.TexCoord.x * AtlasWidth) % BlockWidth) / BlockWidth;
	float v = ((input.TexCoord.y * AtlasHeight) % BlockHeight) / BlockHeight;
	float2 uv = float2(u, v);
	// find current block face
	float4 face = tex2D(s2, uv);

	float l = 0;
	if (face.r > 0)
		l = input.Color.b;
	else if (face.g > 0)
		l = input.Color.g;
	else if (face.b > 0)
		l = input.Color.r;

	float a = input.Color.a;
	return (1-a)*color + float4(a*color.rgb*input.Color.rgb, color.a);
	//color = float4(depth, depth, depth, color.a);
	
	/*float3 rgb = color.rgb * input.Color.rgb * color.a;*/
	/*float3 rgb = (1 - a) * color.rgb + a * input.Color.rgb;*/
	float r = (1 - a) * color.r + a * input.Color.r;
	float g = (1 - a) * color.g + a * input.Color.g;
	float b = (1 - a) * color.b + a * input.Color.b;

	

	return float4(r, g, b, color.a);// float4(input.Color.rgb, 1);// float4(1, 0, 0, 1);
}

float4 BlurMore(VertexShaderOutput input) : COLOR0
{
	// TODO: add your pixel shader code here.
	float4 color;
	color = tex2D(s, input.TexCoord.xy);
	color += tex2D(s, input.TexCoord.xy + float2(0.02, 0));
	color += tex2D(s, input.TexCoord.xy - float2(0.02, 0));
	color += tex2D(s, input.TexCoord.xy + float2(0, 0.02));
	color += tex2D(s, input.TexCoord.xy - float2(0, 0.02));
	color /= 5;
	//return float4(input.TexCoord.x, input.TexCoord.y, 0, 1);// color;
	//return color;
	return float4(input.Color.r, input.Color.g, input.Color.b, color.a);
}

float4 SolidBorder(VertexShaderOutput input) : COLOR0
{
	// TODO: add your pixel shader code here.
	float4 color;
	color = tex2D(s, input.TexCoord.xy);
	clip(color.a == 0 ? -1 : 1);
	return float4(1, 0, 0, color.a);
}

float4 ColorKey(VertexShaderOutput input) : COLOR0
{
	// TODO: add your pixel shader code here.
	float4 color;
	color = tex2D(s, input.TexCoord.xy);
	//if (color == float4(0, 1, 0, 1))
	if (color.g == 1 && !any(color.rba))
		return float4(0, 0, 0, 0);
	clip(color.a == 0 ? -1 : 1);
	return color;
}

float4 LightFunction(VertexShaderOutput input, out float depth : DEPTH0) : COLOR0
{
	float4 color;
	color = tex2D(s, input.TexCoord.xy);
	clip(color.a == 0 ? -1 : 1);
	clip(color.r == 1 && color.g == 0 && color.b == 0 ? -1 : 1);
	depth = input.Depth.x;

	// get local UV
	float u = ((input.TexCoord.x * AtlasWidth) % BlockWidth) / BlockWidth;
	float v = ((input.TexCoord.y * AtlasHeight) % BlockHeight) / BlockHeight;
	float2 uv = float2(u, v);

		// find current block face
		float4 face = tex2D(s2, uv);
		float l = 0;

	if (v < TileVertEnsureDraw || face.r > 0)
		l = input.Color.b;
	else if (face.g > 0)
		l = input.Color.g;
	else if (face.b > 0)
		l = input.Color.r;
	//clip(l == 0 ? -1 : 1); //clip if light is zero
	return float4(l, l, l, 1);
}
float GetDepth(float depthMap, float inputDepth)
{
	//return depthMap*DepthResolution + inputDepth;
	float adjustedDepth = (depthMap - 0.5f);// d.r *0.8f - 0.4f;// (d.r - 0.5f); // bring sampled depth within range of (-0.4f,0.4) to account for depth origin being at the center of the block //(-0.5f,0.5f)
	return adjustedDepth * DepthResolution + inputDepth;//(d.r)*DepthResolution + 
}


float4 EntityShaderWithDepth(MyVertexOutput input, out float depthBuffer : DEPTH0) : COLOR0 //, out float depth : DEPTH0
{
	// TODO: add your pixel shader code here.
	float4 color, depth;
	color = tex2D(s, input.TexCoord.xy);
	float4 d = tex2D(s1, input.TexCoord.xy); // sample depth map at uv coords
	clip(color.a == 0 ? -1 : 1);

	//float adjustedDepth = (d.r - 0.5f);// d.r *0.8f - 0.4f;// (d.r - 0.5f); // bring sampled depth within range of (-0.4f,0.4) to account for depth origin being at the center of the block //(-0.5f,0.5f)
	//float depthToWrite = adjustedDepth * DepthResolution + input.Depth.x;//(d.r)*DepthResolution + 

	float depthToWrite = GetDepth(d.r, input.Depth.x);
	depthBuffer = depthToWrite;// input.Depth.x;// -DepthResolution / 2;
//	return float4(depthBuffer, depthBuffer, depthBuffer, 1);
	//return float4(input.ScreenPosition.xy, 0, 1);
	return color * AmbientLight * input.Color;// float4(input.Color.rgb, 1); //rrr, 1);
}
float4 EntityShaderWithDepthColored(MyVertexOutput input, out float depthBuffer : DEPTH0) : COLOR0 //, out float depth : DEPTH0
{
	// TODO: add your pixel shader code here.
	float4 grayscalecolor, depth;

	// we're either sampling grayscale textures of sprites, and tint them accordingly via the input color
	// , or samping colored textures and tint them via the input color which should generally be white
	grayscalecolor = tex2D(s, input.TexCoord.xy); 
	float4 d = tex2D(s1, input.TexCoord.xy); // sample depth map at uv coords
		clip(grayscalecolor.a == 0 ? -1 : 1);

	float depthToWrite = GetDepth(d.r, input.Depth.x);
	depthBuffer = depthToWrite;
	//return input.Light;
	//return input.Fog;

	float4 outcol = grayscalecolor * input.Color;

		// add lighting
	float4 maxlight = float4(max(input.Light.r, input.BlockLight.r), max(input.Light.g, input.BlockLight.g), max(input.Light.b, input.BlockLight.b), 1);
	outcol *= maxlight;

	// add fog
	//float fogValue = input.Fog.a;
	//float4 fogTex = tex2D(s4, float2(input.TexCoord.x + FogOffset, input.TexCoord.y));
	//float4 fogFinal = fogTex * (1 - fogValue);// +FogColor * fog.r; // comment to make fog color white
	//outcol = outcol * (1 - fogValue) + fogFinal*fogValue;// fogTex*FogColor*fog.r;

	// TODO: draw entities before compositing final scene so i can sample the fog texture at the correct screen coordinates
	float fogValue = input.Fog.a;
	float4 fog = FogColor * fogValue;
	outcol += fog;

	return outcol;
}
float4 ParticleShader(MyVertexOutput input, out float depthBuffer : DEPTH0, out float4 fog : COLOR1, out float4 depthmap : COLOR2) : COLOR0 //, out float depth : DEPTH0
{
	float4 grayscalecolor, depth;

	// we're either sampling grayscale textures of sprites, and tint them accordingly via the input color
	// , or samping colored textures and tint them via the input color which should generally be white
	grayscalecolor = tex2D(s, input.TexCoord.xy);
	float4 d = tex2D(s1, input.TexCoord.xy); // sample entity's depth map at uv coords
		clip(grayscalecolor.a == 0 ? -1 : 1);

	//float depthToWrite = GetDepth(d.r, input.Depth.x);
	float depthToWrite = input.Depth.x;
	depthBuffer = depthToWrite;
	depthmap = float4(depthToWrite, depthToWrite, depthToWrite, 1);
	//float4 outcol = grayscalecolor * input.Color;

	// TODO: pass shininess to shader
	float shininess = input.Color.a;
	float shine = grayscalecolor.r * shininess;
	shine = pow(shine, 3);
	//float4 outcol = shininess; // OMG!!! COOL EFFECT FOR GHOSTS (because of the alpha)
	float4 outcol = grayscalecolor * shine + (grayscalecolor * input.Color) * (1 - shine); //float4(shininess, shininess, shininess, 1);// 
		outcol.a = 1; // make output alpha 1 again because i'm passing shininess factor through the input color's alpha channel
	outcol = input.Color * grayscalecolor;

	// add lighting
	float4 maxlight = float4(max(input.Light.r, input.BlockLight.r), max(input.Light.g, input.BlockLight.g), max(input.Light.b, input.BlockLight.b), 1);
		outcol *= maxlight;

	// add fog
	//float fogValue = input.Fog.a;
	//float4 fogTex = tex2D(s4, float2(input.TexCoord.x + FogOffset, input.TexCoord.y));
	//float4 fogFinal = fogTex * (1 - fogValue);// +FogColor * fog.r; // comment to make fog color white
	//outcol = outcol * (1 - fogValue) + fogFinal*fogValue;// fogTex*FogColor*fog.r;

	// TODO: draw entities before compositing final scene so i can sample the fog texture at the correct screen coordinates
	float fogValue = input.Fog.a;
	fog = float4(fogValue, 0, 0, 1);// input.Fog.r;
	//float4 fog = FogColor * fogValue;
	//	outcol += fog;
	return outcol;
}

float4 mixWithMaterial(float4 color, float4 material, float shininess)
{
	float grayscale = (color.r + color.g + color.b) / 3.0f;
	float shine = grayscale * shininess;
	shine = pow(shine, 3);
	//float4 outcol = shininess; // OMG!!! COOL EFFECT FOR GHOSTS (because of the alpha)
	float4 outcol = color * shine + (color * material) * (1 - shine); //float4(shininess, shininess, shininess, 1);// 
		outcol.a = 1;
	return outcol;
}
float4 mixWithMaterial(float4 color, float4 material)
{
	float shininess = material.a;
	float grayscale = (color.r + color.g + color.b) / 3.0f;
	float pixelshine = grayscale * shininess;
	pixelshine = pow(pixelshine, 3);
	//float4 outcol = shininess; // OMG!!! COOL EFFECT FOR GHOSTS (because of the alpha)
	float blend = 1;// .9f;// .66f;
	float4 targetcolor = color *(1 - blend) + blend*color * float4(material.rgb, 1);// *shine + (color * float4(material.rgb, 1)) * (1 - shine); //float4(shininess, shininess, shininess, 1);// 
		float4 outcol = color * pixelshine + (targetcolor)* (1 - pixelshine); //float4(shininess, shininess, shininess, 1);// 

		//float4 outcol = color * shine + (color * material) * (1 - shine); //float4(shininess, shininess, shininess, 1);// 

		outcol.a = 1;
	return outcol;
}
//float4 EntitiesFogShader(MyVertexOutput input, out float depthBuffer : DEPTH0, out float4 fog : COLOR1) : COLOR0 //, out float depth : DEPTH0
float4 EntitiesFogShader(MyVertexOutput input, out float depthBuffer : DEPTH0, out float4 fog : COLOR1, out float4 depthmap : COLOR2) : COLOR0 //, out float depth : DEPTH0
{
	float4 grayscalecolor, depth;

	// we're either sampling grayscale textures of sprites, and tint them accordingly via the input color
	// , or samping colored textures and tint them via the input color which should generally be white
	grayscalecolor = tex2D(s, input.TexCoord.xy);
	float4 d = tex2D(s1, input.TexCoord.xy); // sample entity's depth map at uv coords
		clip(grayscalecolor.a == 0 ? -1 : 1);

	float depthToWrite = GetDepth(d.r, input.Depth.x);
	depthBuffer = depthToWrite;
	depthmap = float4(depthToWrite, depthToWrite, depthToWrite, 1);
	//float4 outcol = grayscalecolor * input.Color;

		// TODO: pass shininess to shader
	//float shininess = input.Color.a;
	//float shine = shininess * (grayscalecolor.r + grayscalecolor.g + grayscalecolor.b) / 3.0f; //grayscalecolor.r * shininess;
	//shine = pow(shine, 3);
	////float4 outcol = shininess; // OMG!!! COOL EFFECT FOR GHOSTS (because of the alpha)
	//float4 outcol = grayscalecolor * shine + (grayscalecolor * input.Color) * (1 - shine); //float4(shininess, shininess, shininess, 1);// 
	//	outcol.a = 1; // make output alpha 1 again because i'm passing shininess factor through the input color's alpha channel

	float4 outcol = mixWithMaterial(grayscalecolor, input.Material);
		//outcol = outcol * (1 - input.Color.a) + float4(input.Color.rgb, 1) *  input.Color.a;
		//outcol *= input.Color.a;
		outcol *= input.Color;

		// add lighting
		float4 maxlight = float4(max(input.Light.r, input.BlockLight.r), max(input.Light.g, input.BlockLight.g), max(input.Light.b, input.BlockLight.b), 1);
		outcol *= maxlight;

	// add fog
	//float fogValue = input.Fog.a;
	//float4 fogTex = tex2D(s4, float2(input.TexCoord.x + FogOffset, input.TexCoord.y));
	//float4 fogFinal = fogTex * (1 - fogValue);// +FogColor * fog.r; // comment to make fog color white
	//outcol = outcol * (1 - fogValue) + fogFinal*fogValue;// fogTex*FogColor*fog.r;

	// TODO: draw entities before compositing final scene so i can sample the fog texture at the correct screen coordinates
	float fogValue = input.Fog.a;
	fog = float4(fogValue, 0, 0, 1);// input.Fog.r;
	//float4 fog = FogColor * fogValue;
	//	outcol += fog;
	return outcol;
}
float4 EntitiesUIShader(MyVertexOutput input, out float depthBuffer : DEPTH0) : COLOR0 //, out float depth : DEPTH0
{
	depthBuffer = 0;
	return float4(1, 0, 0, 1);
	//float4 grayscalecolor, depth;

	//// we're either sampling grayscale textures of sprites, and tint them accordingly via the input color
	//// , or samping colored textures and tint them via the input color which should generally be white
	//grayscalecolor = tex2D(s, input.TexCoord.xy);
	//float4 d = tex2D(s1, input.TexCoord.xy); // sample entity's depth map at uv coords
	//	clip(grayscalecolor.a == 0 ? -1 : 1);

	//float depthToWrite = GetDepth(d.r, input.Depth.x);
	//depthBuffer = depthToWrite;

	////float4 outcol = grayscalecolor * input.Color;

	//// TODO: pass shininess to shader
	//float shininess = input.Color.a;
	//float shine = grayscalecolor.r * shininess;
	//shine = pow(shine, 3);
	////float4 outcol = shininess; // OMG!!! COOL EFFECT FOR GHOSTS (because of the alpha)
	//float4 outcol = grayscalecolor * shine + (grayscalecolor * input.Color) * (1 - shine); //float4(shininess, shininess, shininess, 1);// 
	//	outcol.a = 1; // make output alpha 1 again because i'm passing shininess factor through the input color's alpha channel

	//// add lighting
	//float4 maxlight = float4(max(input.Light.r, input.BlockLight.r), max(input.Light.g, input.BlockLight.g), max(input.Light.b, input.BlockLight.b), 1);
	//	outcol *= maxlight;

	//// add fog
	////float fogValue = input.Fog.a;
	////float4 fogTex = tex2D(s4, float2(input.TexCoord.x + FogOffset, input.TexCoord.y));
	////float4 fogFinal = fogTex * (1 - fogValue);// +FogColor * fog.r; // comment to make fog color white
	////outcol = outcol * (1 - fogValue) + fogFinal*fogValue;// fogTex*FogColor*fog.r;

	//// TODO: draw entities before compositing final scene so i can sample the fog texture at the correct screen coordinates
	//float fogValue = input.Fog.a;
	//fog = float4(fogValue, 0, 0, 1);// input.Fog.r;
	////float4 fog = FogColor * fogValue;
	////	outcol += fog;
	//return outcol;
}
float4 EntityShader(VertexShaderOutput input, out float depth : DEPTH0) : COLOR0 //, out float depth : DEPTH0
{
	// TODO: add your pixel shader code here.
	float4 color;
	color = tex2D(s, input.TexCoord.xy);
	clip(color.a == 0 ? -1 : 1);
	//clip(color.a == 0 ? -1 : 1);
	//clip(color.r == 1 && color.g == 0 && color.b == 0 ? -1 : 1);
	//depth = 0;// input.Depth.r;
	//depth = input.Depth.x;
	//return input.Depth.x;
	depth = input.Depth.x;// -DepthResolution / 2;
	//return float4(depth, depth, depth, 1);
	return color * AmbientLight;
	

	float a = input.Color.a;
	return (1 - a)*color + float4(a*color.rgb*input.Color.rgb, color.a);
}

float4 FinalOutsideBorders(VertexShaderOutput input, out float depthBuffer : DEPTH0) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	float4 light = tex2D(s1, input.TexCoord.xy);
	float depth = tex2D(s2, input.TexCoord.xy);
	depthBuffer = depth;
	//clip(color.a == 0 ? -1 : 1);

	//float borderThickness = 0.003;
	float borderThreshold = 0.02;
	float dtop = tex2D(s2, input.TexCoord.xy + float2(0, -BorderThickness));
	float dbottom = tex2D(s2, input.TexCoord.xy + float2(0, BorderThickness));
	float dleft = tex2D(s2, input.TexCoord.xy + float2(-BorderThickness, 0));
	float dright = tex2D(s2, input.TexCoord.xy + float2(BorderThickness, 0));
	//float4 border = float4(1, 1, 1, 1);
	float4 border;
	float diff;

	diff = depth.r - dtop.r;
	diff = max(diff, depth.r - dbottom.r);
	diff = max(diff, depth.r - dleft.r);
	diff = max(diff, depth.r - dright.r);

	if (diff > borderThreshold)
	{
		float distance = diff - borderThreshold;
		float b = 1 - distance / DepthResolution;
		return ((1 - b) * float4(0, 0, 0, 1) + (b)* color*light);
		//return float4(b, b, b, 1) * float4(color.rgb, 1);// *light * AmbientLight;
	}
		//return float4(0, 0, 0, 1);
		return color.a * color * light * AmbientLight; //float4(depth, depth, depth, 1);//  border * 

		if (
			depth.r > dtop.r + BorderThickness ||
			depth.r > dbottom.r + BorderThickness ||
			depth.r > dleft.r + BorderThickness ||
			depth.r > dright.r + BorderThickness)
			/*depth.r < dtop.r - borderThreshold ||
			depth.r < dbottom.r - borderThreshold ||
			depth.r < dleft.r - borderThreshold ||
			depth.r < dright.r - borderThreshold)*/
		{
		depthBuffer = depth - borderThreshold;
		return float4(0, 0, 0, 1);

		depthBuffer = depth - borderThreshold;
		//border = float4(0, 0, 0, 1)*  (dright.r - borderThreshold);
		float factor = dright.r - borderThreshold;
		border = (1-factor)*color + factor*float4(0, 0, 0, 1);
		return color.a *border;
		}

	//return float4(depth, depth, depth, 1);// 
	return color.a * color * light * AmbientLight; //float4(depth, depth, depth, 1);//  border * 
	
}

float4 FogWaterShader(VertexShaderOutput input, out float depthBuffer : DEPTH0) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	float4 fog = tex2D(s1, input.TexCoord.xy);

	//sample depth from depth texture and write it to render target
	float4 depth = tex2D(s3, input.TexCoord.xy);
	float d = depth.r;
	depthBuffer = d;

	//water here
	//float4 watermap = tex2D(s3, input.TexCoord.xy);
	float2 wateruv = Viewport / (WaterTextureSize * Zoom);
	wateruv *= (0.5f - input.TexCoord.xy);
	float4 water = tex2D(s4, float2(wateruv.x, wateruv.y) + WaterOffset);
		float wateramount = fog.b * 0.5f;// watermap.b *0.5f;
	color = color * (1 - wateramount) + water * wateramount;
	//
	float2 foguv = Viewport / (FogTextureSize * Zoom);

		foguv *= (0.5f - input.TexCoord.xy);
	float4 fogTex = tex2D(s2, float2(foguv.x, foguv.y) + FogOffset);

		float value = fog.r;
	value = max(fog.r, 1 - color.a);
	float4 fogFinal = fogTex * (1 - value);// +FogColor * fog.r; // comment to make fog color white
		float4 afterFog = color * (1 - value) + fogFinal*value;// fogTex*FogColor*fog.r;

		// effect for reducing color depth
	/*		float r = round(afterFog.r * 8) / 8;
		float g = round(afterFog.g * 8) / 8;
		float b = round(afterFog.b * 4) / 4;
		afterFog = float4(r, g, b, afterFog.a);*/


		//	float water = fog.b;
		//afterFog = afterFog * (1 - water) + float4(0, 0, 1, 1) * water;

		return afterFog;
}

float4 MapDump(VertexShaderOutput input, out float depthBuffer : DEPTH0, out float4 fogoutput : COLOR1) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	//clip(color.g == 1 ? -1 : 1);
	float4 fog = tex2D(s1, input.TexCoord.xy);
	//return float4(0, 1 - color.a, 0, 1);

	//sample depth from depth texture and write it to render target
	float4 depth = tex2D(s3, input.TexCoord.xy);
	float d = depth.r;
	depthBuffer = d;
	fogoutput = float4(fog.r, 0, 0, 1);
	return color;
}
float4 ApplyFogShader(VertexShaderOutput input, out float depthBuffer : DEPTH0) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	//clip(color.g == 1 ? -1 : 1);
	float4 fog = tex2D(s1, input.TexCoord.xy);
	//return float4(0, 1 - color.a, 0, 1);

	//sample depth from depth texture and write it to render target
	float4 depth = tex2D(s3, input.TexCoord.xy);
	float d = depth.r;
	depthBuffer = d;
	//return float4(d, d, d, 1);

	//float2 foguv = Viewport / FogTextureSize;
	float2 foguv = Viewport / (FogTextureSize * Zoom);

		foguv *= (0.5f - input.TexCoord.xy);
	//float4 fogTex = tex2D(s2, float2(foguv.x + FogOffset, foguv.y));
	float4 fogTex = tex2D(s2, float2(foguv.x, foguv.y) + FogOffset);

		//return fogTex;
		//return fog;
		float value = fog.r;
	value = max(fog.r, 1 - color.a);
	float4 fogFinal = fogTex * (1 - value);// +FogColor * fog.r; // comment to make fog color white
		float4 afterFog = color * (1 - value) + fogFinal*value;// fogTex*FogColor*fog.r;

	//	 //effect for reducing color depth
	//	 float r = round(afterFog.r * 16) / 16;// 8) / 8;
	//float g = round(afterFog.g * 16) / 16;//8) / 8;
	//float b = round(afterFog.b * 16) / 16;//8) / 8; //4) / 4;
	//	afterFog = float4(r, g, b, afterFog.a);


		//	float water = fog.b;
		//afterFog = afterFog * (1 - water) + float4(0, 0, 1, 1) * water;

		return afterFog;
}
float4 FogShader(VertexShaderOutput input, out float depthBuffer : DEPTH0) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	//clip(color.g == 1 ? -1 : 1);
	float4 fog = tex2D(s1, input.TexCoord.xy);
	//return float4(0, 1 - color.a, 0, 1);

	//sample depth from depth texture and write it to render target
	float4 depth = tex2D(s3, input.TexCoord.xy);
	float d = depth.r;
	depthBuffer = d;
	//return float4(d, d, d, 1);

	//float2 foguv = Viewport / FogTextureSize;
	float2 foguv = Viewport / (FogTextureSize * Zoom);

	foguv *= (0.5f - input.TexCoord.xy);
	//float4 fogTex = tex2D(s2, float2(foguv.x + FogOffset, foguv.y));
	float4 fogTex = tex2D(s2, float2(foguv.x, foguv.y) + FogOffset);

		//return fogTex;
		//return fog;
		float value = fog.r;
	value = max(fog.r, 1 - color.a);
	float4 fogFinal = fogTex * (1 - value);// +FogColor * fog.r; // comment to make fog color white
		float4 afterFog = color * (1 - value) + fogFinal*value;// fogTex*FogColor*fog.r;

		// effect for reducing color depth
		//	float r = round(afterFog.r * 8) / 8;
		//float g = round(afterFog.g * 8) / 8;
		//float b = round(afterFog.b * 4) / 4;
		//afterFog = float4(r, g, b, afterFog.a);


	//	float water = fog.b;
	//afterFog = afterFog * (1 - water) + float4(0, 0, 1, 1) * water;

	return afterFog;
}

float4 FinalWaterShader(VertexShaderOutput input, out float depthBuffer : DEPTH0, out float4 fogoutput : COLOR1) : COLOR0
{
	//float4 color = tex2D(s, input.TexCoord.xy);
	//clip(color.a == 0 ? -1 : 1);

	////sample depth from depth texture and write it to render target
	//float4 depth = tex2D(s3, input.TexCoord.xy);
	//float d = depth.r;
	//depthBuffer = d;
	//
	//return color * 0.5f;
	
	/**/

	float4 color = tex2D(s, input.TexCoord.xy);
	float4 fog = tex2D(s1, input.TexCoord.xy);
	//color;// *= 0.5f;

	//sample depth from depth texture and write it to render target
	float4 depth = tex2D(s3, input.TexCoord.xy);
	float d = depth.r;
	depthBuffer = d;
	clip(fog.r == 1 ? -1 : 1);
	fogoutput = float4(fog.r, 0, 0, 1);
	return color;// *0.5f; // i set the alpha in the compositewater technique because its value depends on block face

	float fogvalue = fog.r;
	fogvalue = max(fog.r, 1 - color.a);
	color *= (1-fogvalue);
	//color.a = 0.5f;
	color *= 0.5f; // uncomment to make water transparent
	//color = float4(fogvalue, 0, 0, 1);
	return color;

	float2 foguv = Viewport / (FogTextureSize * Zoom);

		foguv *= (0.5f - input.TexCoord.xy);
	float4 fogTex = tex2D(s2, float2(foguv.x, foguv.y) + FogOffset);

		float value = fog.r;
	value = max(fog.r, 1 - color.a);
	float4 fogFinal = fogTex * (1 - value);// +FogColor * fog.r; // comment to make fog color white
		float4 afterFog = color * (1 - value) + fogFinal*value;


		return afterFog;
}


float4 FinalInsideBordersFunction(VertexShaderOutput input, out float depthBuffer : DEPTH0) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	float4 light = tex2D(s1, input.TexCoord.xy);
	float depth = tex2D(s2, input.TexCoord.xy);
	float4 fog = tex2D(s3, input.TexCoord.xy);
		//float4 fogTex = tex2D(s4, float2(input.TexCoord.x + FogOffset, input.TexCoord.y));// input.TexCoord.xy);
		float4 fogTex = tex2D(s4, float2(input.TexCoord.x, input.TexCoord.y) + FogOffset);// input.TexCoord.xy);

	depthBuffer = depth;
	//clip(light == 0 ? -1 : 1);
	float dtop = tex2D(s2, input.TexCoord.xy + float2(0, -BorderResolution.y));
	float dbottom = tex2D(s2, input.TexCoord.xy + float2(0, BorderResolution.y));
	float dleft = tex2D(s2, input.TexCoord.xy + float2(-BorderResolution.x, 0));
	float dright = tex2D(s2, input.TexCoord.xy + float2(BorderResolution.x, 0));
	float diff;

	diff = dtop.r - depth.r;
	diff = max(diff, dbottom.r - depth.r);
	diff = max(diff, dleft.r - depth.r);
	diff = max(diff, dright.r - depth.r);

	if (diff > OutlineThreshold)// OutlineThreshold)
	{
		float b = 0.5f;
		float4 outline = ((1 - b) * float4(0, 0, 0, 1) + (b)* color*light) * AmbientLight;
		return outline * (1 - fog.r) + FogColor*fog.r;
		return ((1 - b) * float4(0, 0, 0, 1) + (b)* color*light) * AmbientLight;
		//return float4(b, b, b, 1) * float4(color.rgb, 1);// *light * AmbientLight;
	}

	/*float rad = 0.001 *Zoom;
	color += tex2D(s, float2(input.TexCoord.x - rad, input.TexCoord.y - rad));
	color += tex2D(s, float2(input.TexCoord.x + rad, input.TexCoord.y - rad));
	color += tex2D(s, float2(input.TexCoord.x - rad, input.TexCoord.y + rad));
	color += tex2D(s, float2(input.TexCoord.x + rad, input.TexCoord.y + rad));
	color /= 5;*/

	/*float4 tl = tex2D(s, float2(input.TexCoord + float2(-1/Viewport.x, -1/Viewport.y)));
	float4 tc = tex2D(s, float2(input.TexCoord + float2(0, -1 / Viewport.y)));
	float4 tr = tex2D(s, float2(input.TexCoord + float2(1 / Viewport.x, -1 / Viewport.y)));

	float4 cl = tex2D(s, float2(input.TexCoord + float2(-1 / Viewport.x, 0)));
	float4 cc = tex2D(s, float2(input.TexCoord + float2(0, 0)));
	float4 cr = tex2D(s, float2(input.TexCoord + float2(-1 / Viewport.x, 0)));

	float4 bl = tex2D(s, float2(input.TexCoord + float2(-1 / Viewport.x, 1 / Viewport.y)));
	float4 bc = tex2D(s, float2(input.TexCoord + float2(0, 1 / Viewport.y)));
	float4 br = tex2D(s, float2(input.TexCoord + float2(1 / Viewport.x, 1 / Viewport.y)));

	return (tl + tc + tr + cl + cc + cr + bl + bc + br) / 9;*/

	float4 beforeFog = color.a * color *light;
	// DONT ADD FOG HERE, ADD LATER AFTER DRAWING ENTITIES
	float4 fogFinal = fogTex * (1 - fog.r);// +FogColor * fog.r; // comment to make fog color white
	float4 afterFog = beforeFog * (1 - fog.r) + fogFinal*fog.r;// fogTex*FogColor*fog.r;
	return afterFog;

	return color.a * color *light;// *AmbientLight; //float4(depth, depth, depth, 1);//  border * 
}
float4 FinalInsideBordersFunctionNoFog(VertexShaderOutput input, out float depthBuffer : DEPTH0) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	float4 light = tex2D(s1, input.TexCoord.xy);
	float depth = tex2D(s2, input.TexCoord.xy);
	depthBuffer = depth;


		// DONT DRAW WATER HERE, DRAW IT INSIDE FOG SHADER INSTEAD
	/*float4 watermap = tex2D(s3, input.TexCoord.xy);
	float2 wateruv = Viewport / (WaterTextureSize * Zoom);
	wateruv *= (0.5f - input.TexCoord.xy);
	float4 water = tex2D(s4, float2(wateruv.x, wateruv.y) + WaterOffset);
	float wateramount = watermap.b *0.5f;
	color = color * (1 - wateramount) + water * wateramount;*/

	float dtop = tex2D(s2, input.TexCoord.xy + float2(0, -BorderResolution.y));
	float dbottom = tex2D(s2, input.TexCoord.xy + float2(0, BorderResolution.y));
	float dleft = tex2D(s2, input.TexCoord.xy + float2(-BorderResolution.x, 0));
	float dright = tex2D(s2, input.TexCoord.xy + float2(BorderResolution.x, 0));
	float diff;

	diff = dtop.r - depth.r;
	diff = max(diff, dbottom.r - depth.r);
	diff = max(diff, dleft.r - depth.r);
	diff = max(diff, dright.r - depth.r);

	if (diff > OutlineThreshold)// OutlineThreshold)
	{
		float b = 0;// 0.5f; //outline transparency
		float4 outline = ((1 - b) * float4(0, 0, 0, 1) + (b)* color*light) * AmbientLight;
			return outline;
		//	return outline * (1 - fog.r) + FogColor*fog.r;
		//return ((1 - b) * float4(0, 0, 0, 1) + (b)* color*light) * AmbientLight;
	}

	float4 beforeFog = color.a * color *light;


	//	//effect for reducing color depth
	//	float r = round(beforeFog.r * 8) / 8;
	//float g = round(beforeFog.g * 8) / 8;
	//float b = round(beforeFog.b * 8) / 8; //4) / 4;
	//beforeFog = float4(r, g, b, beforeFog.a);

		return beforeFog;
		// DONT ADD FOG HERE, ADD LATER AFTER DRAWING ENTITIES

	//	float4 fogFinal = fogTex * (1 - fog.r);// +FogColor * fog.r; // comment to make fog color white
	//	float4 afterFog = beforeFog * (1 - fog.r) + fogFinal*fog.r;
	//	return afterFog;

	//return color.a * color *light;
}

float4 CompositeWaterShader(VertexShaderOutput input, out float depthBuffer : DEPTH0) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	float4 light = tex2D(s1, input.TexCoord.xy);
	float depth = tex2D(s2, input.TexCoord.xy);
	depthBuffer = depth;

	// DONT DRAW WATER HERE, DRAW IT INSIDE FOG SHADER INSTEAD
	float4 watermap = tex2D(s3, input.TexCoord.xy);
	float2 wateruv = Viewport / (WaterTextureSize * Zoom);
	wateruv *= (0.5f - input.TexCoord.xy);
		
	float4 water = tex2D(s4, float2(wateruv.x, wateruv.y) + WaterOffset);
		water += tex2D(s4, float2(wateruv.x, wateruv.y) + WaterOffset2); //+float2(-WaterOffset.y, WaterOffset.x));

		/*	float2 wateruv2 = Viewport / (WaterTextureSize * Zoom);
			wateruv2 *= (0.5f - input.TexCoord.xy)* 2.0f;
			float4 water2 = tex2D(s4, (wateruv2 + WaterOffset) / 2.0f);
			*/

			//float wateramount = watermap.b *0.5f;
			//color = color * (1 - wateramount) + water * wateramount;
			//color = water;// *water2;// *0.5f;

	float4 outcolor;
	if (watermap.b == 1) // if the blue component of the watermaptexture is 1, it means we are drawing water
	{
		//float4 outcolor = water;
		outcolor = water;

		outcolor.a = .5f;
		if (color.g == 1)
		{
			outcolor.rgb *= .5f;
			outcolor.a = .75f;
		}
		if (color.b == 1)
		{
			outcolor.rgb *= .25f;
			outcolor.a = .9f;
		}
	}
	else //otherwise, we are drawing a regular block, so make it transparent (here? or pass the alpha value through the vertex?)
	{
		outcolor = float4(color.rgb, .5f);// color*.5f;// float4(1, 0, 0, .5f);// color * .2f;
	}
	//outcolor = float4(watermap.bbb, 1);
	float dtop = tex2D(s2, input.TexCoord.xy + float2(0, -BorderResolution.y));
	float dbottom = tex2D(s2, input.TexCoord.xy + float2(0, BorderResolution.y));
	float dleft = tex2D(s2, input.TexCoord.xy + float2(-BorderResolution.x, 0));
	float dright = tex2D(s2, input.TexCoord.xy + float2(BorderResolution.x, 0));
	float diff;

	diff = dtop.r - depth.r;
	diff = max(diff, dbottom.r - depth.r);
	diff = max(diff, dleft.r - depth.r);
	diff = max(diff, dright.r - depth.r);

	if (diff > OutlineThreshold)// OutlineThreshold)
	{
		float b = 0.5f;
		float4 outline = ((1 - b) * float4(0, 0, 0, 1) + (b)* outcolor*light) * AmbientLight;
			return outline;
		//	return outline * (1 - fog.r) + FogColor*fog.r;
		//return ((1 - b) * float4(0, 0, 0, 1) + (b)* color*light) * AmbientLight;
	}

	float4 beforeFog = outcolor.a * outcolor *light;


		//effect for reducing color depth
	//	float r = round(beforeFog.r * 8) / 8;
	//float g = round(beforeFog.g * 8) / 8;
	//float b = round(beforeFog.b * 8) / 8; //4) / 4;
	//beforeFog = float4(r, g, b, beforeFog.a);

		return beforeFog;
	// DONT ADD FOG HERE, ADD LATER AFTER DRAWING ENTITIES

	//	float4 fogFinal = fogTex * (1 - fog.r);// +FogColor * fog.r; // comment to make fog color white
	//	float4 afterFog = beforeFog * (1 - fog.r) + fogFinal*fog.r;
	//	return afterFog;

	//return color.a * color *light;
}

MyPixelOutput MyPixelShader(MyVertexOutput input)
{
	MyPixelOutput output;
	float4 color = tex2D(s, input.TexCoord.xy);
		clip(color.a == 0 ? -1 : 1);
	clip(color.r == 1 && color.g == 0 && color.b == 0 ? -1 : 1);

	// TODO: pack flags instead of color in vertex for lit face (FASTER!)
	// get local UV
	float u = ((input.TexCoord.x * AtlasWidth) % BlockWidth) / BlockWidth;
	float v = ((input.TexCoord.y * AtlasHeight) % BlockHeight) / BlockHeight;
	float2 uv = float2(u, v);
		float4 face = tex2D(s2, uv);
		float sunlight = 0;
	float blocklight;
	if (v < TileVertEnsureDraw || face.r > 0)
	{
		sunlight = input.Light.b;
		blocklight = input.BlockLight.b;
	}
	else if (face.g > 0)
	{
		sunlight = input.Light.g;
		blocklight = input.BlockLight.g;
	}
	else if (face.b > 0)
	{
		sunlight = input.Light.r;
		blocklight = input.BlockLight.r;
	}
	/*if (CullDark)
		clip(sunlight == 0 ? -1 : 1);*/
	float a = input.Color.a;
	float4 d = tex2D(s3, uv);
		float depthToWrite = GetDepth(d.r, input.Depth.x);
	float fog = input.Fog.a;
	//color = color * (1 - fog) + float4(input.Fog.xyz, 1) * fog;

	output.Fog = float4(fog, 0, input.Water.r, 1);// float4(input.Fog.xyz * fog, 1); //
	//color = color * (1 - fog) + output.Fog;
	//color = color * (1 - fog) + float4(input.Fog.xyz, 1) * fog;

	if (input.Water.r > 0)
		output.Color = 0;
	else
		output.Color = d.a*((1 - a)*color + float4(a*color.rgb*input.Color.rgb, color.a));

	output.Depth = depthToWrite + float4(0, 0, 0, 1);
	output.DepthBuffer = depthToWrite;
	float4 lightcolor = sunlight * AmbientLight + float4(blocklight, blocklight, blocklight, 1);// blocklight * float4(1, 1, 1, 1);
		output.Light = lightcolor;

	// input color's alpha channel is used for shininess, DONT multiply it with output color!!!
	//output.Color = output.Color * input.Color.a;
		return output;
}

MyPixelOutput TransparentBlockShader(MyVertexOutput input)
{
	MyPixelOutput output;
	float4 color = tex2D(s, input.TexCoord.xy);
		clip(color.a == 0 ? -1 : 1);
	clip(color.r == 1 && color.g == 0 && color.b == 0 ? -1 : 1);

	// TODO: pack flags instead of color in vertex for lit face (FASTER!)

	float4 face = tex2D(s2, input.TexCoord.xy);
		float sunlight = 0;
	float blocklight;
	//if (v < TileVertEnsureDraw || 
	if (face.r > 0)
	{
		sunlight = input.Light.b;
		blocklight = input.BlockLight.b;
	}
	else if (face.g > 0)
	{
		sunlight = input.Light.g;
		blocklight = input.BlockLight.g;
	}
	else if (face.b > 0)
	{
		sunlight = input.Light.r;
		blocklight = input.BlockLight.r;
	}
	float a = input.Color.a;
	float4 d = tex2D(s3, input.TexCoord.xy);
		float depthToWrite = GetDepth(d.r, input.Depth.x);
	float fog = input.Fog.a;

	output.Fog = float4(fog, 0, input.Water.r, 1);

	output.Color = mixWithMaterial(color, input.Material);// , input.Material.a);
	//output.Color = d.a*((1 - a)*color + float4(a*color.rgb*input.Color.rgb, color.a));
	output.Color *= input.Color;
	/*if (color.a < 1)
	output.Color = float4(color.a, color.a, color.a, color.a);*/
	output.Color *= color.a;
	output.Depth = depthToWrite + float4(0, 0, 0, 1);
	output.DepthBuffer = depthToWrite;
	float4 lightcolor = sunlight * AmbientLight + float4(blocklight, blocklight, blocklight, 1);// blocklight * float4(1, 1, 1, 1);
		output.Light = lightcolor;

	// input color's alpha channel is used for shininess, DONT multiply it with output color!!!
	//output.Color = output.Color * input.Color.a;
	return output;
}

MyPixelOutput MyChunkPixelShader(MyVertexOutput input)
{
	MyPixelOutput output;
	float4 color = tex2D(s, input.TexCoord.xy);
		clip(color.a == 0 ? -1 : 1);
	clip(color.r == 1 && color.g == 0 && color.b == 0 ? -1 : 1);

	
	if (HideWalls)
		if (input.BlockCoords.z >= PlayerGlobal.z)
		{
			// TODO: optimize
			float blockx = input.BlockCoords.x * RotCos - input.BlockCoords.y * RotSin;
			float blocky = input.BlockCoords.x * RotSin + input.BlockCoords.y * RotCos;
			float playerx = PlayerGlobal.x * RotCos - PlayerGlobal.y * RotSin;
			float playery = PlayerGlobal.x * RotSin + PlayerGlobal.y * RotCos;

			//if (depthToWrite>GetDepth(.5f, PlayerDepth))
			//if (input.BlockCoords.x + input.BlockCoords.y > PlayerGlobal.x + PlayerGlobal.y)

			if (blockx + blocky > playerx + playery) //the good one
			//if ((blockx >= playerx) && (blocky >= playery)) //old one that was in camera class hidesplayer() method
				/*if (PlayerBoundingBox.x <= input.ScreenPosition.x && input.ScreenPosition.x < PlayerBoundingBox.z &&
					PlayerBoundingBox.y <= input.ScreenPosition.y && input.ScreenPosition.y < PlayerBoundingBox.w)*/
			{
				float2 positionNormalised = input.ScreenPosition.xy / float2(1, (Viewport.x / Viewport.y));
					float l = pow(positionNormalised.x, 2) + pow(positionNormalised.y, 2);
				//float r = .01f * Zoom * Zoom;
				//if (l < r)
				if (l < OcclusionRadius)
					clip(-1);
			}
		}
	// TODO: pack flags instead of color in vertex for lit face (FASTER!)

	float4 face = tex2D(s2, input.TexCoord.xy);
		float sunlight = 0;
	float blocklight;
	//if (v < TileVertEnsureDraw || 
	if (face.r > 0)
	{
		sunlight = input.Light.b;
		blocklight = input.BlockLight.b;
	}
	else if (face.g > 0)
	{
		sunlight = input.Light.g;
		blocklight = input.BlockLight.g;
	}
	else if (face.b > 0)
	{
		sunlight = input.Light.r;
		blocklight = input.BlockLight.r;
	}
	float a = input.Color.a;
	float4 d = tex2D(s3, input.TexCoord.xy);
		float depthToWrite = GetDepth(d.r, input.Depth.x);
	float fog = input.Fog.a;

	output.Fog = float4(fog, 0, input.Water.r, 1);



	/*if (input.Water.r > 0)
	output.Color = 0;
	else*/
	//output.Color = d.a*((1 - a)*color + float4(a*color.rgb*input.Color.rgb, color.a));

	output.Color = mixWithMaterial(color, input.Material);// , input.Material.a);
	//output.Color = d.a*((1 - a)*color + float4(a*color.rgb*input.Color.rgb, color.a));
	output.Color *= input.Color;
	/*if (color.a < 1)
	output.Color = float4(color.a, color.a, color.a, color.a);*/
	output.Color *= color.a;
	output.Depth = depthToWrite + float4(0, 0, 0, 1);
	output.DepthBuffer = depthToWrite;
	float4 lightcolor = sunlight * AmbientLight + float4(blocklight, blocklight, blocklight, 1);// blocklight * float4(1, 1, 1, 1);
		output.Light = lightcolor;
	//output.Color = depthToWrite + float4(0, 0, 0, 1);
	// input color's alpha channel is used for shininess, DONT multiply it with output color!!!
	//output.Color = output.Color * input.Color.a;
	return output;
}


MyPixelOutput MyPixelShaderWithNormals(MyVertexOutput input)
{
	MyPixelOutput output;
	float4 color = tex2D(s, input.TexCoord.xy);
		clip(color.a == 0 ? -1 : 1);
	clip(color.r == 1 && color.g == 0 && color.b == 0 ? -1 : 1);

	// TODO: pack flags instead of color in vertex for lit face (FASTER!)

		float4 face = tex2D(s2, input.TexCoord.xy);
		float sunlight = 0;
	float blocklight;
	//if (v < TileVertEnsureDraw || 
		if(face.r > 0)
	{
		sunlight = input.Light.b;
		blocklight = input.BlockLight.b;
	}
	else if (face.g > 0)
	{
		sunlight = input.Light.g;
		blocklight = input.BlockLight.g;
	}
	else if (face.b > 0)
	{
		sunlight = input.Light.r;
		blocklight = input.BlockLight.r;
	}
	float a = input.Color.a;
	float4 d = tex2D(s3, input.TexCoord.xy);
		float depthToWrite = GetDepth(d.r, input.Depth.x);
	float fog = input.Fog.a;

	output.Fog = float4(fog, 0, input.Water.r, 1);



	/*if (input.Water.r > 0)
		output.Color = 0;
	else*/
		//output.Color = d.a*((1 - a)*color + float4(a*color.rgb*input.Color.rgb, color.a));

	output.Color = mixWithMaterial(color, input.Material);// , input.Material.a);
	//output.Color = d.a*((1 - a)*color + float4(a*color.rgb*input.Color.rgb, color.a));
	output.Color *= input.Color;
	/*if (color.a < 1)
		output.Color = float4(color.a, color.a, color.a, color.a);*/
	output.Color *= color.a;
	output.Depth = depthToWrite + float4(0, 0, 0, 1);
	output.DepthBuffer = depthToWrite;
	float4 lightcolor = sunlight * AmbientLight + float4(blocklight, blocklight, blocklight, 1);// blocklight * float4(1, 1, 1, 1);
		output.Light = lightcolor;

	// input color's alpha channel is used for shininess, DONT multiply it with output color!!!
	//output.Color = output.Color * input.Color.a;
	return output;
}

MyPixelOutput MyWaterShader(MyVertexOutput input)
{
	MyPixelOutput output;
	float4 color = tex2D(s, input.TexCoord.xy);
		clip(color.a == 0 ? -1 : 1);
	clip(color.r == 1 && color.g == 0 && color.b == 0 ? -1 : 1);

	// TODO: pack flags instead of color in vertex for lit face (FASTER!)

	float4 face = tex2D(s2, input.TexCoord.xy);
		float sunlight = 0;
	float blocklight;
	float waterfx = 0; // draw water texture later only on the top face
	//if (v < TileVertEnsureDraw || 
	if (face.r > 0)
	{
		sunlight = input.Light.b;
		blocklight = input.BlockLight.b;
		waterfx = 1; // draw water texture later only on the top face
	}
	else if (face.g > 0)
	{
		sunlight = input.Light.g;
		blocklight = input.BlockLight.g;
	}
	else if (face.b > 0)
	{
		sunlight = input.Light.r;
		blocklight = input.BlockLight.r;
	}
	float a = input.Color.a;
	float4 d = tex2D(s3, input.TexCoord.xy);
		float depthToWrite = GetDepth(d.r, input.Depth.x);
	float fog = input.Fog.a;
	output.Fog = float4(fog, 0, input.Water.r, 1); // draw water texture later only on the top face

	//output.Fog = float4(fog, 0, input.Water.r * waterfx, 1); // draw water texture later only on the top face
	//output.Fog = float4(fog, 0, 0, 1);


	/*if (input.Water.r > 0)
	output.Color = 0;
	else*/
	//if (face.r>0)

	if (input.Water.r == 1)
		color.rgb = face.rgb; // i added this so i can draw individual water faces differently later in the water texture shader
	else // if we aren't drawing water, mix color with input material
		color = mixWithMaterial(color, input.Material);

	//color.rgb = input.Fog.b;
	output.Color = d.a*((1 - a)*color + float4(a*color.rgb*input.Color.rgb, color.a));
	output.Color *= input.Color.a;// float4(color.rgb, 1);
	//output.Color *= (1 - waterfx);
	//output.Color.a = 1;

	output.Depth = depthToWrite + float4(0, 0, 0, 1);
	output.DepthBuffer = depthToWrite;
	float4 lightcolor = sunlight * AmbientLight + float4(blocklight, blocklight, blocklight, 1);// blocklight * float4(1, 1, 1, 1);
		output.Light = lightcolor;

	// input color's alpha channel is used for shininess, DONT multiply it with output color!!!
	//output.Color = output.Color * input.Color.a;
	return output;
}


MyPixelOutput MyPixelShader2(MyVertexOutput input)
{
	MyPixelOutput output;
	float4 color = tex2D(s, input.TexCoord.xy);
	clip(color.a == 0 ? -1 : 1);
	clip(color.r == 1 && color.g == 0 && color.b == 0 ? -1 : 1);

	// TODO: pack flags instead of color in vertex for lit face (FASTER!)
	// get local UV
	float u = ((input.TexCoord.x * AtlasWidth) % BlockWidth) / BlockWidth;
	float v = ((input.TexCoord.y * AtlasHeight) % BlockHeight) / BlockHeight;
	float2 uv = float2(u, v);

	// find current block face
	float4 face = tex2D(s2, uv);
	float l = 0;

	if (v < TileVertEnsureDraw || face.r > 0)
		l = input.Light.b;
	else if (face.g > 0)
		l = input.Light.g;
	else if (face.b > 0)
		l = input.Light.r;
	if (CullDark)
		clip(l == 0 ? -1 : 1);
	// blend sampled color with source color
	float a = input.Color.a;

	// sample depth map from blockdepthmap at uv coords
	float4 d = tex2D(s3, uv);
		//float adjustedD = (d.r - 0.5f) * DepthResolution;// d.r *0.8f - 0.4f;// (d.r - 0.5f); // bring sampled depth within range of (-0.4f,0.4) to account for depth origin being at the center of the block //(-0.5f,0.5f)
		//float depthToWrite = input.Depth.x +adjustedD;//(d.r)*DepthResolution + 
		float depthToWrite = GetDepth(d.r, input.Depth.x);
	//output.Color = depthToWrite + float4(0, 0, 0, 1); // to draw opaque depth
	output.Color = d.a*((1 - a)*color + float4(a*color.rgb*input.Color.rgb, color.a));

	// blend sampled depth with input depth
	// add interpolated depth resolution (by factor from sampled depth) to input depth
	output.Depth = depthToWrite + float4(0, 0, 0, 1); // to draw opaque depth //d.r*DepthResolution + 
	output.DepthBuffer = depthToWrite; //d.r*DepthResolution + input.Depth.x; //
	output.Light = float4(l, l, l, 1);

	

	return output;
}

float4 EntityShadowShader(MyVertexOutput input, out float depthBuffer : DEPTH0) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	clip(color.a == 0 ? -1 : 1);

	// TODO: pack flags instead of color in vertex for lit face (FASTER!)
	// get local UV
	//float u = ((input.TexCoord.x * AtlasWidth) % 32) / 32;
	float v = ((input.TexCoord.y * AtlasHeight) % 16) / 16;
	//float2 uv = float2(u, v);

		float adjustedD = (1 - v - 0.5f);	// bring sampled depth within range of (-0.4f,0.4) to account for depth origin being at the center of the block //(-0.5f,0.5f)

		depthBuffer = adjustedD * DepthResolution + input.Depth.x - DepthResolution / 6; // to bring it a bit out from the ground plane
	return color;// float4(depthBuffer, 0, 0, 1);
}

float4 SolidColor(MyVertexOutput input) : COLOR0
{
	clip(tex2D(s, input.TexCoord.xy).a == 0 ? -1 : 1);
	return input.Color;
}
float4 SpriteDepthTextureShader(MyVertexOutput input) : COLOR0
{
	clip(tex2D(s, input.TexCoord.xy).a == 0 ? -1 : 1);
	return tex2D(s1, input.TexCoord.xy);
}
float4 DefaultShader(MyVertexOutput input) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	/*float d = input.Depth.x;
	return float4(d, d, d, 1);*/
	return color * input.Color;
}
float4 EntityMouseoverPixelShaderFunction(MyVertexOutput input, out float zBuffer : DEPTH0) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	float d = 1 - input.Depth.x;
	clip(color.a == 0 ? -1 : 1);
	zBuffer = d;// 1 - input.Depth.x;
	//return float4(d, d, d, 1);// 
	//return float4(d, 0, 0, 1);//
	
	float4 mix = mixWithMaterial(color, input.Material);
		return mix * input.Color;
	return color * input.Color; //float4(d, d, d, 1);// 
}
float4 DefaultShaderWithDepth(VertexShaderOutput input, out float zBuffer : DEPTH0) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	float d = 1 - input.Depth.x;
	clip(color.a == 0 ? -1 : 1);
	zBuffer = d;// 1 - input.Depth.x;
	//return float4(d, d, d, 1);// 
	//return float4(d, 0, 0, 1);//
	return color * input.Color; //float4(d, d, d, 1);// 
}
float4 BlockHighlightShader(MyVertexOutput input, out float depthBuffer : DEPTH0) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	clip(color.a == 0 ? -1 : 1);
	float4 d = tex2D(s1, input.TexCoord.xy);
		//	float adjustedD = d.r*0.8 - 0.4f;
		//float dd = adjustedD * DepthResolution + input.Depth.x - DepthResolution / 12; // to bring it a bit out from the ground plane
		float adjustedD = d.r*0.9 - 0.45f;
	float dd = adjustedD * DepthResolution + input.Depth.x - DepthResolution / 10;// GetDepth(d.r, input.Depth.x) - DepthResolution / 12;
	//dd = GetDepth(d.r, input.Depth.x);
	depthBuffer = dd;// input.Depth.x;// dd;

	//color *= input.Color;
	float a = input.Color.a;
	//color = ((1 - a)*color + float4(a*color.rgb*input.Color.rgb, color.a));

	//color *= float4(input.Color.rgb, 1);
	//color *= a;
	color = mixWithMaterial(color, input.Material);
	color *= input.Color;
	//color *= .5f;
	return color; // // // float4(input.Depth.x, input.Depth.x, input.Depth.x, 1);// 
}
float4 BlockHighlightShaderLastWorking(MyVertexOutput input, out float depthBuffer : DEPTH0) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	clip(color.a == 0 ? -1 : 1);
	float4 d = tex2D(s1, input.TexCoord.xy);
		//	float adjustedD = d.r*0.8 - 0.4f;
		//float dd = adjustedD * DepthResolution + input.Depth.x - DepthResolution / 12; // to bring it a bit out from the ground plane
		float adjustedD = d.r*0.9 - 0.45f;
	float dd = adjustedD * DepthResolution + input.Depth.x - DepthResolution / 10;// GetDepth(d.r, input.Depth.x) - DepthResolution / 12;
	depthBuffer = dd;// dd;
	return color * input.Color; //float4(dd, dd, dd, 1);// // 
}
float4 BlockHighlightShader2(MyVertexOutput input, out float depthBuffer : DEPTH0) : COLOR0
{
	float4 color = tex2D(s, input.TexCoord.xy);
	clip(color.a == 0 ? -1 : 1);
	//float u = ((input.TexCoord.x * AtlasWidth) % BlockWidth) / BlockWidth;
	//float v = ((input.TexCoord.y * AtlasHeight) % BlockHeight) / BlockHeight;
	//float2 uv = float2(u, v);
		float4 d = tex2D(s1, input.TexCoord.xy);
	//float adjustedD = d.r - 0.5f;// (1 - v - 0.5f);
	float adjustedD = d.r*0.8 - 0.4f;// (1 - v - 0.5f);
		float dd = adjustedD * DepthResolution + input.Depth.x -DepthResolution / 12; // to bring it a bit out from the ground plane
	depthBuffer = dd;
	//return d;// float4(d, d, d, 1);

	return color * input.Color;
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 SolidBorder();
    }
}

technique Technique2
{
	pass Pass1
	{
		// TODO: set renderstates here.

		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 ColorKey();
	}
}
technique Solid
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 SolidColor();
	}
}
technique Normal
{
	pass Pass1
	{
		// TODO: set renderstates here.

		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}

technique Entities
{
	pass Pass1
	{
		// TODO: set renderstates here.

		VertexShader = compile vs_2_0 MyVertexShader();
		//PixelShader = compile ps_2_0 EntityShaderWithDepth();
		PixelShader = compile ps_2_0 EntityShaderWithDepthColored();
	}
}

technique Light
{
	pass Pass1
	{
		// TODO: set renderstates here.

		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 LightFunction();
	}
}

technique Final
{
	pass Pass1
	{
		// TODO: set renderstates here.

		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 FinalOutsideBorders();
	}
}

technique CompositeWater
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 CompositeWaterShader();
	}
};

technique FinalInsideBorders
{
	pass Pass1
	{
		// TODO: set renderstates here.
		VertexShader = compile vs_2_0 VertexShaderFunction();
		//PixelShader = compile ps_2_0 FinalInsideBordersFunction();
		PixelShader = compile ps_2_0 FinalInsideBordersFunctionNoFog();

	}
	//pass Pass2
	//{
	//	// TODO: set renderstates here.

	//	VertexShader = compile vs_2_0 VertexShaderFunction();
	//	PixelShader = compile ps_2_0 Blur();
	//}
}
technique Chunks{
	pass Pass1
	{
		VertexShader = compile vs_2_0 MyChunkVertexShader();
		PixelShader = compile ps_2_0 MyChunkPixelShader();
	}
};
technique Combined{
	pass Pass1
	{
		VertexShader = compile vs_2_0 MyVertexShader();
		//PixelShader = compile ps_2_0 MyPixelShader();
		PixelShader = compile ps_2_0 MyPixelShaderWithNormals();
	}
};
technique CombinedWater{
	pass Pass1
	{
		//VertexShader = compile vs_2_0 MyVertexShader();
		//PixelShader = compile ps_2_0 MyPixelShader();
		VertexShader = compile vs_2_0 MyChunkVertexShader();

		PixelShader = compile ps_2_0 MyWaterShader();
	}
};

technique EntityShadows{
	pass Pass1
	{
		VertexShader = compile vs_2_0 MyVertexShader();
		PixelShader = compile ps_2_0 EntityShadowShader();
	}
};

technique BlockHighlight
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 MyVertexShader();
		PixelShader = compile ps_2_0 BlockHighlightShader();
	}
};

technique SpriteDepthTexture
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 SpriteDepthTextureShader();
	}
};

technique Default
{
	pass Pass1
	{
		ZEnable = true;
		ZWriteEnable = true;
		VertexShader = compile vs_2_0 VertexShaderFunction();
		//VertexShader = compile vs_2_0 MyVertexShader();
		//PixelShader = compile ps_2_0 DefaultShader();
		PixelShader = compile ps_2_0 DefaultShaderWithDepth();
	}
};

technique EntityMouseover
{
	pass Pass1
	{
		ZEnable = true;
		ZWriteEnable = true;
		VertexShader = compile vs_2_0 EntityMouseoverVertexShaderFunction();
		PixelShader = compile ps_2_0 EntityMouseoverPixelShaderFunction();
	}
};

technique RenderMapWithoutFog
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 MyVertexShader();
		PixelShader = compile ps_2_0 MapDump();
	}
};
technique ApplyFog
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 MyVertexShader();
		PixelShader = compile ps_2_0 ApplyFogShader();
	}
};
technique Fog
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 MyVertexShader();
		PixelShader = compile ps_2_0 FogShader();
	}
};
technique Water
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 MyVertexShader();
		PixelShader = compile ps_2_0 FinalWaterShader();
	}
};
technique FogWater
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 MyVertexShader();
		PixelShader = compile ps_2_0 FogWaterShader();
	}
};
technique Particles
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 MyVertexShader();
		PixelShader = compile ps_2_0 ParticleShader();
	}
};
technique EntitiesFog
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 MyVertexShader();
		PixelShader = compile ps_2_0 EntitiesFogShader();
	}
};

technique EntitiesUI
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 MyVertexShader();
		PixelShader = compile ps_2_0 EntitiesUIShader();
	}
};