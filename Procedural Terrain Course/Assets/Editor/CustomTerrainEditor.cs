using UnityEngine;
using UnityEditor;
using EditorGUITable;

[CustomEditor(typeof(CustomTerrain))]
[CanEditMultipleObjects]


public class CustomTerrainEditor : Editor {
    //======================================= Variables =============================================
    #region Variables

    //--------------------------- Properties -------------------------------
    #region Properties

    //--------- Reset Terrain --------------
    SerializedProperty resetTerrain;
    //--------- Random Heights -------------
    SerializedProperty randomHeightRange;
    //--------- Load Texture --------------
    SerializedProperty heightMapScale;
    SerializedProperty heightMapImage;
    //--------- Perlin Noise --------------
    SerializedProperty perlinXScale;
    SerializedProperty perlinYScale;
    SerializedProperty perlinOffsetX;
    SerializedProperty perlinOffsetY;
    SerializedProperty perlinOctaves;
    SerializedProperty perlinPersistance;
    SerializedProperty perlinHeightScale;
    //---- Multiple Perline Noise --------
    GUITableState perlinParameterTable;
    SerializedProperty perlinParameters;
    //---------- Voronoi ----------------
    SerializedProperty voronoiPeaks;
    SerializedProperty voronoiFallOff;
    SerializedProperty voronoiDropOff;
    SerializedProperty voronoiMinHeight;
    SerializedProperty voronoiMaxHeight;
    SerializedProperty voronoiType;

    #endregion Properties
    //--------------------------- Foldouts --------------------------------
    #region Foldouts

    bool showRandom = false;
    bool showLoadHeights = false;
    bool showPerlinNoise = false;
    bool showMultiplePerlin = false;
    bool showVoronoi = false;

    #endregion Foldouts

    #endregion Variables

    void OnEnable()
    {
        //--------- Reset Terrain --------------
        resetTerrain = serializedObject.FindProperty("resetTerrain");
        //--------- Random Heights --------------
        randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        //--------- Load Texture --------------
        heightMapScale = serializedObject.FindProperty("heightMapScale");
        heightMapImage = serializedObject.FindProperty("heightMapImage");
        //--------- Perlin Noise --------------
        perlinXScale = serializedObject.FindProperty("perlinXScale");
        perlinYScale = serializedObject.FindProperty("perlinYScale");
        perlinOffsetX = serializedObject.FindProperty("perlinOffsetX");
        perlinOffsetY = serializedObject.FindProperty("perlinOffsetY");
        perlinOctaves = serializedObject.FindProperty("perlinOctaves");
        perlinPersistance = serializedObject.FindProperty("perlinPersistance");
        perlinHeightScale = serializedObject.FindProperty("perlinHeightScale");
        //---- Multiple Perline Noise --------
        perlinParameterTable = new GUITableState("perlinParameterTable");
        perlinParameters = serializedObject.FindProperty("perlinParameters");
        //---------- Voronoi ----------------
        voronoiPeaks = serializedObject.FindProperty("voronoiPeaks");
        voronoiFallOff = serializedObject.FindProperty("voronoiFallOff");
        voronoiDropOff = serializedObject.FindProperty("voronoiDropOff");
        voronoiMinHeight = serializedObject.FindProperty("voronoiMinHeight");
        voronoiMaxHeight = serializedObject.FindProperty("voronoiMaxHeight");
        voronoiType = serializedObject.FindProperty("voronoiType");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        CustomTerrain terrain = (CustomTerrain)target;
        // Reset terrain toggle button -- Controls whether functions will reset the terrain before running or
        // add their values to existing terrain data
        EditorGUILayout.PropertyField(resetTerrain);

        #region Reset Terrain
        //Reset the terrain heights to 0
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Reset All Heights To 0", EditorStyles.boldLabel);
        if (GUILayout.Button("Reset Terrain"))
        {
            terrain.ResetTerrain();
        }
        #endregion

        #region Random Heights
        //Drives foldout display for Random Heights Section
        showRandom = EditorGUILayout.Foldout(showRandom, "Random");
        if(showRandom)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set Heights Between Random Values", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(randomHeightRange);
            if(GUILayout.Button("Random Heights"))
            {
                terrain.RandomTerrain();
            }
        }
        #endregion

        #region Load From Texture
        //Drives foldout display for Load Texture Section
        showLoadHeights = EditorGUILayout.Foldout(showLoadHeights, "Load From Texture");
        if (showLoadHeights)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set Heights According To Texture", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(heightMapImage);
            EditorGUILayout.PropertyField(heightMapScale);
            if (GUILayout.Button("Load Texture"))
            {
                terrain.LoadTexture();
            }
        }
        #endregion

        #region Single Perlin Noise
        //Drives foldout for Single Perlin Noise Section
        showPerlinNoise = EditorGUILayout.Foldout(showPerlinNoise, "Single Perlin Noise");
        if (showPerlinNoise)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Use Perlin Noise to Generate Heights", EditorStyles.boldLabel);
            EditorGUILayout.Slider(perlinXScale, 0, .05f, new GUIContent("X Scale"));
            EditorGUILayout.Slider(perlinYScale, 0, .05f, new GUIContent("Y Scale"));
            EditorGUILayout.IntSlider(perlinOffsetX, 0, 10000, new GUIContent("X Offset"));
            EditorGUILayout.IntSlider(perlinOffsetY, 0, 10000, new GUIContent("Y Offset"));
            EditorGUILayout.IntSlider(perlinOctaves, 1, 10, new GUIContent("Octaves"));
            EditorGUILayout.Slider(perlinPersistance, 0.1f, 10, new GUIContent("Persistance"));
            EditorGUILayout.Slider(perlinHeightScale, 0, 1, new GUIContent("Height Scale"));

            if (GUILayout.Button("Perlin"))
            {
                terrain.Perlin();
            }
        }

        #endregion

        #region Multiple Perlin Noise

        showMultiplePerlin = EditorGUILayout.Foldout(showMultiplePerlin, "Multiple Perlin Noise");
        if(showMultiplePerlin)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Multiple Perlin Noise", EditorStyles.boldLabel);
            //Draw table for Perlin Layers
            perlinParameterTable = GUITableLayout.DrawTable(perlinParameterTable, perlinParameters);
            GUILayout.Space(20);                            // Add space for formatting
            EditorGUILayout.BeginHorizontal();              //Start formatting horizontally
            if(GUILayout.Button("+"))                       //Add layer button
            {
                terrain.AddNewPerlin();
            }
            if(GUILayout.Button("-"))                       //Remove layer button
            {
                terrain.RemovePerlin();
            }
            EditorGUILayout.EndHorizontal();                //Stop formatting horizontally
            if(GUILayout.Button("Apply Multiple Perlin"))   //Apply the multiple layers of Perlin Noise
            {
                terrain.MultiplePerlinTerrain();
            }

        }
        #endregion

        #region Voronoi
        //Drives foldout display for Random Heights Section
        showVoronoi = EditorGUILayout.Foldout(showVoronoi, "Voronoi");
        if (showVoronoi)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Generate Mountains using Voronoi Tesselation", EditorStyles.boldLabel);
            EditorGUILayout.IntSlider(voronoiPeaks, 1, 50, new GUIContent("Peak Count"));
            EditorGUILayout.Slider(voronoiFallOff, 0, 10, new GUIContent("Falloff"));
            EditorGUILayout.Slider(voronoiDropOff, 0, 10, new GUIContent("Drop Off"));
            EditorGUILayout.Slider(voronoiMinHeight, 0, 1, new GUIContent("Min Height"));
            EditorGUILayout.Slider(voronoiMaxHeight, 0, 1, new GUIContent("Max Height"));
            EditorGUILayout.PropertyField(voronoiType);

            if (GUILayout.Button("Voronoi"))
            {
                terrain.Voronoi();
            }
        }
        #endregion

        serializedObject.ApplyModifiedProperties();
    }

}
