using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[PostProcess(typeof(SobelOutlineRenderer), PostProcessEvent.BeforeStack, "SobelOutline")]
public class SobelOutline : PostProcessEffectSettings
{
    public FloatParameter thickness = new FloatParameter { value = 1.0f };
    public FloatParameter depthMultiplier = new FloatParameter { value = 1.0f };
    public FloatParameter depthBias = new FloatParameter { value = 1.0f };
    public FloatParameter normalMultiplier = new FloatParameter { value = 1.0f };
    public FloatParameter normalBias = new FloatParameter { value = 10.0f };
    public ColorParameter color = new ColorParameter { value = Color.black };
}

public sealed class SobelOutlineRenderer : PostProcessEffectRenderer<SobelOutline>
{
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("PostProcessing/SobelOutline"));

        GetNormalsTextureEvent texture = new GetNormalsTextureEvent();
        Event<GetNormalsTextureEvent>.Broadcast(texture);
        if(texture.normals != null)
            sheet.properties.SetTexture("_NormalTex", texture.normals);

        if (texture.depth != null)
            sheet.properties.SetTexture("_DepthTex", texture.depth);

        sheet.properties.SetFloat("_OutlineThickness", settings.thickness);
        sheet.properties.SetFloat("_OutlineDepthMultiplier", settings.depthMultiplier);
        sheet.properties.SetFloat("_OutlineDepthBias", settings.depthBias);
        sheet.properties.SetFloat("_OutlineNormalMultiplier", settings.normalMultiplier);
        sheet.properties.SetFloat("_OutlineNormalBias", settings.normalBias);
        sheet.properties.SetColor("_OutlineColor", settings.color);

        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}