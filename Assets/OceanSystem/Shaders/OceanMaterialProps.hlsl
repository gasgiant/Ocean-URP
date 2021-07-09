#if !defined(OCEAN_MATERIAL_PROPS_INCLUDED)
#define OCEAN_MATERIAL_PROPS_INCLUDED

TEXTURE2D(_SurfaceFoamAlbedo);
SAMPLER(sampler_SurfaceFoamAlbedo);
float4 _SurfaceFoamAlbedo_ST;
TEXTURE2D(_FoamUnderwaterTexture);
SAMPLER(sampler_FoamUnderwaterTexture);
float4 _FoamUnderwaterTexture_ST;
TEXTURE2D(_ContactFoamTexture);
SAMPLER(sampler_ContactFoamTexture);
float4 _ContactFoamTexture_ST;
TEXTURE2D(_FoamTrailTexture);
SAMPLER(sampler_FoamTrailTexture);



CBUFFER_START(UnityPerMaterial)
// specular
float _SpecularStrength;
float _SpecularMinRoughness;

// horizon
float _RoughnessScale;
float _RoughnessDistance;
float _HorizonFog;
float _CascadesFadeDist;

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
float _SssHeight;
float _SssHeightMult;
float _SssFadeDistance;

// foam
float _FoamNormalsDetail;
float4 _FoamTint;
float4 _UnderwaterFoamColor;
float _UnderwaterFoamParallax;
float _ContactFoam;
CBUFFER_END

#endif