// Base code written while following tutorial: Procedural Landmass Generation (Unity 5) (c) Sebastian Lague 2016
// Comments and supplimentory code (c) Matthew Duddington 2017 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SL_TextureGenerator {

  // Turns the provided colour map into a Unity texture that can be appled and displayed on a mesh
  public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height) {
    Texture2D texture = new Texture2D (width, height);

    texture.filterMode = FilterMode.Point;     // Texure pixels are not blended but retain their sharpness at all zoom levels
    texture.wrapMode = TextureWrapMode.Clamp;  // Clamps the texture to the boarders rather than repeating

    texture.SetPixels (colourMap);  // Fill the texture sequentially with  the colour map pixels
    texture.Apply ();               // Apply the set pixels to the texture

    return texture;
  }

  // Creates a greyscale colour map from a given heightmap so that it can be passed to the above TextureFromColourMap function and displayed on the editor preview plane
  public static Texture2D TextureFromHeightMap(float[,] heightMap) {
    int width = heightMap.GetLength (0);
    int height = heightMap.GetLength (1);

    Color[] colourMap = new Color[width * height];
    for (int y = 0; y < height; y++) {
      for (int x = 0; x < width; x++) {
        colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);  // Defines a colour inbetween pure black and white by using the height map value as t
      }
    }

    return TextureFromColourMap (colourMap, width, height);
  }

}
