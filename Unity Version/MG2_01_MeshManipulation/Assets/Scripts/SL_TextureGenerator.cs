﻿// Base code written while following tutorial: Procedural Landmass Generation (Unity 5) (c) Sebastian Lague 2016
// Comments and supplimentory code (c) Matthew Duddington 2017 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SL_TextureGenerator {

  public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height) {
    Texture2D texture = new Texture2D (width, height);
    texture.filterMode = FilterMode.Point;
    texture.wrapMode = TextureWrapMode.Clamp;
    texture.SetPixels (colourMap);
    texture.Apply ();
    return texture;
  }

  public static Texture2D TextureFromHeightMap(float[,] heightMap) {
    int width = heightMap.GetLength (0);
    int height = heightMap.GetLength (1);

    Color[] colourMap = new Color[width * height];
    for (int y = 0; y < height; y++) {
      for (int x = 0; x < width; x++) {
        colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
      }
    }

    return TextureFromColourMap (colourMap, width, height);
  }

}
