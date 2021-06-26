using UnityEngine;

namespace OceanSystem
{
    [CreateAssetMenu(fileName = "New Environment", menuName = "Ocean/Environment")]
    public class OceanEnvironment : ScriptableObject
    {
        //public Cubemap environmentCube;
        public Color bottomHemisphereColor;
        public float bottomHemisphereRadius;
        public float bottomHemisphereStrength;

        public Gradient waterFog;
        public Gradient subsurfaceScattering;
        public float fogGradientScale = 25;
        public float fogDensity = 0.1f;
        public Gradient tint;
        public float tintGradientScale = 10;
        public Color murkColor;

        private Texture2D fogTex;
        private Texture2D sssTex;
        private Texture2D tintTex;

        public Texture2D GetFogTex()
        {
            if (fogTex == null)
                GradientTexture.BakeGradient(waterFog, ref fogTex);
            return fogTex;
        }

        public Texture2D GetSssTex()
        {
            if (sssTex == null)
                GradientTexture.BakeGradient(subsurfaceScattering, ref sssTex);
            return sssTex;
        }

        public Texture2D GetTintTex()
        {
            if (tintTex == null)
                GradientTexture.BakeGradient(tint, ref tintTex);
            return tintTex;
        }

        private void OnValidate()
        {
            GradientTexture.BakeGradient(waterFog, ref fogTex);
            GradientTexture.BakeGradient(subsurfaceScattering, ref sssTex);
            GradientTexture.BakeGradient(tint, ref tintTex);
        }
    }
}