using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]

public class CustomTerrain : MonoBehaviour {
    //============================================= Terrain Variables =====================================================
    #region Terrain Variables
    //------------ Reset Terrain ----------------
    public bool resetTerrain = true; // When true, all functions will overwrite existing terrain data
                                     // When false, all functions will add heights to existing terrain data
    //----------- Textures ----------------------
    public Texture2D heightMapImage;

    //------------ Vectors ----------------------------
    public Vector2 randomHeightRange = new Vector2(0, 0.3f);
    public Vector3 heightMapScale = new Vector3(1, 1, 1);


    //------------ Perlin Noise Variables ------------------------
    public float perlinXScale = 0.01f;
    public float perlinYScale = 0.01f;
    public float perlinPersistance = 8f;
    public float perlinHeightScale = 0.09f;
    public int perlinOffsetX = 0;
    public int perlinOffsetY = 0;
    public int perlinOctaves = 3;

    //------------ Multiple Perlin Noise Variables ------------------------
    [System.Serializable]
    public class PerlinParameters
    {
        public float mPerlinXScale = 0.01f;
        public float mPerlinYScale = 0.01f;
        public float mPerlinPersistance = 8f;
        public float mPerlinHeightScale = 0.09f;
        public int mPerlinOffsetX = 0;
        public int mPerlinOffsetY = 0;
        public int mPerlinOctaves = 3;
        public bool remove = false;
    }

    public List<PerlinParameters> perlinParameters = new List<PerlinParameters>()
    {
        new PerlinParameters()
    };

    //---------------- Voronoi ---------------------
    public int voronoiPeaks = 5;
    public float voronoiFallOff = 0.2f;
    public float voronoiDropOff = 0.6f;
    public float voronoiMinHeight = 0.1f;
    public float voronoiMaxHeight = 0.5f;
    public enum VoronoiType {  Linear = 0, Power = 1, Combined = 2, PowerSin = 3 }
    public VoronoiType voronoiType = VoronoiType.Linear;

    //--------------- Midpoint Displacement ----------
    public float MPDheightMin = -10.0f;
    public float MPDheightMax = 10.0f;
    public float MPDheightDampenerPower = 2.0f;
    public float MPDroughness = 2.0f;
    //-------------- Smooth ------------
    public int smoothIterations = 1;
    //------------- Splatmaps ---------------------
    [System.Serializable]
    public class SplatHeights
    {
        public Texture2D texture = null;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public float minSlope = 0;
        public float maxSlope = 90;
        public Vector2 tileOffset = new Vector2(0, 0);
        public Vector2 tileSize = new Vector2(50, 50);
        public float splatXScale = 0.01f;
        public float splatYScale = 0.01f;
        public float splatScalar = .02f;
        public float splatOffset = 0.01f;
        public bool remove = false;
    }
    public List<SplatHeights> splatHeights = new List<SplatHeights>()
    {
        new SplatHeights()
    };
    //----------- Trees --------------
    [System.Serializable]
    public class Vegetation
    {
        public GameObject mesh;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public float minSlope = 0;
        public float maxSlope = 90;
        public bool remove = false;
    }
    public List<Vegetation> vegetation = new List<Vegetation>()
    {
        new Vegetation()
    };

    public int maxTrees = 5000;
    public int treeSpacing = 5;
    

    //----------- Terrain and TerrainData ---------------------
    public Terrain terrain;
    public TerrainData terrainData;
    #endregion
    //============================================= Terrain Functions =====================================================
    #region Terrain Functions

    //Creates a new, blank heightmap or gets the existing heightmap depending on
    //if we want to reset the terrain or not
    float[,] GetHeightMap()
    {
        if (!resetTerrain)
        {
            return terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);
        }
        else
            return new float[terrainData.heightmapWidth, terrainData.heightmapHeight];
    }

    //Returns neighboring positions in a 2D array -- Only returns the position if it actually exists
   List<Vector2> GenerateNeighbors(Vector2 pos, int width, int height)
    {
        List<Vector2> neighbors = new List<Vector2>();
        for (int y = -1; y < 2; y++)
        {
            for (int x = -1; x < 2; x++)
            {
                if(!(x == 0 & y == 0))
                {
                    Vector2 nPos = new Vector2(Mathf.Clamp(pos.x + x, 0, width - 1),
                                                Mathf.Clamp(pos.y + y, 0, height - 1));
                    if (!neighbors.Contains(nPos))
                        neighbors.Add(nPos);
                }
            }
        }
        return neighbors;
    }

    //Sets all heights to be 0
    public void ResetTerrain()
    {
        float[,] heightMap;
        heightMap = new float[terrainData.heightmapWidth, terrainData.heightmapHeight];
        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                heightMap[x, y] = 0;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    //Smooth out the terrain
    public void Smooth()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);
        float smoothProgress = 0;
        EditorUtility.DisplayProgressBar("Smoothing Terrain", "Progress", smoothProgress);

        for (int i = 0; i < smoothIterations; i++)
        {
            
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                for (int x = 0; x < terrainData.heightmapWidth; x++)
                {
                    float avgHeight = heightMap[x, y];
                    List<Vector2> neighbors = GenerateNeighbors(new Vector2(x, y), terrainData.heightmapWidth, terrainData.heightmapHeight);
                    foreach (Vector2 n in neighbors)
                    {
                        avgHeight += heightMap[(int)n.x, (int)n.y];
                    }
                    heightMap[x, y] = avgHeight / ((float)neighbors.Count + 1);
                }
            }

            smoothProgress++;
            EditorUtility.DisplayProgressBar("Smoothing Terrain", "Progress", smoothProgress / smoothIterations);

        }
         terrainData.SetHeights(0, 0, heightMap);
        EditorUtility.ClearProgressBar();
    }

    //Adds a random height to the current terrain height
    public void RandomTerrain()
    {
        float[,] heightMap = GetHeightMap();
        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                heightMap[x, y] += UnityEngine.Random.Range(randomHeightRange.x, randomHeightRange.y);
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    //Loads height data from an image
    public void LoadTexture()
    {
        float[,] heightMap = GetHeightMap();
        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                heightMap[x, y] += heightMapImage.GetPixel((int)(x * heightMapScale.x),
                                                          (int)(y * heightMapScale.z)).grayscale
                                                          * heightMapScale.y;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    //Generates heights from a single Perlin Noise layer using Fractal Brownian Method
    public void Perlin()
    {
        float[,] heightMap = GetHeightMap();

        //Assign heights using Fractal Brownian Motion function
        for (int y = 0; y < terrainData.heightmapHeight; y++)
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                heightMap[x, y] += Utils.fBM( (x + perlinOffsetX) * perlinXScale,
                                             (y + perlinOffsetY) * perlinYScale,
                                             perlinOctaves,
                                             perlinPersistance) * perlinHeightScale; //perlinHeightScale gives extra control over the final height
                                                                                // without it, the terrain would be able to reach the max height set by Unity
            }
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    //Generates heights from multiple Perlin Noise layers using Fractal Brownian Method
    #region Multiple Perlin Noise
    public void MultiplePerlinTerrain()
    {
        float[,] heightMap = GetHeightMap();

        //Assign heights using Fractal Brownian Motion function for each layer of Perlin Noise
        for (int y = 0; y < terrainData.heightmapHeight; y++)
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                foreach (PerlinParameters p in perlinParameters)
                {
                    heightMap[x, y] += Utils.fBM((x + p.mPerlinOffsetX) * p.mPerlinXScale,
                                                 (y + p.mPerlinOffsetY) * p.mPerlinYScale,
                                                 p.mPerlinOctaves,
                                                 p.mPerlinPersistance) * p.mPerlinHeightScale; //perlinHeightScale gives extra control over the final height
                                                                                               // without it, the terrain would be able to reach the max height set by Unity
                }
                
            }
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    //Adds another Perlin layer
    public void AddNewPerlin()
    {
        perlinParameters.Add(new PerlinParameters());
    }

    //Removes all layers that are marked for removal
    public void RemovePerlin()
    {
        List<PerlinParameters> keptPerlinParameters = new List<PerlinParameters>();
        for (int i = 0; i < perlinParameters.Count; i++)
        {
            if(!perlinParameters[i].remove)
            {
                keptPerlinParameters.Add(perlinParameters[i]);
            }
        }
        if (keptPerlinParameters.Count == 0) //If we don't want to keep any
        {
            keptPerlinParameters.Add(perlinParameters[0]); //We keep the first one because GUITable Layout wants at least one entry
        }
        perlinParameters = keptPerlinParameters;
    }

    #endregion Multiple Perlin Noise

    public void Voronoi()
    {
        float[,] heightmap = GetHeightMap();
        float maxDistance = Vector2.Distance(new Vector2(0, 0), new Vector2(terrainData.heightmapWidth, terrainData.heightmapHeight));
        //Loop through to create each peak
        for (int i = 0; i < voronoiPeaks; i++)
        {
            
            Vector3 peak = new Vector3(UnityEngine.Random.Range(0, terrainData.heightmapWidth),
                                       UnityEngine.Random.Range(voronoiMinHeight, voronoiMaxHeight),
                                       UnityEngine.Random.Range(0, terrainData.heightmapHeight)
                                       );
            //Prevents divots if therrain is already raised
            if (heightmap[(int)peak.x, (int)peak.z] < peak.y)
                heightmap[(int)peak.x, (int)peak.z] = peak.y;
            else
                continue;

            //Adjust terrain surrounding the peak
            Vector2 peakLocation = new Vector2(peak.x, peak.z);

            //Loop through surrounding terrain
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                for (int x = 0; x < terrainData.heightmapWidth; x++)
                {
                    if (!(x == peak.x && y == peak.y))
                    {
                        float distanceToPeak = Vector2.Distance(peakLocation, new Vector2(x, y)) / maxDistance;
                        float h;
                        if (voronoiType == VoronoiType.Combined) //Combined function
                            h = peak.y - distanceToPeak * voronoiFallOff - Mathf.Pow(distanceToPeak, voronoiDropOff);
                        else if (voronoiType == VoronoiType.Power)
                            h = peak.y - Mathf.Pow(distanceToPeak, voronoiDropOff) * voronoiFallOff; // Power function
                        else if (voronoiType == VoronoiType.PowerSin)
                            h = peak.y - Mathf.Pow(distanceToPeak * 3, voronoiFallOff) - Mathf.Sin(distanceToPeak * 2 * Mathf.PI) / voronoiDropOff;
                        else
                            h = peak.y - distanceToPeak * voronoiFallOff; //Linear function

                        //Prevents us from overwritng other peaks with flat land
                        if (h > heightmap[x,y])
                        {
                            heightmap[x, y] = h;
                        }
                    }
                }
            }
        }
        terrainData.SetHeights(0, 0, heightmap);
    }

    public void MidpointDisplacement()
    {
        float[,] heightMap = GetHeightMap();
        int width = terrainData.heightmapWidth - 1; //This function relies on powers of 2, heightmapWidth is 513 - 1 = 512
        int squareSize = width; //Size of the square is equal to the width, used for calculating corners
        float heightMin = MPDheightMin;
        float heightMax = MPDheightMax;
        float heightDampener = (float)Mathf.Pow(MPDheightDampenerPower, -1 * MPDroughness);

        int cornerX, cornerY; //Coordinates of far corners
        int midX, midY; //Coordinates of middle point
        int pmidXL, pmidXR, pmidYU, pmidYD;

        ////Set corners to random height
        //heightMap[0, 0] = UnityEngine.Random.Range(0f, 0.2f);
        //heightMap[0, terrainData.heightmapHeight - 2] = UnityEngine.Random.Range(0f, 0.2f);
        //heightMap[terrainData.heightmapWidth - 2, 0] = UnityEngine.Random.Range(0f, 0.2f);
        //heightMap[terrainData.heightmapWidth - 2, terrainData.heightmapHeight - 2] = UnityEngine.Random.Range(0f, 0.2f);

        while(squareSize > 0)
        {
            for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {
                    //Find far corners
                    cornerX = (x + squareSize);
                    cornerY = (y + squareSize);
                    //Find midpoint
                    midX = (int)(x + squareSize / 2.0f);
                    midY = (int)(y + squareSize / 2.0f);

                    //Set midpoint to average of corners
                    heightMap[midX, midY] = (float)((heightMap[x, y] +
                                                     heightMap[cornerX, y] +
                                                     heightMap[x, cornerY] +
                                                     heightMap[cornerX, cornerY]) / 4.0f + 
                                                     UnityEngine.Random.Range(heightMin, heightMax));
                }
            }

            for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {
                    //Find far corners
                    cornerX = (x + squareSize);
                    cornerY = (y + squareSize);
                    //Find midpoint
                    midX = (int)(x + squareSize / 2.0f);
                    midY = (int)(y + squareSize / 2.0f);
                    //Points for finding heights of diamond points
                    pmidXR = (int)(midX + squareSize);
                    pmidYU = (int)(midY + squareSize);
                    pmidXL = (int)(midX - squareSize);
                    pmidYD = (int)(midY - squareSize);

                    //Proceed to next step if there are points that will produce out of bounds exception
                    if (pmidXL <= 0 || pmidYD <= 0 || pmidXR >= width - 1 || pmidYU >= width - 1)
                        continue;

                    //Find diamond point heights
                    //Calculate squar value for bottom
                    heightMap[midX, y] = (float)((heightMap[midX, midY] +
                                                     heightMap[x, y] +
                                                     heightMap[midX, pmidYD] +
                                                     heightMap[cornerX, y]) / 4.0f +
                                                     UnityEngine.Random.Range(heightMin, heightMax));
                    //Calculate square value for left
                    heightMap[x, midY] = (float)((heightMap[midX, midY] +
                                                     heightMap[x, y] +
                                                     heightMap[pmidXL, midY] +
                                                     heightMap[x, cornerY]) / 4.0f +
                                                     UnityEngine.Random.Range(heightMin, heightMax));
                    //Calculate square value for top
                    heightMap[midX, cornerY] = (float)((heightMap[midX, midY] +
                                                     heightMap[x, cornerY] +
                                                     heightMap[midX, pmidYU] +
                                                     heightMap[cornerX, cornerY]) / 4.0f +
                                                     UnityEngine.Random.Range(heightMin, heightMax));
                    //Calculate square value for right
                    heightMap[cornerX, midY] = (float)((heightMap[midX, midY] +
                                                     heightMap[cornerX, cornerY] +
                                                     heightMap[pmidXR, midY] +
                                                     heightMap[cornerX, y]) / 4.0f +
                                                     UnityEngine.Random.Range(heightMin, heightMax));

                }
            }

            squareSize = (int)(squareSize / 2.0f);
            heightMin *= heightDampener;
            heightMax *= heightDampener;
        }
        

        terrainData.SetHeights(0, 0, heightMap);
    }

    #region Splatmaps
    /* Custom GetSteepness Method --- Can be used in place of built in function
    float GetSteepness(float[,] heightmap, int x, int y, int width, int height)
    {
        float h = heightmap[x, y];
        int nx = x + 1;
        int ny = y + 1;

        //If on the upper edges of the map, find gradient by going backwards
        if (nx > width - 1) nx = x - 1;
        if (ny > height - 1) ny = y - 1;

        float dx = heightmap[nx, y] - h;
        float dy = heightmap[x, ny] - h;
        Vector2 gradient = new Vector2(dx, dy);

        float steep = gradient.magnitude;

        return steep;
    }
    */
    //Add textures to Splat Prototypes
    public void SplatMaps()
    {
        SplatPrototype[] newSplatPrototypes;
        newSplatPrototypes = new SplatPrototype[splatHeights.Count];
        int spindex = 0;
        foreach (SplatHeights sh in splatHeights)
        {
            newSplatPrototypes[spindex] = new SplatPrototype();
            newSplatPrototypes[spindex].texture = sh.texture;
            newSplatPrototypes[spindex].tileOffset = sh.tileOffset;
            newSplatPrototypes[spindex].tileSize = sh.tileSize;
            newSplatPrototypes[spindex].texture.Apply(true);
            spindex++;
        }
        terrainData.splatPrototypes = newSplatPrototypes; //Applies textures to Terrain's list of textures

        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);
        float[,,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];
        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                float[] splat = new float[terrainData.alphamapLayers];
                for (int i = 0; i < splatHeights.Count; i++)
                {
                    float noise = Mathf.PerlinNoise(x * splatHeights[i].splatXScale, y * splatHeights[i].splatYScale) * splatHeights[i].splatScalar;
                    float offset = splatHeights[i].splatOffset + noise;
                    float thisHeightStart = splatHeights[i].minHeight - offset;
                    float thisHeightStop = splatHeights[i].maxHeight + offset;
                    float steepness = terrainData.GetSteepness(y / (float)terrainData.alphamapHeight, x / (float)terrainData.alphamapWidth);
                        //GetSteepness(heightMap, x, y,
                        //                            terrainData.heightmapWidth, terrainData.heightmapHeight);

                    if((heightMap[x,y] >= thisHeightStart && heightMap[x,y] <= thisHeightStop) &&
                        (steepness >= splatHeights[i].minSlope && steepness <= splatHeights[i].maxSlope))
                    {
                        splat[i] = 1;
                    }
                }
                NormalizeVector(splat);
                for (int j = 0; j < splatHeights.Count; j++)
                {
                    splatmapData[x, y, j] = splat[j];
                }
            }
        }
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }
    
    void NormalizeVector(float[] v)
    {
        float total = 0;
        for (int i = 0; i < v.Length; i++)
        {
            total += v[i];
        }
        for (int i = 0; i < v.Length; i++)
        {
            v[i] /= total;
        }
    }

    //Adds another Splat Height layer
    public void AddNewSplatHeight()
    {
        splatHeights.Add(new SplatHeights());
    }

    //Removes all layers that are marked for removal
    public void RemoveSplatHeight()
    {
        List<SplatHeights> keptSplatHeights = new List<SplatHeights>();
        for (int i = 0; i < splatHeights.Count; i++)
        {
            if (!splatHeights[i].remove)
            {
                keptSplatHeights.Add(splatHeights[i]);
            }
        }
        if (keptSplatHeights.Count == 0) //If we don't want to keep any
        {
            keptSplatHeights.Add(splatHeights[0]); //We keep the first one because GUITable Layout wants at least one entry
        }
        splatHeights = keptSplatHeights;
    }

    #endregion

    #region Vegetation

    public void PlantVegetation()
    {
        //Add tree meshes to tree prototypes in Terrain object
        TreePrototype[] newTreePrototypes;
        newTreePrototypes = new TreePrototype[vegetation.Count];
        int tindex = 0;
        foreach (Vegetation t in vegetation)
        {
            newTreePrototypes[tindex] = new TreePrototype();
            newTreePrototypes[tindex].prefab = t.mesh;
            tindex++;
        }
        terrainData.treePrototypes = newTreePrototypes;

        List<TreeInstance> allVegetation = new List<TreeInstance>();
        for (int z = 0; z < terrainData.size.z; z += treeSpacing)
        {
            for (int x = 0; x < terrainData.size.x; x += treeSpacing)
            {
                for (int tp = 0; tp < terrainData.treePrototypes.Length; tp++)
                {
                    float thisHeight = terrainData.GetHeight(x, z) / terrainData.size.y;
                    float thisHeightStart = vegetation[tp].minHeight;
                    float thisHeightEnd = vegetation[tp].maxHeight;

                    if (thisHeight >= thisHeightStart && thisHeight <= thisHeightEnd)
                    {
                        TreeInstance instance = new TreeInstance();
                        instance.position = new Vector3((x + UnityEngine.Random.Range(-5.0f, 5.0f)) / terrainData.size.x, thisHeight, (z + UnityEngine.Random.Range(-5.0f, 5.0f)) / terrainData.size.z);

                        instance.rotation = UnityEngine.Random.Range(0, 360);
                        instance.prototypeIndex = tp;
                        instance.color = Color.white;
                        instance.lightmapColor = Color.white;
                        instance.heightScale = 0.95f;
                        instance.widthScale = 0.95f;

                        allVegetation.Add(instance);
                        if (allVegetation.Count >= maxTrees) goto TREESDONE;
                    }
                        
                }
            }
        }

        TREESDONE:
            terrainData.treeInstances = allVegetation.ToArray();
            
    }

    public void AddNewVegetation()
    {
        vegetation.Add(new Vegetation());
    }

    public void RemoveVegetation()
    {
        List<Vegetation> keptVegetation = new List<Vegetation>();
        for (int i = 0; i < vegetation.Count; i++)
        {
            if (!vegetation[i].remove)
            {
                keptVegetation.Add(vegetation[i]);
            }
        }
        if (keptVegetation.Count == 0) //If we don't want to keep any
        {
            keptVegetation.Add(vegetation[0]); //We keep the first one because GUITable Layout wants at least one entry
        }
        vegetation = keptVegetation;
    }

    #endregion

    #endregion
    //============================================= Initialization Functions ==============================================
    #region Initialization Functions
    void OnEnable()
    {
        Debug.Log("Initialising Terrain Data");
        terrain = this.GetComponent<Terrain>();
        //TODO: Change this to terrainData = terrain.terrainData for use with multiple terrain objects
        terrainData = Terrain.activeTerrain.terrainData;
    }

    void Awake()
    {
        //Get tag database
        SerializedObject tagManager = new SerializedObject(
                                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        //Add new tags
        AddTag(tagsProp, "Terrain");
        AddTag(tagsProp, "Cloud");
        AddTag(tagsProp, "Shore");

        //Apply tag changes to tag database
        tagManager.ApplyModifiedProperties();

        //Apply tag to this game object
        this.gameObject.tag = "Terrain";
    }

    void AddTag(SerializedProperty tagsProp, string newTag)
    {
        bool found = false;

        //ensure the tag doesn't already exist
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if(t.stringValue.Equals(newTag))
            {
                found = true;
                break;
            }
        }

        //add the new tag
        if(!found)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
            newTagProp.stringValue = newTag;
        }

    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    #endregion
}
