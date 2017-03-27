// (c) Matthew Duddington 2017 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageMapGenerator : MonoBehaviour {

  public Texture2D image;  // The image to be mapped from

  public float[,] GenerateImageMap ()
  {
    float[,] imageMap = new float[image.width, image.height];  // Store the greyscale values of the image here

    Color[] imagePixels = image.GetPixels();  // Retreave the colours of each pixel of the image

    int imageHeight = image.height;
    int imageWidth = image.width;

    float minGreyValue = 1;
    float maxGreyValue = 0;

    // For each pixel, store the greyscale value within the image map
    for (int y = 0; y < imageHeight; y++) {
      for (int x = 0; x < imageWidth; x++) {
        float greyValue = imagePixels [(imageWidth * y) + x].grayscale;
        imageMap [imageWidth - 1 - x, imageHeight - 1 - y] = greyValue;  // Reverse the pixels so that the texure appears the right way around

        // Keep track of the highest and lowest values within this map to enable redistribution before return
        if (greyValue > maxGreyValue) { maxGreyValue = greyValue; }
        else if (greyValue < minGreyValue) { minGreyValue = greyValue; }
      }
    }

    // Loop through the map values and redistribute the resulting values to make full use of the available range
    for (int y = 0; y < image.height; y++) {
      for (int x = 0; x < image.width; x++) {
        imageMap [x, y] = Mathf.InverseLerp (minGreyValue, maxGreyValue, imageMap [x, y]);  // Interpolate each value so that it is relocated proportinally in comparison to the max and min values
      }
    }

    return imageMap;
  }

}
