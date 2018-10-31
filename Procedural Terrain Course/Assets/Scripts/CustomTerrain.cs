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
    public int numPeaks = 5;
    public float falloff = 2f;
    public float dropOff = 0.6f;
    public float minHeight = 0.3f;
    public float maxHeight = 0.6f;

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
        Debug.Log(numPeaks);
        for (int i = 0; i < numPeaks; i++)
        {
            
            Vector3 peak = new Vector3(UnityEngine.Random.Range(0, terrainData.heightmapWidth),
                                       UnityEngine.Random.Range(minHeight, maxHeight),
                                       UnityEngine.Random.Range(0, terrainData.heightmapHeight)
                                       );
            heightmap[(int)peak.x, (int)peak.z] = peak.y;
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
                        float h = peak.y - distanceToPeak * falloff - Mathf.Pow(distanceToPeak, dropOff);
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
