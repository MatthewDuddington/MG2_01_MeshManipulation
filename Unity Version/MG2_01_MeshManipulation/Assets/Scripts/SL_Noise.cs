// Base code written while following tutorial: Procedural Landmass Generation (Unity 5) (c) Sebastian Lague 2016
// Comments and supplimentory code (c) Matthew Duddington 2017 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SL_Noise {

  public enum NormaliseMode { Local, Global };  // Choice of using precice local normalised height values but with chunk edge discrepencies or estimated global values with edge agreement

  public static float[,] GenerateNoiseMap (int mapWidth,    
                                           int mapHeight,
                                           int seed,           // Seed for the PRNG
                                           float scale,        // For zooming the resulting noise
                                           int octaves,        // Number of iterations of detail that are combined witin the noise
                                           float persistance,  // How quickly the influence of each subsiquent octave decreases (how 'strongly' each subsiquent octave of features transfer into the shape of the map)
                                           float lacunarity,   // Increase of frequency scale between octaves (how 'sharp' the details of each subsiquent octave become in comparison to the previous)
                                           Vector2 offset,     // For offsetting the resulting map along the X or Z axis
                                           NormaliseMode normalizeMode)  // Option to change how the max and min values used to normalise are determined
  {
    float[,] noiseMap = new float[mapWidth, mapHeight];

    System.Random psuRandomGen = new System.Random (seed);
    Vector2[] octaveOffsets = new Vector2[octaves];

    float maxPossibleHeight = 0;
    float amplitude = 1;  // y-axis scalar, defines the scale of the octave
    float frequency = 1;  // x-axis scalar, defines the detail of the octave

    // Loop through each octave...
    for (int i = 0; i < octaves; i++) {
      // Generate a pair of pseudo random number values such that each octave will be sampled from a different location within the noise function,
      // enable custom in-editor offset by adding additional offset
      float offsetX = psuRandomGen.Next (-100000, 100000) + offset.x;  
      float offsetY = psuRandomGen.Next (-100000, 100000) - offset.y;
      octaveOffsets [i] = new Vector2(offsetX, offsetY);

      maxPossibleHeight += amplitude;  // Initially assume the max height to be the sum of the maximum amplitude of each octave
      amplitude *= persistance;  // Scale the max amplitude for each subsiquent octave by the persistance value
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

        // Loop through each octave...
        for (int i = 0; i < octaves; i++) {
          // Set a sample position for each octave,
          // offset to a different sample area for each octave,
          // tightened by the frequency value,
          // scaled by dividing result,
          // centred within the chunk (for purposes of scaling) with subtracting half sizes
          float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
          float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;

          float perlinValue = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1;  // Perlin returns 0 to 1 by defult, but more interesting noise can come from -1 to 1, so (*2 - 1) to achieve this
          noiseHeight += perlinValue * amplitude;  // scale the Perlin value to the octives amplitude range

          amplitude *= persistance;  // persistance 'strength' modulates the change in amplitude on each subsiquent octave's loop
          frequency *= lacunarity;  // lacunarity 'sharpness' modulates the change in frequency applied on each subsiquent octave's loop
        }

        // Keep track of the highest and lowest values within this map to enable normalisation before return
        if (noiseHeight > maxLocalNoiseHeight) { maxLocalNoiseHeight = noiseHeight; }
        else if (noiseHeight < minLocalNoiseHeight) { minLocalNoiseHeight = noiseHeight; }

        noiseMap [x, y] = noiseHeight;  // Store the (not yet normalised) value
      }
    }

    // Loop through the map values and normalise the resulting values to return them to a 0 to 1 range
    for (int y = 0; y < mapHeight; y++) {
      for (int x = 0; x < mapWidth; x++) {
        // If working with a descrete map generated all at the same time then min and max heights can be known precicely
        if (normalizeMode == NormaliseMode.Local) {
          noiseMap [x, y] = Mathf.InverseLerp (minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap [x, y]);  // Interpolate each value so that it is relocated proportinally in comparison to the max and min values
        } 
        // However with an endless terrain, these min and max must be estimated
        else {
          float normalisedHeight = (noiseMap [x, y] + 1) / maxPossibleHeight;  // Estimate the normalised max height
          noiseMap [x, y] = Mathf.Clamp(normalisedHeight, 0, int.MaxValue);  // Ensure values will be at least zero and chop down any values that stray above the estimated height
        }
      }
    }

    return noiseMap;
  }

}
