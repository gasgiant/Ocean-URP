using UnityEngine;

namespace OceanSystem
{
    [System.Serializable]
    public struct FoamParams
    {
        [Range(-0.1f, 1)]
        public float coverage;
        [Range(0, 1)]
        public float underwater;
        public float density;
        [Range(0, 1)]
        public float persistence;
        public float decayRate;
        public Vector4 cascadesWeights;

        public static FoamParams GetDefault()
        {
            return new FoamParams()
            {
                density = 8.4f,
                persistence = 0.5f,
                decayRate = 0.1f,
                cascadesWeights = Vector4.one
            };
        }

        public static FoamParams Lerp(FoamParams lhs, FoamParams rhs, float t)
        {
            FoamParams res = new FoamParams();
            res.coverage = Mathf.Lerp(lhs.coverage, rhs.coverage, t);
            res.underwater = Mathf.Lerp(lhs.underwater, rhs.underwater, t);
            res.density = Mathf.Lerp(lhs.density, rhs.density, t);
            res.persistence = Mathf.Lerp(lhs.persistence, rhs.persistence, t);
            res.decayRate = Mathf.Lerp(lhs.decayRate, rhs.decayRate, t);
            res.cascadesWeights = Vector4.Lerp(lhs.cascadesWeights, rhs.cascadesWeights, t);
            return res;
        }
    }
}
