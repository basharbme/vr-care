using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Gamma Correction")]
public class GammaCorrectionEffect : ImageEffectBase
{
    public float gamma;

    // Called by camera to apply image effect
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        material.SetFloat("_Gamma", 1f/gamma);
        ImageEffects.BlitWithMaterial(material, source, destination);
    }
}