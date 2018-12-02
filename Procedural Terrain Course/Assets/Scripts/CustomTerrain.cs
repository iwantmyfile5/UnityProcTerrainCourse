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
    public bool resetTerrain                                                = true; // When true, all functions will overwrite existing terrain data
                                                                                    // When false, all functions will add heights to existing terrain data
    //----------- Textures ----------------------
    public Texture2D heightMapImage;

    //------------ Vectors ----------------------------
    public Vector2 randomHeightRange                                        = new Vector2(0, 0.3f);
    public Vector3 heightMapScale                                           = new Vector3(1, 1, 1);


    //------------ Perlin Noise Variables ------------------------
    public float perlinXScale                                               = 0.01f;
    public float perlinYScale                                               = 0.01f;
    public float perlinPersistance                                          = 8f;
    public float perlinHeightScale                                          = 0.09f;
    public int perlinOffsetX                                                = 0;
    public int perlinOffsetY                                                = 0;
    public int perlinOctaves                                                = 3;

    //------------ Multiple Perlin Noise Variables ------------------------
    [System.Serializable]
    public class PerlinParameters
    {
        public float mPerlinXScale                                          = 0.01f;
        public float mPerlinYScale                                          = 0.01f;
        public float mPerlinPersistance                                     = 8f;
        public float mPerlinHeightScale                                     = 0.09f;
        public int mPerlinOffsetX                                           = 0;
        public int mPerlinOffsetY                                           = 0;
        public int mPerlinOctaves                                           = 3;
        public bool remove                                                  = false;
    }

    public List<PerlinParameters> perlinParameters                          = new List<PerlinParameters>()
    {
        new PerlinParameters()
    };

    //---------------- Voronoi ---------------------
    public enum VoronoiType { Linear = 0, Power = 1, Combined = 2, PowerSin = 3 }
    public int voronoiPeaks                                                 = 5;
    public float voronoiFallOff                                             = 0.2f;
    public float voronoiDropOff                                             = 0.6f;
    public float voronoiMinHeight                                           = 0.1f;
    public float voronoiMaxHeight                                           = 0.5f;
    public VoronoiType voronoiType                                          = VoronoiType.Linear;

    //--------------- Midpoint Displacement ----------
    public float MPDheightMin                                               = -10.0f;
    public float MPDheightMax                                               = 10.0f;
    public float MPDheightDampenerPower                                     = 2.0f;
    public float MPDroughness                                               = 2.0f;
    //-------------- Smooth ------------
    public int smoothIterations                                             = 1;
    //------------- Splatmaps ---------------------
    [System.Serializable]
    public class SplatHeights
    {
        public Texture2D texture                                            = null;
        public float minHeight                                              = 0.1f;
        public float maxHeight                                              = 0.2f;
        public float minSlope                                               = 0;
        public float maxSlope                                               = 90;
        public Vector2 tileOffset                                           = new Vector2(0, 0);
        public Vector2 tileSize                                             = new Vector2(50, 50);
        public float splatXScale                                            = 0.01f;
        public float splatYScale                                            = 0.01f;
        public float splatScalar                                            = .02f;
        public float splatOffset                                            = 0.01f;
        public bool remove                                                  = false;
    }
    public List<SplatHeights> splatHeights                                  = new List<SplatHeights>()
    {
        new SplatHeights()
    };
    //----------- Trees --------------
    [System.Serializable]
    public class Vegetation
    {
        public GameObject mesh;
        public float minHeight                                              = 0.1f;
        public float maxHeight                                              = 0.2f;
        public float minSlope                                               = 0;
        public float maxSlope                                               = 90;
        public float minScale                                               = 0.5f;
        public float maxScale                                               = 1;
        public float bendFactor                                             = 0.1f;
        public Color color1                                                 = Color.white;
        public Color color2                                                 = Color.white;
        public Color lightColor                                             = Color.white;
        public float minRotation                                            = 0;
        public float maxRotation                                            = 360;
        public float density                                                = 0.5f;
        public bool remove                                                  = false;
    }
    public List<Vegetation> vegetation                                      = new List<Vegetation>()
    {
        new Vegetation()
    };

    public int maxTrees                                                     = 5000;
    public int treeSpacing                                                  = 5;

    //----------- Details -----------------
    [System.Serializable]
    public class Detail
    {
        public GameObject prototype                                         = null;
        public Texture2D prototypeTexture                                   = null;
        public float minHeight                                              = 0.1f;
        public float maxHeight                                              = 0.2f;
        public float minSlope                                               = 0;
        public float maxSlope                                               = 90;
        public float bendFactor                                             = 0.1f;
        public Color healthyColor                                           = Color.white;
        public Color dryColor                                               = Color.white;
        public Vector2 heightRange                                          = new Vector2(1, 1);
        public Vector2 widthRange                                           = new Vector2(1, 1);
        public float noiseSpread                                            = 0.5f;
        public float overlap                                                = 0.01f;
        public float feather                                                = 0.5f;
        public float density                                                = 0.5f;
        public bool remove                                                  = false;
    }
    public List<Detail> details                                             = new List<Detail>()
    {
        new Detail()
    };

    public int maxDetails                                                   = 5000;
    public int detailSpacing                                                = 5;

    //---------- Water -------------
    public float waterHeight                                                = 0.5f;
    public GameObject waterGameObject;
    public Material shoreLineMaterial;

    //--------- Erosion -----------
    public enum ErosionType {  Rain = 0, Thermal = 1, Tidal = 2, River = 3, Wind = 4, Canyon = 5 }
    public ErosionType erosionType                                          = ErosionType.Rain;
    public float erosionStrength                                            = 0.1f;
    public float erosionAmount                                              = 0.02f;
    public int springsPerRiver                                              = 5;
    public float solubility                                                 = 0.01f;
    public int droplets                                                     = 10;
    public int erosionSmoothAmount                                          = 5;

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

    //Returns a list of neighboring positions from a 2D array -- Only returns the position if it actually exists
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
        SplatPrototype[] newSplatPrototypes;                                            //Create array of splat prototypes
        newSplatPrototypes = new SplatPrototype[splatHeights.Count];                    //Set the size of the array to the size of our list of splat layers
        int spindex = 0;                                                                //Create index
        foreach (SplatHeights sh in splatHeights)                                       //Loop through the list of splat layers
        {
            //Set properties of the splat prototype to the properties set in the editor
            newSplatPrototypes[spindex]                 = new SplatPrototype();         
            newSplatPrototypes[spindex].texture         = sh.texture;                   
            newSplatPrototypes[spindex].tileOffset      = sh.tileOffset;            
            newSplatPrototypes[spindex].tileSize        = sh.tileSize;
            newSplatPrototypes[spindex].texture.Apply(true);

            spindex++;                                                                  //Increment the index
        }
        terrainData.splatPrototypes = newSplatPrototypes;                               //Apply splat prototypes to terrain data

        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);                                                     //Get the heightmap
        float[,,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];                                          //Create a splatmap
        //Loop through the alpha map
        for (int y = 0; y < terrainData.alphamapHeight; y++)                                                                                                            //Loop through alpha map y values
        {   
            for (int x = 0; x < terrainData.alphamapWidth; x++)                                                                                                         //Loop through alpha map x values
            {
                float[] splat = new float[terrainData.alphamapLayers];                                                                                                  //Create array to store the strengths of each layer at a point on the splat map
                for (int i = 0; i < splatHeights.Count; i++)                                                                                                            //Loop through all splat layers
                {
                    float noise             = Mathf.PerlinNoise(x * splatHeights[i].splatXScale, y * splatHeights[i].splatYScale) * splatHeights[i].splatScalar;        //Calculate a noise value to add some randomness
                    float offset            = splatHeights[i].splatOffset + noise;                                                                                      //Calculate the offset - this blends textures
                    float thisHeightStart   = splatHeights[i].minHeight - offset;                                                                                       //Calculate the min height for this texture
                    float thisHeightStop    = splatHeights[i].maxHeight + offset;                                                                                       //Calculate the max height for this texture
                    float steepness         = terrainData.GetSteepness(y / (float)terrainData.alphamapHeight, x / (float)terrainData.alphamapWidth);                    //Get the slope of the terrain

                    if((heightMap[x,y] >= thisHeightStart && heightMap[x,y] <= thisHeightStop) &&                                                                       //If the height of the terrain is within our min and max height
                        (steepness >= splatHeights[i].minSlope && steepness <= splatHeights[i].maxSlope))                                                               //and the slope of the terrain is within our min and max slope
                    {
                        splat[i] = 1;                                                                                                                                   //Set this texture to appear at this location
                    }
                }
                NormalizeVector(splat);                                                                                                                                 //Normalize the vector so we have a total strength value of 1 across all the splat layers
                for (int j = 0; j < splatHeights.Count; j++)                                                                                                            //Loop through all the splat layers
                {
                    splatmapData[x, y, j] = splat[j];                                                                                                                   //Apply them to the splat map
                }
            }
        }
        terrainData.SetAlphamaps(0, 0, splatmapData);                                                                                                                   //Apply the splat map to the terrain data
    }
    
    //Maps values in an array to values between 0 and 1
    void NormalizeVector(float[] v)
    {
        float total = 0;                        //Stores running total
        for (int i = 0; i < v.Length; i++)      //Loop through the array
        {
            total += v[i];                      //Add each element to the total
        }
        for (int i = 0; i < v.Length; i++)      //Loop through the array
        {
            v[i] /= total;                      //Set the element to itself / total
        }
    }

    //Adds another Splat Height layer
    public void AddNewSplatHeight()
    {
        splatHeights.Add(new SplatHeights());   //Add new splat layer
    }

    //Removes all layers that are marked for removal
    public void RemoveSplatHeight()
    {
        List<SplatHeights> keptSplatHeights = new List<SplatHeights>();     //Create a list of splat layers to keep
        for (int i = 0; i < splatHeights.Count; i++)                        //Loop through all the splat layers
        {
            if (!splatHeights[i].remove)                                    //If they aren't marked for removal
            {
                keptSplatHeights.Add(splatHeights[i]);                      //Add them to the list to keep
            }
        }
        if (keptSplatHeights.Count == 0)                                    //If we don't want to keep any
        {
            keptSplatHeights.Add(splatHeights[0]);                          //We keep the first one because GUITable Layout wants at least one entry
        }
        splatHeights = keptSplatHeights;                                    //Set the splat layers list to the list of layers to keep
    }

    #endregion

    #region Vegetation

    public void PlantVegetation()
    {
        TreePrototype[] newTreePrototypes;                              //Create an array of tree prototypes
        newTreePrototypes = new TreePrototype[vegetation.Count];        //Set the size of this array to the size of our vegetation list
        int tindex = 0;                                                 //Create index
        foreach (Vegetation t in vegetation)                            //Loop through the vegetation list
        {
            //Set properties of the tree prototype to the properties from the editor
            newTreePrototypes[tindex]               = new TreePrototype();
            newTreePrototypes[tindex].prefab        = t.mesh;
            newTreePrototypes[tindex].bendFactor    = t.bendFactor;

            tindex++;                                                   //Increment the index
        }
        terrainData.treePrototypes = newTreePrototypes;                 //Apply the tree prototypes to the terrain data

        List<TreeInstance> allVegetation = new List<TreeInstance>();                                                                                                                                                //Create a list of the vegetation
        //Loop through all positions of the terrain data
        for (int z = 0; z < terrainData.size.z; z += treeSpacing)                                                                                                                                                   //Loop through the z positions in our terrain data, incrementing by treeSpacing
        {
            for (int x = 0; x < terrainData.size.x; x += treeSpacing)                                                                                                                                               //Loop through the x positions in our terrain data, incrementing by treeSpacing
            {
                for (int tp = 0; tp < terrainData.treePrototypes.Length; tp++)                                                                                                                                      //Loop through each tree prototype
                {
                    if (UnityEngine.Random.Range(0.0f, 1.0f) > vegetation[tp].density) break;                                                                                                                       //Create random number, if its greater than the density, skip placing this tree

                    float thisHeight        = terrainData.GetHeight(x, z) / terrainData.size.y;                                                                                                                     //Get  the hieght of the terrain
                    float thisHeightStart   = vegetation[tp].minHeight;                                                                                                                                             //Get the min height for this tree
                    float thisHeightEnd     = vegetation[tp].maxHeight;                                                                                                                                             //Get the max height for this tree

                    float steepness = terrainData.GetSteepness(x / (float)terrainData.size.x, z / (float)terrainData.size.z);                                                                                       //Get the slope of the terrain

                    if (thisHeight >= thisHeightStart && thisHeight <= thisHeightEnd &&                                                                                                                             //If the height of the terrain is within our min and max height, and the slope of the terrain
                        steepness >= vegetation[tp].minSlope && steepness <= vegetation[tp].maxSlope)                                                                                                               //is within our min and max slope
                    {
                        TreeInstance instance = new TreeInstance();                                                                                                                                                 //Create a new tree instance
                        instance.position = new Vector3((x + UnityEngine.Random.Range(-5.0f, 5.0f)) / terrainData.size.x, thisHeight, (z + UnityEngine.Random.Range(-5.0f, 5.0f)) / terrainData.size.z);            //Create position for the tree, but add some randomness so trees aren't placed in grids

                        Vector3 treeWorldPos = new Vector3(instance.position.x * terrainData.size.x,                                                                                                                //Get the position of the tree in world coordinates
                                                           instance.position.y * terrainData.size.y,
                                                           instance.position.z * terrainData.size.z) + this.transform.position;

                        RaycastHit hit;                                                                                                                                                                             //Create a raycast
                        int layerMask = 1 << terrainLayer;                                                                                                                                                          //Set the lay mask for ray casting to the terrain layer
                        if(Physics.Raycast(treeWorldPos + 10 * Vector3.up, Vector3.down, out hit, 100, layerMask) ||                                                                                                //Ray cast up and down from the position of the tree to find terrain
                            Physics.Raycast(treeWorldPos + 10 * Vector3.down, Vector3.up, out hit, 100, layerMask))
                        {
                            float treeHeight        = (hit.point.y - this.transform.position.y) / terrainData.size.y;                                                                                               //Get the height of the terrain from the raycast hit
                            instance.position       = new Vector3(instance.position.x, treeHeight, instance.position.z);                                                                                            //Move the tree to this height

                            instance.rotation       = UnityEngine.Random.Range(vegetation[tp].minRotation, vegetation[tp].maxRotation);                                                                             //Set the rotation of the tree                                       
                            instance.prototypeIndex = tp;                                                                                                                                                           //Set the index of the instance to use the current tree index
                            instance.color          = Color.Lerp(vegetation[tp].color1, vegetation[tp].color2, UnityEngine.Random.Range(0.0f, 1.0f));                                                               //Set the color of the tree
                            instance.lightmapColor  = vegetation[tp].lightColor;                                                                                                                                    //Set the lightmap color of the tree, has stronger effect than just color
                            float s                 = UnityEngine.Random.Range(vegetation[tp].minScale, vegetation[tp].maxScale);                                                                                   //Create a scale of the tree
                            instance.heightScale    = s;                                                                                                                                                            //Set height scale
                            instance.widthScale     = s;                                                                                                                                                            //Set width scale

                            allVegetation.Add(instance);                                                                                                                                                            //Add the instance to the list of trees - this prevents any tree that did not have a successful raycast from being added to the map
                            if (allVegetation.Count >= maxTrees) goto TREESDONE;                                                                                                                                    //If we've reached the maximum number of trees, exit tree generation and skip to applying the trees
                        }

                        
                    }
                        
                }
            }
        }

        TREESDONE:
            terrainData.treeInstances = allVegetation.ToArray();                                                                                                                                                    //Apply the vegetation to the terrain data
            
    }

    public void AddNewVegetation()
    {
        vegetation.Add(new Vegetation());   //Add a new vegetation
    }

    public void RemoveVegetation()
    {
        List<Vegetation> keptVegetation = new List<Vegetation>();   //Create a list of vegetation we'd like to keep
        for (int i = 0; i < vegetation.Count; i++)                  //Loop through all vegetation objects
        {
            if (!vegetation[i].remove)                              //If they're not marked for removal
            {
                keptVegetation.Add(vegetation[i]);                  //Add them to the list to keep
            }
        }
        if (keptVegetation.Count == 0)                              //If we don't want to keep any
        {
            keptVegetation.Add(vegetation[0]);                      //We keep the first one because GUITable Layout wants at least one entry
        }
        vegetation = keptVegetation;                                //Set the vegetation list to the list of the ones we'd like to keep
    }

    #endregion

    #region Details

    public void PlaceDetails()
    {
        DetailPrototype[] newDetailPrototypes;                                                  //Array of detail prototypes
        newDetailPrototypes = new DetailPrototype[details.Count];                               //Set it to the size of our list of details
        int dindex = 0;                                                                         //Create an index
        foreach (Detail d in details)                                                           //Loop through the list of details
        {
            //Set the properties of each detail to the values set in the editor
            newDetailPrototypes[dindex]                     = new DetailPrototype();
            newDetailPrototypes[dindex].prototype           = d.prototype;
            newDetailPrototypes[dindex].prototypeTexture    = d.prototypeTexture;
            newDetailPrototypes[dindex].bendFactor          = d.bendFactor;
            newDetailPrototypes[dindex].healthyColor        = d.healthyColor;
            newDetailPrototypes[dindex].dryColor            = d.dryColor;
            newDetailPrototypes[dindex].noiseSpread         = d.noiseSpread;
            newDetailPrototypes[dindex].minHeight           = d.heightRange.x;
            newDetailPrototypes[dindex].maxHeight           = d.heightRange.y;
            newDetailPrototypes[dindex].minWidth            = d.widthRange.x;
            newDetailPrototypes[dindex].maxWidth            = d.widthRange.y;

            //Set properties that are different for mesh details and billboard details
            if (newDetailPrototypes[dindex].prototype)
            {
                newDetailPrototypes[dindex].usePrototypeMesh    = true;
                newDetailPrototypes[dindex].renderMode          = DetailRenderMode.VertexLit;
            }
            else
            {
                newDetailPrototypes[dindex].usePrototypeMesh    = false;
                newDetailPrototypes[dindex].renderMode          = DetailRenderMode.GrassBillboard;
            }

            dindex++;                                                                           //Increment the index
        }
        terrainData.detailPrototypes = newDetailPrototypes;                                     //Apply the details to the terrain data

        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);                     //Get the hieghtmap

        for (int i = 0; i < terrainData.detailPrototypes.Length; i++)                                                                   //Loop through all the detail prototypes
        {
            int[,] detailMap = new int[terrainData.detailWidth, terrainData.detailHeight];                                              //Create a detail map

            //Loop through the detail map
            for (int y = 0; y < terrainData.detailHeight; y += detailSpacing)                                                           //Loop through all y values of the detail map, incrementing by detail spacing
            {
                for (int x = 0; x < terrainData.detailWidth; x += detailSpacing)                                                        //Loop through all y values of the detail map, incrementing by detail spacing
                {
                    if (UnityEngine.Random.Range(0.0f, 1.0f) > details[i].density) continue;                                            //Generate random number, if its greater than the density, skip placing this detail
                    int xHM = (int)(x / (float)terrainData.detailWidth * terrainData.heightmapWidth);                                   //Find the x location on the height map
                    int yHM = (int)(y / (float)terrainData.detailHeight * terrainData.heightmapHeight);                                 //Find the y location on the height map

                    float thisNoise = Utils.Map(Mathf.PerlinNoise(x * details[i].feather,                                               //Create some noise for placing this detail
                                                                    y * details[i].feather), 0, 1, 0.5f, 1);

                    float thisHeightStart = details[i].minHeight * thisNoise - details[i].overlap * thisNoise;                          //Get the min height for this detail
                    float thisHeightEnd = details[i].maxHeight * thisNoise + details[i].overlap * thisNoise;                            //Get the max height for this detail

                    float thisHeight = heightMap[yHM, xHM];                                                                             //Get the height of the terrain
                    float steepness = terrainData.GetSteepness(xHM / (float)terrainData.size.x, yHM / (float)terrainData.size.z);       //Get the steepness of the terrain

                    if((thisHeight >= thisHeightStart && thisHeight <= thisHeightEnd) &&                                                //If the height of the terrain is within our min and max heights, and the steepness is within our min and max slope
                       (steepness >= details[i].minSlope && steepness <= details[i].maxSlope))
                    {
                        detailMap[y, x] = 1;                                                                                            //Place the detail
                    }
                }
            }
            terrainData.SetDetailLayer(0, 0, i, detailMap);                                                                             //Apply the detail map
        }
    }

    public void AddNewDetail()
    {
        details.Add(new Detail());  //Create a new detail at the end of the list
    }

    public void RemoveDetail()
    {
        List<Detail> keptDetail = new List<Detail>();   //Create a list of details we'd like to keep
        for (int i = 0; i < details.Count; i++)         //Loop through all details
        {
            if (!details[i].remove)                     //If they aren't marked for removal
            {
                keptDetail.Add(details[i]);             //Add them to list of details to keep
            }
        }
        if (keptDetail.Count == 0)                      //If we don't want to keep any
        {
            keptDetail.Add(details[0]);                 //We keep the first one because GUITable Layout wants at least one entry
        }
        details = keptDetail;                           //Set the details list to the list of details we'd like to keep
    }

    #endregion

    #region Water

    public void AddWater()
    {
        GameObject water = GameObject.Find("water");                                                                                                            //Find the water object if it has been created previously
        if(!water)                                                                                                                                              //If it hasn't been created
        {
            water = Instantiate(waterGameObject, this.transform.position, this.transform.rotation);                                                             //Create the water object
            water.name = "water";                                                                                                                               //Name it so we can find it later
        }
        water.transform.position = this.transform.position + new Vector3(terrainData.size.x / 2, waterHeight * terrainData.size.y, terrainData.size.z / 2);     //Place it in the cetner of the terrain
        water.transform.localScale = new Vector3(terrainData.size.x, 1, terrainData.size.z);                                                                    //Scale it so it covered the entire terrain and extends beyond the edges
    }

    public void DrawShoreLine()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);                                         //Get the heightmap

        //Loop through the entire heightmap
        for (int y = 0; y < terrainData.heightmapHeight; y++)                                                                                               //Loop through y positions of the heightmap
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)                                                                                            //Loop through x positions of the heightmap
            {
                //Find spot on shore
                Vector2 thisLocation = new Vector2(x, y);                                                                                                   //Store this location
                List<Vector2> neighbors = GenerateNeighbors(thisLocation, terrainData.heightmapWidth, terrainData.heightmapHeight);                         //Get the neighbors of this position
                foreach (Vector2 n in neighbors)                                                                                                            //Loop through all the neighbors
                {
                    if(heightMap[x,y] < waterHeight && heightMap[(int)n.x, (int)n.y] > waterHeight)                                                         //If we're on the shoreline
                    {

                            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);                                                                 //Create a primitive quad game object
                            go.transform.localScale *= 10.0f; //Change this to a slider                                                                     //Enlarge the object

                            go.transform.position = this.transform.position + new Vector3(y  / (float)terrainData.heightmapHeight * terrainData.size.z,     //Move the quad to the shoreline
                                                                                        waterHeight * terrainData.size.y,
                                                                                        x / (float)terrainData.heightmapWidth * terrainData.size.x);

                        go.transform.LookAt(new Vector3(n.y / (float)terrainData.heightmapHeight * terrainData.size.z,                                      //Make the quad face the shore line
                                                        waterHeight * terrainData.size.y,
                                                        n.x / (float)terrainData.heightmapWidth * terrainData.size.x));

                            go.transform.Rotate(90, 0, 0);                                                                                                  //Make the quad lay horizontal
                            go.tag = "Shore";                                                                                                               //Tag it as part of the shoreline
                        
                    }
                }
            }
        }

        GameObject[] shoreQuads = GameObject.FindGameObjectsWithTag("Shore");           //Find quads that we generated
        MeshFilter[] meshFilters = new MeshFilter[shoreQuads.Length];                   //Create an array of mesh filters for the quads
        for (int m = 0; m < shoreQuads.Length; m++)                                     //Loop through all the mesh filters
        {
            meshFilters[m] = shoreQuads[m].GetComponent<MeshFilter>();                  //Set them to each quad's mesh filter
        }
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];            //Array that's used for combining meshes
        int i = 0;                                                                      //Index for while loop
        while(i < meshFilters.Length)                                                   //Loop through mesh filters
        {
            combine[i].mesh = meshFilters[i].sharedMesh;                                //Add the mesh to the combine instance array
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;         //Add the position of the mesh to the combine instance array
            meshFilters[i].gameObject.SetActive(false);                                 //Set the game object in the mesh filter array to be inactive so it doesn't render
            i++;                                                                        //Increment index
        }

        GameObject currentShoreLine = GameObject.Find("ShoreLine");                     //Get reference to ShoreLine if it already exists
        if(currentShoreLine)                                                            //If it exists, destroy it, since we need to reform it
        {
            DestroyImmediate(currentShoreLine);
        }
        GameObject shoreLine = new GameObject();                                        //Create new shore line
        shoreLine.name = "ShoreLine";                                                   //Name it incase we need to find it later
        shoreLine.AddComponent<WaveAnimation>();                                        //Add wave animation script
        shoreLine.transform.position = this.transform.position;                         //Set position
        shoreLine.transform.rotation = this.transform.rotation;                         //Set rotation
        MeshFilter thisMF = shoreLine.AddComponent<MeshFilter>();                       //Add mesh filter & get a reference to it
        thisMF.mesh = new Mesh();                                                       //Add mesh to mesh filter
        shoreLine.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine);         //Combine all meshes

        MeshRenderer r = shoreLine.AddComponent<MeshRenderer>();                        //Add mesh renderer
        r.sharedMaterial = shoreLineMaterial;                                           //Set material

        //Clean up quads that will no longer be used
        for (int sQ = 0; sQ < shoreQuads.Length; sQ++)
        {
            DestroyImmediate(shoreQuads[sQ]);
        }

    }

    #endregion

    #region Erosion

    //Apply selected erosion & smoothing
    public void Erode()
    {
        //Check which erosion type the user wants to apply
             if (erosionType == ErosionType.Rain)
            Rain();
        else if (erosionType == ErosionType.Thermal)
            Thermal();
        else if (erosionType == ErosionType.Tidal)
            Tidal();
        else if (erosionType == ErosionType.River)
            River();
        else if (erosionType == ErosionType.Wind)
            Wind();
        else if (erosionType == ErosionType.Canyon)
            Canyon();
        
        //Apply the selected smoothness iterations
        smoothIterations = erosionSmoothAmount;
        Smooth();
        
    }

    #region Main Erosion Functions

    //Apply rain erosion
    public void Rain()
    {
        float[,] heightMap              = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);    //Get height map
        for (int i = 0; i < droplets; i++)                                                                                          //Loop through for all desired droplets
        {
            int randX                   = UnityEngine.Random.Range(0, terrainData.heightmapWidth);                                  //Select a random x position
            int randY                   = UnityEngine.Random.Range(0, terrainData.heightmapHeight);                                 //Select a random y position
            heightMap[randX, randY]     -= erosionStrength;                                                                         //Lower the random position by the erosion strength amount
        }
        terrainData.SetHeights(0, 0, heightMap);                                                                                    //Apply the erosion
    }

    //Apply thermal (landslide) erosion TODO: Rename this function to make it more accurate
    public void Thermal()
    {
        float[,] heightMap                              = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);        //Get height map

        //Loop through all positions in height map
        for (int y = 0; y < terrainData.heightmapHeight; y++)                                                                                           //Loop through all y values
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)                                                                                        //Loop through all x values
            {
                Vector2 thisLocation                    = new Vector2(x, y);                                                                            //Create Vector2 for this position
                List<Vector2> neighbors                 = GenerateNeighbors(thisLocation, terrainData.heightmapWidth, terrainData.heightmapHeight);     //Create a list of this position's neighbors

                foreach (Vector2 n in neighbors)                                                                                                        //Loop through the list of neighbors
                {
                    if(heightMap[x,y] > heightMap[(int)n.x, (int)n.y] + erosionStrength)                                                                //If current position's height is greater than the neighbor's height + erosion strength
                    {
                        float currentHeight             = heightMap[x, y];                                                                              //Create variable for current height
                        heightMap[x, y]                 -= currentHeight * erosionAmount;                                                               //Subtract current height * erosion amount from current position
                        heightMap[(int)n.x, (int)n.y]   += currentHeight * erosionAmount;                                                               //Add current height * erosion amount to neighbor's position
                    }
                }
            }
        }

        terrainData.SetHeights(0, 0, heightMap);                                                                                                        //Apply erosion
    }

    //Apply tidal erosion
    public void Tidal()
    {
        float[,] heightMap                              = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);        //Get height map

        //Loop through all positions in the hieghtmap
        for (int y = 0; y < terrainData.heightmapHeight; y++)                                                                                           //Loop through all y values
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)                                                                                        //Loop through all x values
            {
                Vector2 thisLocation                    = new Vector2(x, y);                                                                            //Store current location
                List<Vector2> neighbors                 = GenerateNeighbors(thisLocation, terrainData.heightmapWidth, terrainData.heightmapHeight);     //Create list of neighbors

                foreach (Vector2 n in neighbors)                                                                                                        //Loop through the list of neighbors
                {
                    if (heightMap[x, y] < waterHeight && heightMap[(int)n.x, (int)n.y] > waterHeight)                                                   //If the position is on the shoreline (current height is less than water height & neighbor's height is greater than water height)
                    {                                                                                                                                   //TODO: Change these to prevent weird texturing along shoreline
                        heightMap[x, y]                 = waterHeight;                                                                                  //Set current height to water height
                        heightMap[(int)n.x, (int)n.y]   = waterHeight;                                                                                  //Set neighbors height to water height
                    }
                }
            }
        }

        terrainData.SetHeights(0, 0, heightMap);                                                                                                        //Apply erosion
    }

    //Apply river erosion
    public void River()
    {
        float[,] heightMap  = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);                                                    //Set height map
        float[,] erosionMap = new float[terrainData.heightmapWidth, terrainData.heightmapHeight];                                                                       //Create an erosion map

        for (int i = 0; i < droplets; i++)                                                                                                                              //Loop through for all droplets the user wants to create
        {
            Vector2 dropletPosition = new Vector2(UnityEngine.Random.Range(0, terrainData.heightmapWidth), UnityEngine.Random.Range(0, terrainData.heightmapHeight));   //Randomly positon the droplet on the terrain
            erosionMap[(int)dropletPosition.x, (int)dropletPosition.y] = erosionStrength;                                                                               //Set the erosion map value at the droplet position to erosion strength
            for (int j = 0; j < springsPerRiver; j++)                                                                                                                   //Loop through for each river the user wants to create per droplet TODO: Fix naming on this variable to better reflect what it does
            {
                erosionMap = RunRiver(dropletPosition, heightMap, erosionMap, terrainData.heightmapWidth, terrainData.heightmapHeight);                                 //Call supporting function RunRiver to create a river for each spring
            }
        }
        //Loop through the entire heightmap
        for (int y = 0; y < terrainData.heightmapHeight; y++)                                                                                                           //Loop through all y values for the heightmap
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)                                                                                                        //Loop through all x values for the heightmap
            {
                if(erosionMap[x,y] > 0)                                                                                                                                 //If the eroosion map is has a positive value for this position
                {
                    heightMap[x, y] -= erosionMap[x, y];                                                                                                                //Subtract it from the heightmap
                }
            }
        }

        terrainData.SetHeights(0, 0, heightMap);                                                                                                                        //Apply the erosion
    }

    //Apply wind erosion
    public void Wind()
    {
        float[,] heightMap  = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);                //Create height map with size map
        int width           = terrainData.heightmapWidth;                                                                           //Create width variable for easier typing
        int height          = terrainData.heightmapHeight;                                                                          //Create height variable for easier typing

        float windDir       = 30;                                                                                                   //Set wind direction -- TODO: Change to serialized property for inspector
        float sinAngle      = -Mathf.Sin(Mathf.Deg2Rad * windDir);                                                                  //Calculate sin of wind direction
        float cosAngle      = Mathf.Cos(Mathf.Deg2Rad * windDir);                                                                   //Calculate cosin of wind direction

        for (int y = -(height - 1)*2; y <= height*2; y += 10)                                                                       //Loop through y values, these need to be larger than normal since wind is at an angle. Skip 10 units per loop. TODO: Improve efficiency here
        {
            for (int x = -(width - 1)*2; x <= width*2; x += 1)                                                                      //Loop through x values, these need to be larger than normal since wind is at an angle, could serialize the amount to skip each loop. TODO: Improve efficiency here
            {
                float thisNoise     = (float)Mathf.PerlinNoise(x * 0.06f, y * 0.06f) * 20 * erosionStrength;                        //Create noise value so we don't dig and build up in straight lines
                int nx              = (int)x;                                                                                       //Set base pile x coordinate
                int digY            = y + (int)thisNoise;                                                                           //Set base dig y coordinate
                int ny              = (int)y + 5 + (int)thisNoise;                                                                  //Set base pile y coordinate

                Vector2 digCoords   = new Vector2(x * cosAngle - digY * sinAngle, digY * cosAngle + x * sinAngle);                  //Create vector2 for digging coordinates
                Vector2 pileCoords  = new Vector2(nx * cosAngle - ny * sinAngle, ny * cosAngle + nx * sinAngle);                    //Create vector2 for pile coordinates

                if(!(digCoords.x < 0 || digCoords.x > (width - 1) || digCoords.y < 0 || digCoords.y > (height - 1) ||               //Check if the both pile coords and dig coords are actually on the heightmap
                    pileCoords.x < 0 || pileCoords.x > (width - 1) || pileCoords.y < 0 || pileCoords.y > (height - 1)))
                {
                    heightMap[(int)digCoords.x, (int)digCoords.y]   -= 0.001f;                                                      //Remove a little height from dig coords TODO: Serialize this value so it's adjustable
                    heightMap[(int)pileCoords.x, (int)pileCoords.y] += 0.001f;                                                      //Add a little height to pile coords TODO: Serialize this value so its adjustable
                }
            }
        }
        terrainData.SetHeights(0, 0, heightMap);                                                                                    //Apply the erosion
    }

    float[,] tempHeightMap;                                                                                         //Temporary heightmap used for the canyon function
    //Creates a canyon in the terrain
    public void Canyon()
    {
        float digDepth  = 0.05f;                                                                                    //How deep we should dig out the terrain
        float bankSlope = 0.001f;                                                                                   //The slope of the canyon banks
        float maxDepth   = 0;                                                                                       //The maximum depth we should dig the terrain to
        tempHeightMap   = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);    //Initialize the size of the temporary height map

        int cx = 1;                                                                                                 //Canyon starting x coordinate
        int cy = UnityEngine.Random.Range(10, terrainData.heightmapHeight - 10);                                    //Canyon starting y coordinate
        while(cy >= 0 && cy < terrainData.heightmapHeight && cx > 0 && cx < terrainData.heightmapWidth)             //Continue looping while the canyon is still 
        {
            CanyonCrawler(cx, cy, tempHeightMap[cx, cy] - digDepth, bankSlope, maxDepth);                           //Start the recursive function
            cx = cx + UnityEngine.Random.Range(1, 3);                                                               //Advance the canyon along the x axis by a small random jump
            cy = cy + UnityEngine.Random.Range(-2, 3);                                                              //Advance the canyon along the y axis by a small random jump
        }
        terrainData.SetHeights(0, 0, tempHeightMap);                                                                //Apply the erosion
    }

    #endregion Main Erosion Functions

    #region Support Functions

    //RECURSIVE FUNCTION --- Creates canyons by digging out around a given point
    void CanyonCrawler(int x, int y, float height, float slope, float maxDepth)
    {
        //EXIT STATEMENTS                                                                                           //EXIT WHEN:
        if (x < 0 || x >= terrainData.heightmapWidth) return;                                                       //Off x range of map
        if (y < 0 || y >= terrainData.heightmapHeight) return;                                                      //Off y range of map
        if (height <= maxDepth) return;                                                                             //Max depth has been reached
        if (tempHeightMap[x, y] <= height) return;                                                                  //Ran into lower terrain

        tempHeightMap[x, y] = height;                                                                               //Set temporary heightmap at this position to the given height

        //RECURSIVE CALLS
        CanyonCrawler(x + 1, y    , height + UnityEngine.Random.Range(slope, slope + 0.01f), slope, maxDepth);      
        CanyonCrawler(x - 1, y    , height + UnityEngine.Random.Range(slope, slope + 0.01f), slope, maxDepth);
        CanyonCrawler(x + 1, y + 1, height + UnityEngine.Random.Range(slope, slope + 0.01f), slope, maxDepth);
        CanyonCrawler(x - 1, y + 1, height + UnityEngine.Random.Range(slope, slope + 0.01f), slope, maxDepth);
        CanyonCrawler(x    , y - 1, height + UnityEngine.Random.Range(slope, slope + 0.01f), slope, maxDepth);
        CanyonCrawler(x    , y - 1, height + UnityEngine.Random.Range(slope, slope + 0.01f), slope, maxDepth);
    }

    //Runs a single river path
    float[,] RunRiver(Vector3 dropletPosition, float[,] heightMap, float[,] erosionMap, int width, int height)
    {
        while(erosionMap[(int)dropletPosition.x, (int)dropletPosition.y] > 0)                                                   //While our current position on the erosion map is > 0
        {
            List<Vector2> neighbors = GenerateNeighbors(dropletPosition, width, height);                                        //Get the neighboring locations
            neighbors.Shuffle();                                                                                                //Re-order them to allow for randomization of river paths. See Utils.cs for Shuffle function definition.
            bool foundLower = false;                                                                                            //Set a flag for finding lower neighbors
            foreach(Vector2 n in neighbors)                                                                                     //Loop through the list
            {
                if(heightMap[(int)n.x, (int)n.y] < heightMap[(int)dropletPosition.x, (int)dropletPosition.y])                   //If the height of the neighbor is less than our current height
                {
                    erosionMap[(int)n.x, (int)n.y] = erosionMap[(int)dropletPosition.x, (int)dropletPosition.y] - solubility;   //Set the erosion map at our neighbors position to our current position on the erosion map - the solubility
                    dropletPosition = n;                                                                                        //Set the current position to our neighbor's position
                    foundLower = true;                                                                                          //Set the flag for a lower neighbor to true
                    break;                                                                                                      //Break out of the foreach loop
                }
            }
            if(!foundLower)                                                                                                     //If we haven't found a lower neighbor
            {
                erosionMap[(int)dropletPosition.x, (int)dropletPosition.y] -= solubility;                                       //Lower our current position on the erosion map by the solubility
            }
        }
        return erosionMap;                                                                                                      //Return the erosion map
    }

    #endregion Support Functions

    #endregion Erosion

    #endregion
    //============================================= Initialization Functions ==============================================
    #region Initialization Functions

    public enum TagType { Tag = 0, Layer = 1}                                                                                               //Enum for differientiating between Tags and Layers
    [SerializeField]
    int terrainLayer = -1;                                                                                                                  //For setting the object's layer
    //The layers are used in positioning the trees on the landscape

    void OnEnable()
    {
        Debug.Log("Initialising Terrain Data");                                                                                             //Debug to show the script is active
        terrain = this.GetComponent<Terrain>();                                                                                             //Get a reference to the Terrain component
        //Change this to terrainData = terrain.terrainData for use with multiple terrain objects
        terrainData = Terrain.activeTerrain.terrainData;                                                                                    //Get a reference to the active terrain's data
    }

    void Awake()
    {
        //Get tag database
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        //Add new tags
        AddTag(tagsProp, "Terrain", TagType.Tag);
        AddTag(tagsProp, "Cloud", TagType.Tag); //Not yet implemented
        AddTag(tagsProp, "Shore", TagType.Tag);

        //Apply tag changes to tag database
        tagManager.ApplyModifiedProperties();

        //Add a layer marked "Terrain" for raycasting to place trees
        SerializedProperty layerProp = tagManager.FindProperty("layers");
        terrainLayer = AddTag(layerProp, "Terrain", TagType.Layer);
        tagManager.ApplyModifiedProperties();

        //Apply tag to this game object
        this.gameObject.tag = "Terrain";
        this.gameObject.layer = terrainLayer;
    }

    int AddTag(SerializedProperty tagsProp, string newTag, TagType tType)
    {
        bool found = false;

        for (int i = 0; i < tagsProp.arraySize; i++)                                        //Loop through all tages
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);                      //Get the current tag
            if(t.stringValue.Equals(newTag))                                                //Check if we're trying to add a tag that already exists
            {
                found = true;                                                               //If it exists, set found to true
                return i;                                                                   //Return the index of the tag that already exists
            }
        }

        if(!found && tType == TagType.Tag)                                                  //If the tag didn't already exist and it is a tag
        {
            tagsProp.InsertArrayElementAtIndex(0);                                          //Insert it into the array of tags
            SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);             //Link newTagProp to the newly inserted element
            newTagProp.stringValue = newTag;                                                //Set newTagProp's string value to the tag we're trying to add, this will update the new element in the tags array since they're serialized
        }
        else if (!found && tType == TagType.Layer)                                          //If the tag didn't already exist and it is a layer
        {
            for (int j = 8; j < tagsProp.arraySize; j++) //User layers start at 8             Loop through the user created layers
            {
                SerializedProperty newLayer = tagsProp.GetArrayElementAtIndex(j);           //Link newLayer with the array element at this index
                if(newLayer.stringValue == "")                                              //If the index is empty
                {
                    newLayer.stringValue = newTag;                                          //Set the string value to the layer we're trying to add, which updates the element in the array
                    return j;                                                               //Return the index of the layer we created
                }
            }
        }
        return -1;

    }
    
    #endregion
}
