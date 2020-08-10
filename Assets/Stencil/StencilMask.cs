using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace xx
{
	public class StencilMask : MonoBehaviour
	{
        void Start()
        {
            Renderer[] array = GetComponentsInChildren<Renderer>();
            foreach (var ps in array)
            {
                ps.sharedMaterial.SetFloat(ShaderConfig._StencilComp, (int)UnityEngine.Rendering.CompareFunction.Equal);
                ps.sharedMaterial.SetFloat(ShaderConfig._Stencil, 1);
                ps.sharedMaterial.SetFloat(ShaderConfig._StencilOp, (int)UnityEngine.Rendering.StencilOp.Keep);
                ps.sharedMaterial.SetFloat(ShaderConfig._StencilReadMask, 1);
            }

            Image image; image = GetComponent<Image>();
            image.material.SetInt(ShaderConfig._StencilComp, (int)UnityEngine.Rendering.CompareFunction.Always);
            image.material.SetInt(ShaderConfig._Stencil, 1);
            image.material.SetInt(ShaderConfig._StencilOp, (int)UnityEngine.Rendering.StencilOp.Replace);
            image.material.SetInt(ShaderConfig._StencilReadMask, 255);
        }
    }
}

