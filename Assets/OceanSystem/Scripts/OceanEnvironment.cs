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
    }
}
