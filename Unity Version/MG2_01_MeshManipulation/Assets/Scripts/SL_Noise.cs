// Base code written while following tutorial: Procedural Landmass Generation (Unity 5) (c) Sebastian Lague 2016
// Comments and supplimentory code (c) Matthew Duddington 2017 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SL_Noise {

  public enum NormaliseMode { Local, Global };

  public static float[,] GenerateNoiseMap (int mapWidth,    
                                           int mapHeight,
                                           int seed,           // Seed for the PRNG
                                           float scale,        // For zooming the resulting noise
                                           int octaves,        // Number of iterations of detail that are combined witin the noise
                                           float persistance,  // How quickly the influence of each subsiquent octive decreases (how strongly the 'small features' persist into the shape of the map)
                                           float lacunarity,   // Increase of frequency scale between octives (number of 'small features' in the map)
                                           Vector2 offset,     // For offsetting the resulting map along the X or Z axis
                                           NormaliseMode normalizeMode)  // 
  {
    float[,] noiseMap = new float[mapWidth, mapHeight];

    System.Random psuRandomGen = new System.Random (seed);
    Vector2[] octaveOffsets = new Vector2[octaves];

    float maxPossibleHeight = 0;
    float amplitude = 1;  // y-axis scalar
    float frequency = 1;  // x-axis scalar

    for (int i = 0; i < octaves; i++) {
      float offsetX = psuRandomGen.Next (-100000, 100000) + offset.x;
      float offsetY = psuRandomGen.Next (-100000, 100000) - offset.y;
      octaveOffsets [i] = new Vector2(offsetX, offsetY);

      maxPossibleHeight += amplitude;
      amplitude *= persistance;
    }

    // Prevent division by 0
    if (scale <= 0) { scale = 0.0001f; }

    float maxLocalNoiseHeight = float.MinValue;
    float minLocalNoiseHeight = float.MaxValue;

    float halfWidth = mapWidth * 0.5f;
    float halfHeight = mapHeight * 0.5f;

    for (int y = 0; y < mapHeight; y++) {
      for (int x = 0; x < mapWidth; x++) {

        amplitude = 1;
        frequency = 1;
        float noiseHeight = 0;

        for (int i = 0; i < octaves; i++) {
          float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
          float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;

          float perlinValue = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1;
          noiseHeight += perlinValue * amplitude;

          amplitude *= persistance;
          frequency *= lacunarity;
        }

        if (noiseHeight > maxLocalNoiseHeight) { maxLocalNoiseHeight = noiseHeight; }
        else if (noiseHeight < minLocalNoiseHeight) { minLocalNoiseHeight = noiseHeight; }

        noiseMap [x, y] = noiseHeight;
      }
    }

    for (int y = 0; y < mapHeight; y++) {
      for (int x = 0; x < mapWidth; x++) {
        // If working with a descrete map generated all at the same time then min and max heights can be known imediatly and utilised fully...
        if (normalizeMode == NormaliseMode.Local) {
          noiseMap [x, y] = Mathf.InverseLerp (minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap [x, y]);
        } 
        // ...However with an endless terrain, these min and max must be estimated
        else {
          float normalisedHeight = (noiseMap [x, y] + 1) / maxPossibleHeight;
          noiseMap [x, y] = Mathf.Clamp(normalisedHeight, 0, int.MaxValue);  // Assume values will be at least zero
        }
      }
    }

    return noiseMap;
  }

}
