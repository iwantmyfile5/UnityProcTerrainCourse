﻿using UnityEngine;
using UnityEditor;
using System.IO;

public class TextureCreatorWindow : EditorWindow {

    string filename = "myProceduralTexture";
    float perlinXScale = 0.001f;
    float perlinYScale = 0.001f;
    int perlinOctaves = 5;
    float perlinPersistence = 2.0f;
    float perlinHeightScale = 0.95f;
    int perlinOffsetX;
    int perlinOffsetY;
    bool alphaToggle = false;
    bool seamlessToggle = false;
    bool mapToggle = false;

    Texture2D pTexture;


	[MenuItem("Window/TextureCreatorWindow")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(TextureCreatorWindow));

    }

    private void OnEnable()
    {
        pTexture = new Texture2D(513, 513, TextureFormat.ARGB32, false);
    }

    private void OnGUI()
    {
        GUILayout.Label("Settings", EditorStyles.boldLabel);
        filename = EditorGUILayout.TextField("Texture Name", filename);

        int wSize = (int)(EditorGUIUtility.currentViewWidth - 100);

        perlinXScale        = EditorGUILayout.Slider("X Scale", perlinXScale, 0, 0.1f);
        perlinYScale        = EditorGUILayout.Slider("Y Scale", perlinYScale, 0, 0.1f);
        perlinOctaves       = EditorGUILayout.IntSlider("Octaves", perlinOctaves, 1, 10);
        perlinPersistence   = EditorGUILayout.Slider("Persistence", perlinPersistence, 1, 10);
        perlinHeightScale   = EditorGUILayout.Slider("Height Scale", perlinHeightScale, 0, 1);
        perlinOffsetX       = EditorGUILayout.IntSlider("X Offset", perlinOffsetX, 0, 10000);
        perlinOffsetY       = EditorGUILayout.IntSlider("Y Offset", perlinOffsetY, 0, 10000);
        alphaToggle         = EditorGUILayout.Toggle("Alpha?", alphaToggle);
        seamlessToggle      = EditorGUILayout.Toggle("Seamless?", seamlessToggle);
        mapToggle           = EditorGUILayout.Toggle("Map?", mapToggle);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if(GUILayout.Button("Generate", GUILayout.Width(wSize)))
        {
            int w = 513;
            int h = 513;
            float pValue;
            Color pixCol = Color.white;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if(seamlessToggle)
                    {
                        float u = (float)x / (float)w;
                        float v = (float)y / (float)h;
                        float noise00 = Utils.fBM((x + perlinOffsetX) * perlinXScale, (y + perlinOffsetY) * perlinYScale,
                                                                perlinOctaves, perlinPersistence) * perlinHeightScale;
                        float noise01 = Utils.fBM((x + perlinOffsetX) * perlinXScale, (y + perlinOffsetY + h) * perlinYScale,
                                                                perlinOctaves, perlinPersistence) * perlinHeightScale;
                        float noise10 = Utils.fBM((x + perlinOffsetX + w) * perlinXScale, (y + perlinOffsetY) * perlinYScale,
                                                                perlinOctaves, perlinPersistence) * perlinHeightScale;
                        float noise11 = Utils.fBM((x + perlinOffsetX + w) * perlinXScale, (y + perlinOffsetY + h) * perlinYScale,
                                                                perlinOctaves, perlinPersistence) * perlinHeightScale;

                        float noiseTotal = u * v * noise00 +
                                            u * (1 - v) * noise01 +
                                            (1 - u) * v * noise10 +
                                            (1 - u) * (1 - v) * (noise11);
                        float value = (int)(256 * noiseTotal) + 50;
                        float r = Mathf.Clamp((int)noise00, 0, 255);
                        float g = Mathf.Clamp(value, 0, 255);
                        float b = Mathf.Clamp(value + 50, 0, 255);
                        //float a = Mathf.Clamp(value + 100, 0, 255);

                        pValue = (r + g + b) / (3 * 255.0f);
                    }
                    else
                    {
                        pValue = Utils.fBM((x + perlinOffsetX) * perlinXScale, (y + perlinOffsetY) * perlinYScale,
                                                                perlinOctaves, perlinPersistence) * perlinHeightScale;
                    }
                    
                    float colValue = pValue;
                    pixCol = new Color(colValue, colValue, colValue, alphaToggle ? colValue : 1);
                    pTexture.SetPixel(x, y, pixCol);
                }
            }
            pTexture.Apply(false, false);
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(pTexture, GUILayout.Width(wSize), GUILayout.Height(wSize));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if(GUILayout.Button("Save", GUILayout.Width(wSize)))
        {

        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

    }
}
