using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TextureBasedPrefabPlacer))]
public class TextureBasedPrefabPlacerEditor : Editor
{
    private TextureBasedPrefabPlacer placer;
    private bool showMeshSettings = true;
    private bool showPrefabSettings = true;
    private bool showColorSettings = true;
    private bool showPlacementSettings = true;
    private bool showRandomizationSettings = true;
    private bool showDebugInfo = false;
    
    private void OnEnable()
    {
        placer = (TextureBasedPrefabPlacer)target;
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        DrawHeader();
        
        EditorGUILayout.Space();
        
        DrawMeshSettings();
        DrawPrefabSettings();
        DrawColorSettings();
        DrawPlacementSettings();
        DrawRandomizationSettings();
        DrawDebugInfo();
        
        EditorGUILayout.Space();
        
        DrawActionButtons();
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void DrawHeader()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Texture-Based Prefab Placer", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
    }
    
    private void DrawMeshSettings()
    {
        showMeshSettings = EditorGUILayout.Foldout(showMeshSettings, "Mesh Settings", true);
        if (showMeshSettings)
        {
            EditorGUI.indentLevel++;
            
            SerializedProperty meshRendererProp = serializedObject.FindProperty("targetMeshRenderer");
            SerializedProperty meshFilterProp = serializedObject.FindProperty("targetMeshFilter");
            
            EditorGUILayout.PropertyField(meshRendererProp);
            EditorGUILayout.PropertyField(meshFilterProp);
            
            // Auto-assign if one is set but not the other
            if (meshRendererProp.objectReferenceValue != null && meshFilterProp.objectReferenceValue == null)
            {
                MeshRenderer renderer = meshRendererProp.objectReferenceValue as MeshRenderer;
                if (renderer != null)
                {
                    MeshFilter filter = renderer.GetComponent<MeshFilter>();
                    if (filter != null)
                    {
                        meshFilterProp.objectReferenceValue = filter;
                    }
                }
            }
            
            if (meshFilterProp.objectReferenceValue != null && meshRendererProp.objectReferenceValue == null)
            {
                MeshFilter filter = meshFilterProp.objectReferenceValue as MeshFilter;
                if (filter != null)
                {
                    MeshRenderer renderer = filter.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        meshRendererProp.objectReferenceValue = renderer;
                    }
                }
            }
            
            EditorGUILayout.Space();
            
            // Texture Settings
            SerializedProperty useManualTextureProp = serializedObject.FindProperty("useManualTexture");
            EditorGUILayout.PropertyField(useManualTextureProp);
            
            if (useManualTextureProp.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("manualTexture"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("texturePropertyName"));
                
                if (serializedObject.FindProperty("manualTexture").objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox("Please assign a texture for manual mode.", MessageType.Warning);
                }
            }
            else
            {
                // Validation for auto-detection mode
                if (meshRendererProp.objectReferenceValue == null || meshFilterProp.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox("Please assign both MeshRenderer and MeshFilter components.", MessageType.Warning);
                }
                else
                {
                    MeshRenderer renderer = meshRendererProp.objectReferenceValue as MeshRenderer;
                    if (renderer != null)
                    {
                        bool hasTexture = HasAnyTexture(renderer.material);
                        if (!hasTexture)
                        {
                            EditorGUILayout.HelpBox("The MeshRenderer has no texture assigned.", MessageType.Warning);
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("Texture found and ready for placement.", MessageType.Info);
                        }
                    }
                }
            }
            
            EditorGUI.indentLevel--;
        }
    }
    
    private void DrawPrefabSettings()
    {
        showPrefabSettings = EditorGUILayout.Foldout(showPrefabSettings, "Prefab Settings", true);
        if (showPrefabSettings)
        {
            EditorGUI.indentLevel++;
            
            SerializedProperty prefabProp = serializedObject.FindProperty("prefabToPlace");
            EditorGUILayout.PropertyField(prefabProp);
            
            if (prefabProp.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Please assign a prefab to place.", MessageType.Warning);
            }
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("prefabScale"));
            
            SerializedProperty randomizeRotationProp = serializedObject.FindProperty("randomizeRotation");
            EditorGUILayout.PropertyField(randomizeRotationProp);
            
            SerializedProperty randomizeScaleProp = serializedObject.FindProperty("randomizeScale");
            EditorGUILayout.PropertyField(randomizeScaleProp);
            
            if (randomizeScaleProp.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("scaleVariation"));
            }
            
            SerializedProperty randomizeRotationYOnlyProp = serializedObject.FindProperty("randomizeRotationYOnly");
            EditorGUILayout.PropertyField(randomizeRotationYOnlyProp);
            
            EditorGUI.indentLevel--;
        }
    }
    
    private void DrawColorSettings()
    {
        showColorSettings = EditorGUILayout.Foldout(showColorSettings, "Color Settings", true);
        if (showColorSettings)
        {
            EditorGUI.indentLevel++;
            
            SerializedProperty targetColorProp = serializedObject.FindProperty("targetColor");
            EditorGUILayout.PropertyField(targetColorProp);
            
            SerializedProperty toleranceProp = serializedObject.FindProperty("colorTolerance");
            EditorGUILayout.LabelField("Color Tolerance");
            EditorGUILayout.Slider(toleranceProp, 0f, 1f);
            
            SerializedProperty useRangeProp = serializedObject.FindProperty("useColorRange");
            EditorGUILayout.PropertyField(useRangeProp);
            
            if (useRangeProp.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("colorRangeMin"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("colorRangeMax"));
            }
            
            EditorGUI.indentLevel--;
        }
    }
    
    private void DrawPlacementSettings()
    {
        showPlacementSettings = EditorGUILayout.Foldout(showPlacementSettings, "Placement Settings", true);
        if (showPlacementSettings)
        {
            EditorGUI.indentLevel++;
            
            SerializedProperty densityProp = serializedObject.FindProperty("placementDensity");
            EditorGUILayout.LabelField("Placement Density");
            EditorGUILayout.Slider(densityProp, 0f, 1f);
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("placementHeight"));
            
            SerializedProperty useNormalProp = serializedObject.FindProperty("useSurfaceNormal");
            EditorGUILayout.PropertyField(useNormalProp);
            
            if (useNormalProp.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("normalOffset"));
            }
            
            EditorGUI.indentLevel--;
        }
    }
    
    private void DrawRandomizationSettings()
    {
        showRandomizationSettings = EditorGUILayout.Foldout(showRandomizationSettings, "Randomization Settings", true);
        if (showRandomizationSettings)
        {
            EditorGUI.indentLevel++;
            
            SerializedProperty posRandomProp = serializedObject.FindProperty("positionRandomness");
            EditorGUILayout.LabelField("Position Randomness");
            EditorGUILayout.Slider(posRandomProp, 0f, 1f);
            
            SerializedProperty rotRandomProp = serializedObject.FindProperty("rotationRandomness");
            EditorGUILayout.LabelField("Rotation Randomness");
            EditorGUILayout.Slider(rotRandomProp, 0f, 180f);
            
            SerializedProperty seedProp = serializedObject.FindProperty("seed");
            EditorGUILayout.PropertyField(seedProp);
            
            if (GUILayout.Button("Randomize Seed"))
            {
                seedProp.intValue = Random.Range(0, 10000);
            }
            
            EditorGUI.indentLevel--;
        }
    }
    
    private void DrawDebugInfo()
    {
        showDebugInfo = EditorGUILayout.Foldout(showDebugInfo, "Debug Info", true);
        if (showDebugInfo)
        {
            EditorGUI.indentLevel++;
            
            if (placer != null)
            {
                EditorGUILayout.LabelField("Target Color", ColorUtility.ToHtmlStringRGB(placer.GetTargetColor()));
                EditorGUILayout.LabelField("Placement Density", $"{placer.GetPlacementDensity():P0}");
                EditorGUILayout.LabelField("Color Tolerance", placer.GetColorTolerance().ToString("F2"));
                EditorGUILayout.LabelField("Position Randomness", placer.GetPositionRandomness().ToString("F2"));
                EditorGUILayout.LabelField("Seed", placer.GetSeed().ToString());
            }
            
            EditorGUI.indentLevel--;
        }
    }
    
    private void DrawActionButtons()
    {
        EditorGUILayout.BeginHorizontal();
        
        GUI.enabled = CanPlacePrefabs();
        if (GUILayout.Button("Place Prefabs"))
        {
            placer.PlacePrefabs();
        }
        
        GUI.enabled = true;
        if (GUILayout.Button("Clear Prefabs"))
        {
            placer.ClearPlacedPrefabs();
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private bool HasAnyTexture(Material material)
    {
        if (material == null) return false;
        
        // Try common texture property names
        string[] textureProperties = {
            "_MainTex",           // Standard shader
            "_BaseMap",           // URP/Lit shader
            "_BaseColorMap",      // Some custom shaders
            "_Albedo",            // Some custom shaders
            "_Diffuse",           // Legacy shaders
            "_ColorMap",          // Some custom shaders
            "_Texture",           // Generic
            "_MainTexture"        // Some custom shaders
        };
        
        // First try mainTexture (most common)
        if (material.mainTexture != null) return true;
        
        // Try specific texture properties
        foreach (string propertyName in textureProperties)
        {
            if (material.HasProperty(propertyName))
            {
                Texture texture = material.GetTexture(propertyName);
                if (texture != null) return true;
            }
        }
        
        return false;
    }
    
    private bool CanPlacePrefabs()
    {
        return placer != null && 
               placer.GetTargetMeshRenderer() != null && 
               placer.GetTargetMeshFilter() != null && 
               placer.GetPrefabToPlace() != null;
    }
}

// Extension methods to access private fields
public static class TextureBasedPrefabPlacerExtensions
{
    public static Color GetTargetColor(this TextureBasedPrefabPlacer placer)
    {
        var field = typeof(TextureBasedPrefabPlacer).GetField("targetColor", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (Color)field.GetValue(placer);
    }
    
    public static float GetPlacementDensity(this TextureBasedPrefabPlacer placer)
    {
        var field = typeof(TextureBasedPrefabPlacer).GetField("placementDensity", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (float)field.GetValue(placer);
    }
    
    public static float GetColorTolerance(this TextureBasedPrefabPlacer placer)
    {
        var field = typeof(TextureBasedPrefabPlacer).GetField("colorTolerance", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (float)field.GetValue(placer);
    }
    
    public static float GetPositionRandomness(this TextureBasedPrefabPlacer placer)
    {
        var field = typeof(TextureBasedPrefabPlacer).GetField("positionRandomness", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (float)field.GetValue(placer);
    }
    
    public static int GetSeed(this TextureBasedPrefabPlacer placer)
    {
        var field = typeof(TextureBasedPrefabPlacer).GetField("seed", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (int)field.GetValue(placer);
    }
    
    public static MeshRenderer GetTargetMeshRenderer(this TextureBasedPrefabPlacer placer)
    {
        var field = typeof(TextureBasedPrefabPlacer).GetField("targetMeshRenderer", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (MeshRenderer)field.GetValue(placer);
    }
    
    public static MeshFilter GetTargetMeshFilter(this TextureBasedPrefabPlacer placer)
    {
        var field = typeof(TextureBasedPrefabPlacer).GetField("targetMeshFilter", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (MeshFilter)field.GetValue(placer);
    }
    
    public static GameObject GetPrefabToPlace(this TextureBasedPrefabPlacer placer)
    {
        var field = typeof(TextureBasedPrefabPlacer).GetField("prefabToPlace", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (GameObject)field.GetValue(placer);
    }
} 