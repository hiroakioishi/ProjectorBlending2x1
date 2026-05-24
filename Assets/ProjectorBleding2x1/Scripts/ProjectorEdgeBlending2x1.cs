using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(
    typeof(CustomInvertRenderer),
    PostProcessEvent.AfterStack,
    "Custom/ProjectorEdgeBlending2x1"
)]
public sealed class ProjectorEdgeBlending2x1 : PostProcessEffectSettings
{
    [Range(1, 7680)]
    public IntParameter outputWidthPixels = new IntParameter { value = 3840 };

    [Range(1, 3840)]
    public IntParameter projectorWidthPixels = new IntParameter { value = 1920 };

    [Range(1, 3840)]
    public IntParameter blendWidthPixels = new IntParameter { value = 288 };

    [Range(0.1f, 5.0f)]
    public FloatParameter blendGamma = new FloatParameter { value = 1.0f };

    [Range(0, 1)]
    public IntParameter debugBlend = new IntParameter { value = 0 };

    [Range(0, 1)]
    public IntParameter debugUV = new IntParameter { value = 0 };
}

public sealed class CustomInvertRenderer : PostProcessEffectRenderer<ProjectorEdgeBlending2x1>
{
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(
            Shader.Find("Hidden/Custom/ProjectorEdgeBlending2x1")
        );

        //sheet.properties.SetFloat("_Intensity", settings.intensity);

        sheet.properties.SetFloat("_OutputWidthPixels", settings.outputWidthPixels.value);
        sheet.properties.SetFloat("_ProjectorWidthPixels", settings.projectorWidthPixels.value);
        sheet.properties.SetFloat("_BlendWidthPixels", settings.blendWidthPixels.value);
        sheet.properties.SetFloat("_BlendGamma", settings.blendGamma.value);
        sheet.properties.SetFloat("_DebugBlend", settings.debugBlend.value);
        sheet.properties.SetFloat("_DebugUV", settings.debugUV.value);

        context.command.BlitFullscreenTriangle(
            context.source,
            context.destination,
            sheet,
            0
        );
    }
}