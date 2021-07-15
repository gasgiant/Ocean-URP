using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[CustomEditor(typeof(ShoreMap))]
public class ShoreMapEditor : Editor
{
    const RenderTextureFormat mapRtFormat = RenderTextureFormat.ARGB32;
    const TextureFormat mapFormat = TextureFormat.ARGB32;

    ShoreMap shoreMap;
    Camera cam;
    RenderTexture elevationMap;

    RenderTexture shoreMapRT;
    //ComputeShader distanceFieldShader;

    LayerMask cullingMask;
    Vector3 position;
    float size;
    float backgroundElevation;
    Vector2 elevationRange;
    Vector2 distanceFieldRange;
    int resolution;
    string textureFileName;

    SerializedProperty textureProp;
    SerializedProperty positionProp;
    SerializedProperty sizeProp;
    SerializedProperty elevationRangeProp;
    SerializedProperty distanceFieldRangeProp;
    SerializedProperty cullingMaskProp;
    SerializedProperty backgroundElevationProp;
    SerializedProperty textureSavePathProp;
    SerializedProperty resolutionProp;
    SerializedProperty wavesModulationValueProp;
    SerializedProperty wavesModulationScaleProp;

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        RenderPipelineManager.beginFrameRendering += SetCameraTarget;


        shoreMap = (ShoreMap)target;

        InitializeCaptureParams();
        FindProps();

        var go =
                new GameObject("Elevation Camera") { hideFlags = HideFlags.HideAndDontSave };
        go.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;

        cam = go.AddComponent<Camera>();
        var additionalCamData = cam.GetUniversalAdditionalCameraData();

        cam.enabled = false;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.orthographic = true;
        cam.nearClipPlane = 10f;
        cam.farClipPlane = 1000;
        cam.allowMSAA = false;
        cam.allowHDR = true;
        cam.transform.rotation = Quaternion.Euler(90, 0, 0);

        InitRenderTextures(resolution);
        additionalCamData.renderShadows = false;
        additionalCamData.requiresColorOption = CameraOverrideOption.Off;
        additionalCamData.requiresDepthOption = CameraOverrideOption.Off;
        additionalCamData.SetRenderer(1);

        //distanceFieldShader = (ComputeShader)Resources.Load("ComputeShaders/RaymarchDistanceField");
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginFrameRendering -= SetCameraTarget;
        SceneView.duringSceneGui -= OnSceneGUI;
        if (cam != null)
            DestroyImmediate(cam.gameObject);
        if (elevationMap != null)
            elevationMap.Release();
        if (shoreMapRT != null)
            shoreMapRT.Release();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawBaked();
        DrawCapture();

        InitRenderTextures(resolution);
        cam.transform.position = position;
        cam.orthographicSize = size;
        
        serializedObject.ApplyModifiedProperties();

        DrawPreviewTextures();
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        cam.transform.position = position;
        cam.orthographicSize = size;
        Handles.color = Color.white;
        Vector3 cubeSize;
        cubeSize.y = cam.farClipPlane - cam.nearClipPlane;
        cubeSize.x = cam.orthographicSize * 2;
        cubeSize.z = cam.orthographicSize * 2;
        Handles.DrawWireCube(cam.transform.position + Vector3.down * cubeSize.y * 0.5f, cubeSize);
    }

    private void InitializeCaptureParams()
    {
        cullingMask = shoreMap.cullingMask;
        position = shoreMap.Position;
        size = shoreMap.Position.w;
        backgroundElevation = shoreMap.backgroundElevation;
        elevationRange = shoreMap.ElevationRange;
        distanceFieldRange = shoreMap.DistanceFieldRange;
        resolution = shoreMap.resolution;
        textureFileName = GetPath().Item2 + "_Texture";
    }

    private void FindProps()
    {
        textureProp = serializedObject.FindProperty("texture");
        positionProp = serializedObject.FindProperty("position");
        sizeProp = serializedObject.FindProperty("size");
        elevationRangeProp = serializedObject.FindProperty("elevationRange");
        distanceFieldRangeProp = serializedObject.FindProperty("distanceFieldRange");
        cullingMaskProp = serializedObject.FindProperty("cullingMask");
        backgroundElevationProp = serializedObject.FindProperty("backgroundElevation");
        textureSavePathProp = serializedObject.FindProperty("textureSavePath");
        resolutionProp = serializedObject.FindProperty("resolution");
        wavesModulationValueProp = serializedObject.FindProperty("wavesModulationValue");
        wavesModulationScaleProp = serializedObject.FindProperty("wavesModulationScale");
    }

    private void DrawBaked()
    {
        EditorGUILayout.LabelField("Baked", EditorStyles.boldLabel);
        EditorGUI.indentLevel += 1;
        if (shoreMap.Texture == null)
        {
            EditorGUILayout.LabelField("No baked map", EditorStyles.helpBox);
        }
        else
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(textureProp);
            EditorGUILayout.PropertyField(positionProp);
            EditorGUILayout.PropertyField(sizeProp);
            EditorGUILayout.PropertyField(elevationRangeProp);
            EditorGUILayout.PropertyField(distanceFieldRangeProp);
            GUI.enabled = true;
        }
        EditorGUILayout.PropertyField(wavesModulationValueProp);
        EditorGUILayout.PropertyField(wavesModulationScaleProp);
        EditorGUI.indentLevel -= 1;
        EditorGUILayout.Space();
    }

    private void DrawCapture()
    {
        shoreMap.captureFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(shoreMap.captureFoldout, "Capture");
        if (shoreMap.captureFoldout)
        {
            textureFileName = EditorGUILayout.TextField("Texture File Name", textureFileName);
            resolution = EditorGUILayout.IntField("Resolution", resolution);
            elevationRange = EditorGUILayout.Vector2Field("Elevation Range", elevationRange);
            distanceFieldRange = EditorGUILayout.Vector2Field("Distance Field Range", distanceFieldRange);

            EditorGUILayout.Space();
            LayerMask tempMask = EditorGUILayout.MaskField("Culling Mask", InternalEditorUtility.LayerMaskToConcatenatedLayersMask(cullingMask), InternalEditorUtility.layers);
            cullingMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);
            position = EditorGUILayout.Vector3Field("Position", position);
            size = EditorGUILayout.FloatField("Size", size);
            backgroundElevation = EditorGUILayout.FloatField("Background Elevation", backgroundElevation);
            EditorGUILayout.Space();
            if (GUILayout.Button("Bake map", GUILayout.Height(40)))
            {
                BakeMap();
            }
            EditorGUILayout.Space();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void BakeMap()
    {
        //RenderElevation();
        
        //distanceFieldShader.SetInt("Resolution", resolution);
        //distanceFieldShader.SetFloat("MapSize", size * 2);
        //distanceFieldShader.SetVector("DistanceFieldRange", distanceFieldRange);
        //distanceFieldShader.SetVector("ElevationRange",elevationRange);
        //distanceFieldShader.SetTexture(0, "Elevation", elevationMap);
        //distanceFieldShader.SetTexture(0, "ShoreMap", shoreMapRT);
        //distanceFieldShader.Dispatch(0, resolution / LOCAL_WORK_GROUPS_X, resolution / LOCAL_WORK_GROUPS_Y, 1);
        
        RenderTexture.active = shoreMapRT;
        Texture2D shoreMapTexture = new Texture2D(resolution, resolution, mapFormat, false, true);
        shoreMapTexture.wrapMode = TextureWrapMode.Clamp;
        shoreMapTexture.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
        shoreMapTexture.Apply(false);
        string path = GetPath().Item1 + textureFileName + ".asset";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(shoreMapTexture, path);
        Object textureAsset = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
        
        textureProp.objectReferenceValue = textureAsset;
        positionProp.vector3Value = position;
        sizeProp.floatValue = size;
        elevationRangeProp.vector2Value = elevationRange;
        distanceFieldRangeProp.vector2Value = distanceFieldRange;
        cullingMaskProp.intValue = cullingMask;
        backgroundElevationProp.floatValue = backgroundElevation;
        resolutionProp.intValue = resolution;
    }

    (string , string) GetPath()
    {
        string[] ss = AssetDatabase.GetAssetPath(shoreMap).Split('/');
        string path = "";
        for (int i = 0; i < ss.Length - 1; i++)
        {
            path += ss[i] + "/";
        }
        return (path, ss[ss.Length - 1].Split('.')[0]);
    }

    private void DrawPreviewTextures()
    {
        shoreMap.previewFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(shoreMap.previewFoldout, "Preview");
        if (shoreMap.previewFoldout)
        {
            if (cam.targetTexture != null)
                RenderElevation();
            Rect previewRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(200));

            Rect camPreviewRect = previewRect;
            camPreviewRect.width *= 0.5f;
            EditorGUI.DrawPreviewTexture(camPreviewRect, elevationMap);
            if (shoreMap.Texture != null)
            {
                Rect texPreviewRect = previewRect;
                texPreviewRect.x += texPreviewRect.width * 0.5f;
                texPreviewRect.width *= 0.5f;
                EditorGUI.DrawPreviewTexture(texPreviewRect, shoreMap.Texture);
            }
        }
    }

    private void SetCameraTarget(ScriptableRenderContext context, Camera[] camera)
    {
        cam.targetTexture = elevationMap;
    }

    private void RenderElevation()
    {
        cam.cullingMask = cullingMask;
        cam.backgroundColor = new Vector4(backgroundElevation, 0, 0, 0);
        cam.targetTexture = elevationMap;
        cam.Render();
    }

    private void InitRenderTextures(int size)
    {
        size = Mathf.Clamp(size, 1, 1024);
        if (elevationMap != null && shoreMapRT != null
            && elevationMap.height == size && shoreMapRT.height == size)
            return;

        // elevation map
        if (elevationMap != null) elevationMap.Release();
        elevationMap = new RenderTexture(size, size, 16,
            RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
        elevationMap.useMipMap = false;
        elevationMap.wrapMode = TextureWrapMode.Clamp;
        elevationMap.filterMode = FilterMode.Bilinear;
        elevationMap.anisoLevel = 0;
        elevationMap.Create();

        // shore map
        if (shoreMapRT != null) shoreMapRT.Release();
        shoreMapRT = new RenderTexture(size, size, 0,
            mapRtFormat, RenderTextureReadWrite.Linear);
        shoreMapRT.wrapMode = TextureWrapMode.Clamp;
        shoreMapRT.enableRandomWrite = true;
        shoreMapRT.Create();
    }

    const int LOCAL_WORK_GROUPS_X = 8;
    const int LOCAL_WORK_GROUPS_Y = 8;
}
