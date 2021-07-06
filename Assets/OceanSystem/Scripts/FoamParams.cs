using UnityEngine;

namespace OceanSystem
{
    [System.Serializable]
    public struct FoamParams
    {
        [Range(-0.1f, 1)] public float coverage;
        public float density;
        [Range(0, 1)] public float sharpness;
        [Range(0, 1)] public float persistence;
        [Range(0, 1)] public float trail;
        [Range(0, 1)] public float trailTextureStrength;
        public Vector2 trailTextureSize;
        [Range(0, 1)]  public float underwater;
        public float decayRate;

        public Vector4 cascadesWeights;

        public static FoamParams GetDefault()
        {
            return new FoamParams()
            {
                density = 8.4f,
                sharpness = 0.5f,
                persistence = 0.5f,
                trail = 0,
                trailTextureStrength = 0.5f,
                trailTextureSize = new Vector2(100, 50),
                decayRate = 0.02f,
                cascadesWeights = Vector4.one
            };
        }

        public static FoamParams Lerp(FoamParams lhs, FoamParams rhs, float t)
        {
            FoamParams res = new FoamParams();
            res.coverage = Mathf.Lerp(lhs.coverage, rhs.coverage, t);
            res.density = Mathf.Lerp(lhs.density, rhs.density, t);
            res.sharpness = Mathf.Lerp(lhs.sharpness, rhs.sharpness, t);
            res.persistence = Mathf.Lerp(lhs.persistence, rhs.persistence, t);
            res.trail = Mathf.Lerp(lhs.trail, rhs.trail, t);
            res.trailTextureStrength = Mathf.Lerp(lhs.trailTextureStrength, rhs.trailTextureStrength, t);
            res.trailTextureSize = Vector2.Lerp(lhs.trailTextureSize, rhs.trailTextureSize, t);
            res.underwater = Mathf.Lerp(lhs.underwater, rhs.underwater, t);
            res.cascadesWeights = Vector4.Lerp(lhs.cascadesWeights, rhs.cascadesWeights, t);
            res.decayRate = Mathf.Lerp(lhs.decayRate, rhs.decayRate, t);
            return res;
        }

        public static class ShaderVariables
        {
            public static readonly int Coverage = Shader.PropertyToID("Ocean_FoamCoverage");
            public static readonly int Density = Shader.PropertyToID("Ocean_FoamDensity");
            public static readonly int Sharpness = Shader.PropertyToID("Ocean_FoamSharpness");
            public static readonly int Persistence = Shader.PropertyToID("Ocean_FoamPersistence");
            public static readonly int Trail = Shader.PropertyToID("Ocean_FoamTrail");
            public static readonly int TrailTextureStrength = Shader.PropertyToID("Ocean_FoamTrailTextureStrength");
            public static readonly int TrailTextureSize = Shader.PropertyToID("Ocean_FoamTrailTextureSize");
            public static readonly int Underwater = Shader.PropertyToID("Ocean_FoamUnderwater");
            public static readonly int CascadesWeights = Shader.PropertyToID("Ocean_FoamCascadesWeights");
        }
    }
}
