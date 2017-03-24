// Base code written while following tutorial: Procedural Landmass Generation (Unity 5) (c) Sebastian Lague 2016
// Comments and supplimentory code (c) Matthew Duddington 2017

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Produces an edge mask that can be combined with the generated noise to smmothly remove structures at the edges of terrain chunks
public static class SL_FallOffGenerator {

  public static float[,] GenerateFallOffMap(int size) {
    float[,] map = new float[size, size];  // 2D array to hold the mask

    // Take the coordinates of each point within the map and assign them coordinate values from -1 to 1
    // where -1 and 1 will be at the edges and 0 in the centre
    for (int i = 0; i < size; i++) {
      for (int j = 0; j < size; j++) {
        // n divided by size will give a range between 0 to 1
        // so multiply by 2 to get 0 to 2
        // and then subtract 1 to get -1 to 1
        float x = i / (float)size * 2 - 1;
        float y = j / (float)size * 2 - 1;

        // Take the absolute values to create symetry and maximum of the two for priority
        float value = Mathf.Max (Mathf.Abs (x), Mathf.Abs (y));
        // Set the value of that coordinate point to an influenced proportion of the max value above
        map [i, j] = Evaluate(value);
      }
    }

    return map;
  }

  // Applies a smoothing curve to the value that concentrates the falloff to the edges, leaving the central areas mostly uneffected
  // x^a / (x^a + (b - b * x)^a)
  // https://www.desmos.com/calculator/c3aktrmdds
  static float Evaluate(float value) {
    float a = 3;
    float b = 2.2f;

    return Mathf.Pow (value, a) / (Mathf.Pow(value,a) + Mathf.Pow(b-b*value, a));
  }
}
