using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]

public class CustomTerrain : MonoBehaviour {

    public Texture2D heightMapImage;

    public Vector2 randomHeightRange = new Vector2(0, 0.1f);

    public Vector3 heightMapScale = new Vector3(1, 1, 1);

    public Terrain terrain;

    public TerrainData terrainData;

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
        float[,] heightMap = terrainData.GetHeights(0, 0,
                                terrainData.heightmapWidth,
                                terrainData.heightmapHeight);
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
        float[,] heightMap;
        heightMap = new float[terrainData.heightmapWidth, terrainData.heightmapHeight];
        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                heightMap[x, y] = heightMapImage.GetPixel((int)(x * heightMapScale.x),
                                                          (int)(y * heightMapScale.z)).grayscale
                                                          * heightMapScale.y;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }


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
}
