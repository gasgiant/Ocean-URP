using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OceanSystem
{
    public class FoamSimulationTest : MonoBehaviour
    {
        [SerializeField] private ComputeShader _computeShader;
        [SerializeField] private Renderer _debug;

        private RenderTexture _target;
        private const int TargetResolution = 512;
        private int frame;

        void Start()
        {
            _target = new RenderTexture(TargetResolution, TargetResolution, 0,
                RenderTextureFormat.R16, RenderTextureReadWrite.Linear);
            _target.enableRandomWrite = true;
            _target.Create();

            _debug.sharedMaterial.SetTexture("_MainTex", _target);
        }

        void Update()
        {
            if (frame < 5)
            {
                frame += 1;
                return;
            }

            _computeShader.SetVector("LengthScales",
                Shader.GetGlobalVector(GlobalShaderVariables.Simulation.LengthScales));
            _computeShader.SetInt("CascadesCount", 4);
            _computeShader.SetInt("MapResolution", TargetResolution);
            _computeShader.SetFloat("MapScale", _debug.transform.localScale.x);
            _computeShader.SetFloat("DeltaTime", Time.deltaTime);
            _computeShader.SetTexture(0, "Input", 
                Shader.GetGlobalTexture(GlobalShaderVariables.Simulation.DisplacementAndDerivatives));
            _computeShader.SetTexture(0, "Map", _target);

            _computeShader.Dispatch(0, TargetResolution / 8, TargetResolution / 8, 1);

        }
    }
}
