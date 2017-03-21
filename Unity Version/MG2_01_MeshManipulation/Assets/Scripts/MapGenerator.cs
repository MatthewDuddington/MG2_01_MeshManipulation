﻿// Base code written while following tutorial: Procedural Landmass Generation (Unity 5) (c) Sebastian Lague 2016
// Comments and supplimentory code (c) Matthew Duddington 2017 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {

  public enum DrawMode { NoiseMap, ColourMap, Mesh };
  public DrawMode drawMode;

  public int mapWidth;
  public int mapHeight;
  public float noiseScale;

  public int octaves;
  [Range(0,1)] public float persistance;
  public float lacunarity;

  public int seed;        // Random number seed - to enable regeneration of specific maps
  public Vector2 offset;

  public bool autoUpdate;

  public TerrainType[] regions;

  public void GenerateMap() {
    float[,] noiseMap = Noise.GenerateNoiseMap (mapWidth,
                                                mapHeight,
                                                seed,
                                                noiseScale,
                                                octaves,
                                                persistance,
                                                lacunarity,
                                                offset
                                               );
    Color[] colourMap = new Color[mapWidth * mapHeight];

    for (int y = 0; y < mapHeight; y++) {
      for (int x = 0; x < mapWidth; x++) {
        float currentHeight = noiseMap [x, y];
        for (int i = 0; i < regions.Length; i++) {
          if (currentHeight <= regions [i].height) {
            colourMap [y * mapWidth + x] = regions [i].colour;

            break;
          }
        }
      }
    }

    MapDisplay display = FindObjectOfType<MapDisplay> ();
    switch (drawMode) {
    case DrawMode.NoiseMap:
      display.DrawTexture (TextureGenerator.TextureFromHeightMap (noiseMap));
      break;
    case DrawMode.ColourMap:
      display.DrawTexture (TextureGenerator.TextureFromColourMap (colourMap, mapWidth, mapHeight));
      break;
    case DrawMode.Mesh:
      display.DrawMesh (MeshGenerator.GenerateTerrainMesh (noiseMap), TextureGenerator.TextureFromColourMap (colourMap, mapWidth, mapHeight));
      break;
    default:
      Debug.LogWarning ("No draw mode selected");
      break;
    }
  }

  void OnValidate() {
    if (mapWidth < 1) {
      mapWidth = 1;
    }
    if (mapHeight < 1) {
      mapHeight = 1;
    }
    if (lacunarity < 1) {
      lacunarity = 1;
    }
    if (octaves < 0) {
      octaves = 0;
    }
  }

}

[System.Serializable]
public struct TerrainType {
  public string name;
  public float height;
  public Color colour;
}