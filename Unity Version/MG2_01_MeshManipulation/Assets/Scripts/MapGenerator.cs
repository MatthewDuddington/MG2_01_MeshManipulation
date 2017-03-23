﻿// Base code written while following tutorial: Procedural Landmass Generation (Unity 5) (c) Sebastian Lague 2016
// Comments and supplimentory code (c) Matthew Duddington 2017 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class MapGenerator : MonoBehaviour {

  public enum DrawMode { NoiseMap, ColourMap, Mesh, FallOffMap };  // Method by which the generator should output / update inside the editor
  public DrawMode drawMode;

  public Noise.NormaliseMode normaliseMode;

  // Size of mesh map chunks
  // Unity limits mesh vertices to 65,025 (255 * 255)
  // 241 because we use width - 1 and 240 has lots of factors to use with Level of Detail divisions
  public const int mapChunkSize = 241;

  // The complexity of the mesh to be generated - passes a certain subset of the generated data points through when creating the mesh
  // N.B. these have to be factors of the 'mapChunkSize width - 1' as above i.e. 240 in this case
  // 0 = all verticies,    1 = every 2nd vertex,  2 = every 4th vertex, 3 = every 6th vertex,
  // 4 = every 8th vertex, 5 = every 10th vertex, 6 = every 12th vertex
  [Range(0,6)] public int editorPreviewLevelOfDetail;

  public int octaves;
  [Range(0,1)] public float persistance;
  public float lacunarity;
  public float noiseScale;  // Level of 'zoom' on the noise texture generated

  public int seed;  // Random number seed - to enable regeneration of specific maps
  public Vector2 offset;

  public bool shouldUseFalloff;

  public float meshHeightMultiplyer;  // Scalar to controll exageration of terrain height
  public AnimationCurve meshHeightCurve;  // Curve to control how much multiplyer effects different regions

  public bool autoUpdate;

  public TerrainType[] regions;  // Set of different terrain type information (e.g. colour) and the properties (e.g. height value %) at which it applies

  float[,] fallOffMap;

  // Queues of map generation data returned from threads to be processed on the main thread as they are filled
  Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
  Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

  void Awake() {
    fallOffMap = FallOffGenerator.GenerateFallOffMap (mapChunkSize);
  }

  // Create the position and texture data for a map chunk
  MapData GenerateMapData(Vector2 centre) {
    float[,] noiseMap = Noise.GenerateNoiseMap (mapChunkSize,
                                                mapChunkSize,
                                                seed,
                                                noiseScale,
                                                octaves,
                                                persistance,
                                                lacunarity,
                                                centre + offset,
                                                normaliseMode);
    Color[] colourMap = new Color[mapChunkSize * mapChunkSize];  // Set of pixel colours, to pass on to the texture

    // Loop through each future vertex data point
    for (int y = 0; y < mapChunkSize; y++) {
      for (int x = 0; x < mapChunkSize; x++) {
        if (shouldUseFalloff) {
          noiseMap [x, y] = Mathf.Clamp01(noiseMap [x, y] - fallOffMap [x, y]);
        }

        // Determine the height from the grey value at the corisponding location in the voise map
        float currentHeight = noiseMap [x, y];

        for (int i = 0; i < regions.Length; i++) {
          if (currentHeight >= regions [i].height) {
            colourMap [y * mapChunkSize + x] = regions [i].colour;
          } else {
            break;
          }
        }

      }
    }

    return new MapData (noiseMap, colourMap);
  }

  // Starts up a new map data processing thread and promises to callback to a function (Endless Terrain's OnMapDataRecieved) once it is complete
  public void RequestMapData(Vector2 centre, Action<MapData> callback) {
    ThreadStart threadStart = delegate {
      MapDataThread (centre, callback);  // Pass the callback into the thread
    };

    new Thread (threadStart).Start ();
  }

  // The multi-thread code for generating each chunk's data individually
  void MapDataThread(Vector2 centre, Action<MapData> callback) {
    MapData mapData = GenerateMapData (centre);

    // Lock the thread info queue so that multiple threads cant try to write data at the same time
    lock (mapDataThreadInfoQueue) {
      // Writes the resulting data into a queue so that Unity can process all the mesh handling on the main thread (a limitation of Unity's multi-threading)
      mapDataThreadInfoQueue.Enqueue (new MapThreadInfo<MapData> (callback, mapData));
    }
  }

  public void RequestMeshData(MapData mapData, int levelOfDetail, Action<MeshData> callback) {
    ThreadStart threadStart = delegate {
      MeshDataThread (mapData, levelOfDetail, callback);  // Pass the callback into the thread
    };

    new Thread (threadStart).Start ();
  }

  void MeshDataThread(MapData mapData, int levelOfDetail, Action<MeshData> callback) {
    MeshData meshData = MeshGenerator.GenerateTerrainMesh (mapData.heightMap, meshHeightMultiplyer, meshHeightCurve, levelOfDetail);

    // Lock the thread info queue so that multiple threads cant try to write data at the same time
    lock (meshDataThreadInfoQueue) {
      // Add the data created to the process queue for handling on the main thread, passing along the callback function and the data created here
      meshDataThreadInfoQueue.Enqueue (new MapThreadInfo<MeshData> (callback, meshData));
    }
  }

  void Update() {
    // If there is map data in the queue...
    if (mapDataThreadInfoQueue.Count > 0) {
      for (int i = 0; i < mapDataThreadInfoQueue.Count; i++) {
        // Take each elements out of the queue one by one 
        MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue ();
        threadInfo.callback (threadInfo.parameter);  // Now that we are outside the thread in the main Unity thread, callback the stored function and pass it the dequeued map data
      }
    }

    if (meshDataThreadInfoQueue.Count > 0) {
      for (int i = 0; i < meshDataThreadInfoQueue.Count; i++) {
        MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue ();
        threadInfo.callback (threadInfo.parameter);  // In main Unity thread, callback the stored function and pass the thread's computed mesh data  
      }
    }
  }

  public void DrawMapInEditor() {
    MapData mapData = GenerateMapData (Vector2.zero);

    // Switch for different rendering modes in the editor
    MapDisplay display = FindObjectOfType<MapDisplay> ();
    switch (drawMode) {
    case DrawMode.NoiseMap:  // Flat greyscale noise map
      display.DrawTexture (TextureGenerator.TextureFromHeightMap (mapData.heightMap));
      break;
    case DrawMode.ColourMap:  // Flat coloured regions map
      display.DrawTexture (TextureGenerator.TextureFromColourMap (mapData.colourMap, mapChunkSize, mapChunkSize));
      break;
    case DrawMode.Mesh:  // Textured hight map effected mesh
      display.DrawMesh (MeshGenerator.GenerateTerrainMesh (mapData.heightMap, meshHeightMultiplyer, meshHeightCurve, editorPreviewLevelOfDetail), TextureGenerator.TextureFromColourMap (mapData.colourMap, mapChunkSize, mapChunkSize));
      break;
    case DrawMode.FallOffMap:
      display.DrawTexture (TextureGenerator.TextureFromHeightMap (FallOffGenerator.GenerateFallOffMap (mapChunkSize)));
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

    fallOffMap = FallOffGenerator.GenerateFallOffMap (mapChunkSize);  // Ensure the falloff map is initialised
  }

  struct MapThreadInfo<T> {
    public readonly Action<T> callback;  // Reference to the function to call after the thread has finished processing in a later frame
    public readonly T parameter;  // Data being stored for use in main thread function (map / mesh data)

    public MapThreadInfo (Action<T> callback, T parameter)
    {
      this.callback = callback;
      this.parameter = parameter;
    }
  }

}

[System.Serializable]
public struct TerrainType {
  public string name;
  public float height;
  public Color colour;
}

public struct MapData {
  public readonly float[,] heightMap;
  public readonly Color[] colourMap;

  public MapData (float[,] heightMap, Color[] colourMap)
  {
    this.heightMap = heightMap;
    this.colourMap = colourMap;
  }
}
