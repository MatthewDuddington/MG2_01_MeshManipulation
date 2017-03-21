// Base code written while following tutorial: Procedural Landmass Generation (Unity 5) (c) Sebastian Lague 2016
// Comments and supplimentory code (c) Matthew Duddington 2017 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {

  public enum DrawMode { NoiseMap, ColourMap, Mesh };
  public DrawMode drawMode;

  // Size of mesh map chunks
  // Unity limits mesh vertices to 65,025 (255 * 255)
  // 241 because we use width - 1 and 240 has lots of factors to use with Level of Detail divisions
  public const int mapChunkSize = 241;
  [Range(0,6)] public int levelOfDetail;
  public float noiseScale;

  public int octaves;
  [Range(0,1)] public float persistance;
  public float lacunarity;

  public int seed;        // Random number seed - to enable regeneration of specific maps
  public Vector2 offset;

  public float meshHeightMultiplyer;  // Scalar to controll exageration of terrain height
  public AnimationCurve meshHeightCurve;  // Curve to control how much multiplyer effects different regions

  public bool autoUpdate;

  public TerrainType[] regions;

  public void GenerateMap() {
    float[,] noiseMap = Noise.GenerateNoiseMap (mapChunkSize,
                                                mapChunkSize,
                                                seed,
                                                noiseScale,
                                                octaves,
                                                persistance,
                                                lacunarity,
                                                offset
                                               );
    Color[] colourMap = new Color[mapChunkSize * mapChunkSize];

    for (int y = 0; y < mapChunkSize; y++) {
      for (int x = 0; x < mapChunkSize; x++) {
        float currentHeight = noiseMap [x, y];
        for (int i = 0; i < regions.Length; i++) {
          if (currentHeight <= regions [i].height) {
            colourMap [y * mapChunkSize + x] = regions [i].colour;

            break;
          }
        }
      }
    }

    // Switch for different rendering modes in the editor
    MapDisplay display = FindObjectOfType<MapDisplay> ();
    switch (drawMode) {
    case DrawMode.NoiseMap:  // Flat greyscale noise map
      display.DrawTexture (TextureGenerator.TextureFromHeightMap (noiseMap));
      break;
    case DrawMode.ColourMap:  // Flat coloured regions map
      display.DrawTexture (TextureGenerator.TextureFromColourMap (colourMap, mapChunkSize, mapChunkSize));
      break;
    case DrawMode.Mesh:  // Textured hight map effected mesh
      display.DrawMesh (MeshGenerator.GenerateTerrainMesh (noiseMap, meshHeightMultiplyer, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColourMap (colourMap, mapChunkSize, mapChunkSize));
      break;
    default:
      Debug.LogWarning ("No draw mode selected");
      break;
    }
  }

  void OnValidate() {
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