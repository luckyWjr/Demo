using UnityEngine;

public class ShaderConfig
{
    public static int _StencilComp = Shader.PropertyToID("_StencilComp");
    public static int _Stencil = Shader.PropertyToID("_Stencil");
    public static int _StencilOp = Shader.PropertyToID("_StencilOp");
    public static int _StencilReadMask = Shader.PropertyToID("_StencilReadMask");

    public delegate Shader GetFunction(string name);

    public static GetFunction Get = Shader.Find;

    public static string uiEffectShader = "Custom/FGUI_FX/Particles/Additive";

    public static Shader GetShader(string name)
    {
        Shader shader = Get(name);
        if (shader == null)
        {
            Debug.LogWarning("FairyGUI: shader not found: " + name);
            //shader = Shader.Find("UI/Default");
        }
        shader.hideFlags = HideFlags.DontSaveInEditor;

        return shader;
    }
}