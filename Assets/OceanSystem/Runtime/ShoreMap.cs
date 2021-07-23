using UnityEngine;

[CreateAssetMenu(fileName = "ShoreMap", menuName = "Ocean/ShoreMap")]
public class ShoreMap : ScriptableObject
{
    public Texture2D Texture => texture;
    public Vector4 Position
    {
        get
        {
            Vector4 v = position;
            v.w = size;
            return v;
        }
    }

    public Vector4 Ranges => new Vector4(elevationRange.x, elevationRange.y,
        distanceFieldRange.x, distanceFieldRange.y);
    public Vector2 ElevationRange => elevationRange;
    public Vector2 DistanceFieldRange => distanceFieldRange;
    public Vector4 WavesModulationValue => wavesModulationValue;
    public Vector4 WavesModulationScale => wavesModulationScale;

    [SerializeField] private Texture2D texture;
    [SerializeField] private Vector3 position = Vector3.up * 500;
    [SerializeField] private float size = 1000;
    [SerializeField] private Vector2 elevationRange = new Vector2(-50, 3);
    [SerializeField] private Vector2 distanceFieldRange = new Vector2(-15, 50);
    [SerializeField] private Vector4 wavesModulationValue;
    [SerializeField] private Vector4 wavesModulationScale;

#if UNITY_EDITOR
    public LayerMask cullingMask;
    public float backgroundElevation = -100;
    public int resolution = 256;

    public bool captureFoldout;
    public bool previewFoldout;

    private void OnValidate()
    {
        for (int i = 0; i < 4; i++)
        {
            wavesModulationScale[i] = Mathf.Max(0, wavesModulationScale[i]);
            wavesModulationValue[i] = Mathf.Clamp01(wavesModulationValue[i]);
        }
        resolution = Mathf.Clamp(resolution, 1, 1024);
    }
#endif

}
