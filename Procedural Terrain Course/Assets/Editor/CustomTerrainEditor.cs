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
    //------- Midpoint Displacement -------
    SerializedProperty MPDheightMin;
    SerializedProperty MPDheightMax;
    SerializedProperty MPDheightDampenerPower;
    SerializedProperty MPDroughness;
    //----------- Smooth -----------------
    SerializedProperty smoothIterations;
    //----------- Splat Maps -------------
    GUITableState splatMapTable;
    SerializedProperty splatHeights;
    //SerializedProperty splatXScale;
    //SerializedProperty splatYScale;
    //SerializedProperty splatScalar;
    //SerializedProperty splatOffset;
    //----------- Vegetation ------------
    SerializedProperty vegetation;
    SerializedProperty maxTrees;
    SerializedProperty treeSpacing;
    GUITableState vegetationTable;
    //----------- Details ------------
    GUITableState detailsTable;
    SerializedProperty details;
    SerializedProperty maxDetails;
    SerializedProperty detailSpacing;
    //-------- Water -------------
    SerializedProperty waterHeight;
    SerializedProperty waterGameObject;
    SerializedProperty shoreLineMaterial;



    #endregion Properties
    //--------------------------- Foldouts --------------------------------
    #region Foldouts

    bool showRandom = false;
    bool showLoadHeights = false;
    bool showPerlinNoise = false;
    bool showMultiplePerlin = false;
    bool showVoronoi = false;
    bool showMPD = false;
    bool showSplatMaps = false;
    bool showHeights = false;
    bool showVegetation = false;
    bool showDetails = false;
    bool showWater = false;

    #endregion Foldouts

    //--------- Height Map -------------
    Texture2D hmTexture;

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
        //------- Midpoint Displacement -------
        MPDheightMin = serializedObject.FindProperty("MPDheightMin");
        MPDheightMax = serializedObject.FindProperty("MPDheightMax");
        MPDheightDampenerPower = serializedObject.FindProperty("MPDheightDampenerPower");
        MPDroughness = serializedObject.FindProperty("MPDroughness");
        //--------------- Smooth -------------
        smoothIterations = serializedObject.FindProperty("smoothIterations");
        //----------- Splat Maps -------------
        splatHeights = serializedObject.FindProperty("splatHeights");
        //splatXScale = serializedObject.FindProperty("splatXScale");
        //splatYScale = serializedObject.FindProperty("splatYScale");
        //splatScalar = serializedObject.FindProperty("splatScalar");
        //splatOffset = serializedObject.FindProperty("splatOffset");

        //--------- Height Map -------------
        hmTexture = new Texture2D(513, 513, TextureFormat.ARGB32, false);
        //--------- Vegetation -------------
        vegetation = serializedObject.FindProperty("vegetation");
        maxTrees = serializedObject.FindProperty("maxTrees");
        treeSpacing = serializedObject.FindProperty("treeSpacing");
        //--------- Details ---------------
        details = serializedObject.FindProperty("details");
        maxDetails = serializedObject.FindProperty("maxDetails");
        detailSpacing = serializedObject.FindProperty("detailSpacing");
        //-------- Water -------------
        waterHeight = serializedObject.FindProperty("waterHeight");
        waterGameObject = serializedObject.FindProperty("waterGameObject");
        shoreLineMaterial = serializedObject.FindProperty("shoreLineMaterial");
    }

    Vector2 scrollPos; //Track scrollbar position
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        CustomTerrain terrain = (CustomTerrain)target;

        //Start Scrollbar
        Rect r = EditorGUILayout.BeginVertical();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(r.width), GUILayout.Height(r.height));
        EditorGUI.indentLevel++;

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

        #region Smooth Terrain
        //Smooth out the terrain
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Smooth Terrain", EditorStyles.boldLabel);
        EditorGUILayout.IntSlider(smoothIterations, 1, 10, new GUIContent("Iterations"));
        if (GUILayout.Button("Smooth"))
        {
            terrain.Smooth();
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

        #region Midpoint Displacement
        //Drives foldout display for Random Heights Section
        showMPD = EditorGUILayout.Foldout(showMPD, "Midpoint Displacement");
        if (showMPD)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.PropertyField(MPDheightMin);
            EditorGUILayout.PropertyField(MPDheightMax);
            EditorGUILayout.PropertyField(MPDheightDampenerPower);
            EditorGUILayout.PropertyField(MPDroughness);

            if (GUILayout.Button("MPD"))
            {
                terrain.MidpointDisplacement();
            }
        }
        #endregion

        #region Splatmaps

        showSplatMaps = EditorGUILayout.Foldout(showSplatMaps, "Splat Maps");
        if (showSplatMaps)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Splat Maps", EditorStyles.boldLabel);
            //EditorGUILayout.Slider(splatXScale, 0.001f, .1f, new GUIContent("Noise X Scale"));
            //EditorGUILayout.Slider(splatYScale, 0.001f, .1f, new GUIContent("Noise Y Scale"));
            //EditorGUILayout.Slider(splatScalar, 0.001f, .5f, new GUIContent("Noise Multiplier"));
            //EditorGUILayout.Slider(splatOffset, 0, .1f, new GUIContent("Blend Offset"));
            //Draw table for Splat Map Layers
            splatMapTable = GUITableLayout.DrawTable(splatMapTable, splatHeights);
            GUILayout.Space(20);                            // Add space for formatting
            EditorGUILayout.BeginHorizontal();              //Start formatting horizontally
            if (GUILayout.Button("+"))                       //Add layer button
            {
                terrain.AddNewSplatHeight();
            }
            if (GUILayout.Button("-"))                       //Remove layer button
            {
                terrain.RemoveSplatHeight();
            }
            EditorGUILayout.EndHorizontal();                //Stop formatting horizontally
            if (GUILayout.Button("Apply Splat Maps"))   //Apply the multiple layers of Perlin Noise
            {
                terrain.SplatMaps();
            }

        }
        #endregion

        #region Heightmap

        showHeights = EditorGUILayout.Foldout(showHeights, "Height Map");
        if(showHeights)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            int hmtSize = (int)(EditorGUIUtility.currentViewWidth - 100);
            GUILayout.Label(hmTexture, GUILayout.Width(hmtSize), GUILayout.Height(hmtSize));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Refresh", GUILayout.Width(hmtSize)))
            {
                float[,] heightMap = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight);

                for (int y = 0; y < terrain.terrainData.heightmapHeight; y++)
                {
                    for (int x = 0; x < terrain.terrainData.heightmapHeight; x++)
                    {
                        hmTexture.SetPixel(x, y, new Color(heightMap[x, y], heightMap[x, y], heightMap[x, y], 1));
                    }
                }
                hmTexture.Apply();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        #endregion

        #region Vegetation

        showVegetation = EditorGUILayout.Foldout(showVegetation, "Vegetation");
        if (showVegetation)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Vegetation", EditorStyles.boldLabel);

            EditorGUILayout.IntSlider(maxTrees, 0, 10000, new GUIContent("Maximum Trees"));
            EditorGUILayout.IntSlider(treeSpacing, 2, 20, new GUIContent("Tree Spacing"));
           
            vegetationTable = GUITableLayout.DrawTable(vegetationTable, vegetation);
            GUILayout.Space(20);                            // Add space for formatting
            EditorGUILayout.BeginHorizontal();              //Start formatting horizontally
            if (GUILayout.Button("+"))                       //Add layer button
            {
                terrain.AddNewVegetation();
            }
            if (GUILayout.Button("-"))                       //Remove layer button
            {
                terrain.RemoveVegetation();
            }
            EditorGUILayout.EndHorizontal();                //Stop formatting horizontally
            if (GUILayout.Button("Apply Vegetation"))   //Apply the multiple layers of Perlin Noise
            {
                terrain.PlantVegetation();
            }

        }

        #endregion

        #region Details

        showDetails = EditorGUILayout.Foldout(showDetails, "Details");
        if (showDetails)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Details", EditorStyles.boldLabel);

            EditorGUILayout.IntSlider(maxDetails, 0, 2000, new GUIContent("Details View Distance"));
            EditorGUILayout.IntSlider(detailSpacing, 2, 20, new GUIContent("Detail Spacing"));

            terrain.GetComponent<Terrain>().detailObjectDistance = maxDetails.intValue; //Change view distance
            detailsTable = GUITableLayout.DrawTable(detailsTable, details);
            GUILayout.Space(20);                            // Add space for formatting
            EditorGUILayout.BeginHorizontal();              //Start formatting horizontally
            if (GUILayout.Button("+"))                       //Add layer button
            {
                terrain.AddNewDetail();
            }
            if (GUILayout.Button("-"))                       //Remove layer button
            {
                terrain.RemoveDetail();
            }
            EditorGUILayout.EndHorizontal();                //Stop formatting horizontally
            if (GUILayout.Button("Apply Details"))   //Apply the multiple layers of Perlin Noise
            {
                terrain.PlaceDetails();
            }

        }

        #endregion

        #region Water

        showWater = EditorGUILayout.Foldout(showWater, "Water");
        if(showWater)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Water", EditorStyles.boldLabel);
            EditorGUILayout.Slider(waterHeight, 0, 1, new GUIContent("Water Height"));
            EditorGUILayout.PropertyField(waterGameObject);

            if(GUILayout.Button("Add Water"))
            {
                terrain.AddWater();
            }

            EditorGUILayout.PropertyField(shoreLineMaterial);
            if(GUILayout.Button("Add Shore Line"))
            {
                terrain.DrawShoreLine();
            }
        }

        #endregion

        //End Scrollbar
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }

}
