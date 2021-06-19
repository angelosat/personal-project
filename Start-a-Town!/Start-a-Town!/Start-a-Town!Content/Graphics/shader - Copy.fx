

struct VS_SHADOW_OUTPUT
{
    float4 Position : POSITION;
    float Depth : TEXCOORD0;
};

VS_SHADOW_OUTPUT RenderShadowMapVS(float4 vPos: POSITION)
{
    VS_SHADOW_OUTPUT Out;
    Out.Position = GetPositionFromLight(vPos); 
    // Depth is Z/W.  This is returned by the pixel shader.
    // Subtracting from 1 gives us more precision in floating point.
    Out.Depth.x = 1-(Out.Position.z/Out.Position.w);    
    return Out;
}

float4 RenderShadowMapPS( VS_SHADOW_OUTPUT In ) : COLOR
{ 
    // The depth is Z divided by W. We return
    // this value entirely in a 32-bit red channel
    // using SurfaceFormat.Single.  This preserves the
    // floating-point data for finer detail.
    return float4(In.Depth.x,0,0,1);
}

technique ShadowMapRender
{
    pass P0
    {
        CullMode = NONE;
        ZEnable = TRUE;
        ZWriteEnable = TRUE;
        AlphaBlendEnable = FALSE;
        
        VertexShader = compile vs_2_0 RenderShadowMapVS();
        PixelShader  = compile ps_2_0 RenderShadowMapPS();
    }
}