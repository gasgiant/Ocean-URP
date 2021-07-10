#if !defined(OCEAN_MATERIAL_PROPS_INCLUDED)
#define OCEAN_MATERIAL_PROPS_INCLUDED

TEXTURE2D(_FoamAlbedo);
SAMPLER(sampler_FoamAlbedo);
float4 _FoamAlbedo_ST;
TEXTURE2D(_FoamUnderwaterTexture);
SAMPLER(sampler_FoamUnderwaterTexture);
float4 _FoamUnderwaterTexture_ST;
TEXTURE2D(_ContactFoamTexture);
SAMPLER(sampler_ContactFoamTexture);
float4 _ContactFoamTexture_ST;
TEXTURE2D(_FoamTrailTexture);
SAMPLER(sampler_FoamTrailTexture);

TEXTURE2D(_DistantRoughnessMap);
SAMPLER(sampler_DistantRoughnessMap);
float4 _DistantRoughnessMap_ST;
TEXTURE2D(_FoamDetailMap);
SAMPLER(sampler_FoamDetailMap);
float4 _FoamDetailMap_ST;

CBUFFER_START(UnityPerMaterial)
// specular
float _SpecularStrength;
float _SpecularMinRoughness;

// horizon
float _RoughnessScale;
float _RoughnessDistance;
float _HorizonFog;
float _CascadesFadeDist;
float _UvWarpStrength;

// local reflections
float _ReflectionNormalStength;

// underwater 
float _RefractionStrength;
float _RefractionStrengthUnderwater;

// subsurface scattering
float _SssSunStrength;
float _SssEnvironmentStrength;
float _SssSpread;
float _SssNormalStrength;
float _SssHeightBias;
float _SssFadeDistance;

// foam
float _FoamNormalsDetail;
float4 _FoamTint;
float4 _UnderwaterFoamColor;
float _UnderwaterFoamParallax;
float _ContactFoam;
CBUFFER_END

#endif