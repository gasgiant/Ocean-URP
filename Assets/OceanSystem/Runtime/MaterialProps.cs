using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OceanSystem
{
    public static class MaterialProps
    {
        // volume
        public static readonly int FogDensity = Shader.PropertyToID("_FogDensity");
        public static readonly int AbsorptionDepthScale = Shader.PropertyToID("_AbsorptionDepthScale");

        // Reflection Mask
        public static readonly int ReflectionMaskRadius = Shader.PropertyToID("_ReflectionMaskRadius");
        public static readonly int ReflectionMaskSharpness = Shader.PropertyToID("_ReflectionMaskSharpness");
    }
}
