using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OceanSystem
{
    public class FoamVariablesController
    {
        private Vector2 _foamTrailTextureSize0;
        private Vector2 _foamTrailTextureSize1;
        private Vector2 _foamTrailDirection0;
        private Vector2 _foamTrailDirection1;
        private float _foamTrailBlendTimeStart = -1;

        public void SetGlobalFoamVariables(OceanSimulationInputs inputs, float windAngle)
        {
            Shader.SetGlobalFloat(ShaderVariables.Coverage, inputs.foam.coverage);
            Shader.SetGlobalFloat(ShaderVariables.Density, inputs.foam.density);
            Shader.SetGlobalFloat(ShaderVariables.Sharpness, inputs.foam.sharpness);
            Shader.SetGlobalFloat(ShaderVariables.Persistence, inputs.foam.persistence);
            Shader.SetGlobalFloat(ShaderVariables.Trail, inputs.foam.trail);
            Shader.SetGlobalFloat(ShaderVariables.TrailTextureStrength, inputs.foam.trailTextureStrength);
            Shader.SetGlobalFloat(ShaderVariables.Underwater, inputs.foam.underwater);
            Shader.SetGlobalVector(ShaderVariables.CascadesWeights, inputs.foam.cascadesWeights);

            float windAngleRadians = windAngle * Mathf.Deg2Rad;
            Vector2 windDirection = new Vector2(Mathf.Cos(windAngleRadians), Mathf.Sin(windAngleRadians));
            if (inputs.foamTrailUpdateTime <= 0)
            {
                _foamTrailTextureSize0 = inputs.foam.trailTextureSize;
                _foamTrailDirection0 = windDirection;
                Shader.SetGlobalVector(ShaderVariables.TrailTextureSize0, _foamTrailTextureSize0);
                Shader.SetGlobalVector(ShaderVariables.TrailDirection0, _foamTrailDirection0);
                Shader.SetGlobalFloat(ShaderVariables.TrailBlendValue, 0);
            }
            else
            {
                if (_foamTrailBlendTimeStart + inputs.foamTrailUpdateTime < Time.time)
                {
                    _foamTrailBlendTimeStart = Time.time;
                    _foamTrailTextureSize0 = _foamTrailTextureSize1;
                    _foamTrailDirection0 = _foamTrailDirection1;
                    _foamTrailTextureSize1 = inputs.foam.trailTextureSize;
                    _foamTrailDirection1 = windDirection;
                    Shader.SetGlobalVector(ShaderVariables.TrailTextureSize0, _foamTrailTextureSize0);
                    Shader.SetGlobalVector(ShaderVariables.TrailDirection0, _foamTrailDirection0);
                    Shader.SetGlobalVector(ShaderVariables.TrailTextureSize1, _foamTrailTextureSize1);
                    Shader.SetGlobalVector(ShaderVariables.TrailDirection1, _foamTrailDirection1);
                    Shader.SetGlobalFloat(ShaderVariables.TrailBlendValue, 0);
                }
                else
                {
                    Shader.SetGlobalFloat(ShaderVariables.TrailBlendValue, 
                        Mathf.Clamp01((Time.time - _foamTrailBlendTimeStart) / inputs.foamTrailUpdateTime));
                }
            }

        }

        public static class ShaderVariables
        {
            public static readonly int Coverage = Shader.PropertyToID("Ocean_FoamCoverage");
            public static readonly int Density = Shader.PropertyToID("Ocean_FoamDensity");
            public static readonly int Sharpness = Shader.PropertyToID("Ocean_FoamSharpness");
            public static readonly int Persistence = Shader.PropertyToID("Ocean_FoamPersistence");
            public static readonly int Trail = Shader.PropertyToID("Ocean_FoamTrail");
            public static readonly int TrailTextureStrength = Shader.PropertyToID("Ocean_FoamTrailTextureStrength");
            public static readonly int Underwater = Shader.PropertyToID("Ocean_FoamUnderwater");
            public static readonly int CascadesWeights = Shader.PropertyToID("Ocean_FoamCascadesWeights");

            public static readonly int TrailTextureSize0 = Shader.PropertyToID("Ocean_FoamTrailTextureSize0");
            public static readonly int TrailTextureSize1 = Shader.PropertyToID("Ocean_FoamTrailTextureSize1");
            public static readonly int TrailDirection0 = Shader.PropertyToID("Ocean_FoamTrailDirection0");
            public static readonly int TrailDirection1 = Shader.PropertyToID("Ocean_FoamTrailDirection1");
            public static readonly int TrailBlendValue = Shader.PropertyToID("Ocean_FoamTrailBlendValue");
        }
    }
}
