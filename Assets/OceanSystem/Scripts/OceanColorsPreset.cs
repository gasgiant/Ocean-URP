using UnityEngine;

[CreateAssetMenu(fileName = "New Ocean Colors", menuName = "Ocean/Colors Preset")]
public class OceanColorsPreset : ScriptableObject
{
    [SerializeField, ColorUsage(false, true)] private Color _deepScatter;
    [SerializeField, ColorUsage(false, true)] private Color _shallowScatter;
    [SerializeField, ColorUsage(false, true)] private Color _diffuse;
    [SerializeField, ColorUsage(true, true)] private Color _reflectionMask;
    [SerializeField] private Gradient _absorbtion;

    public Color DeepScatter => _deepScatter;
    public Color ShallowScatter => _shallowScatter;
    public Color Diffuse => _diffuse;
    public Color ReflectionMask => _reflectionMask;
    public Gradient Absorbtion => _absorbtion;
}
