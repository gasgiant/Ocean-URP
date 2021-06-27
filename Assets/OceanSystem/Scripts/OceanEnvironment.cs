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

        public float fogGradientScale = 25;
        public float fogDensity = 0.1f;
        public float tintGradientScale = 10;
    }
}
