// Base code written while following tutorial: Procedural Landmass Generation (Unity 5) (c) Sebastian Lague 2016
// Comments and supplimentory code (c) Matthew Duddington 2017 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SL_MeshGenerator {

  public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplyer, AnimationCurve _heightCurve, int levelOfDetail) {
    // Determine size of map mesh based on the heightmap
    int width = heightMap.GetLength (0);
    int height = heightMap.GetLength (1);

    AnimationCurve heightCurve = new AnimationCurve (_heightCurve.keys);

    // Centre map by defining half map size, offset position of top left vertex
    float topLeftX = (width - 1) * -0.5f;
    float topLeftZ = (height - 1) * 0.5f;

    // If the level of detail is at 0 manually ensure 1 is passed through
    int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2; 

    // Level of detail to build the mesh at (meshSimplificationIncrement should be a factor of the mesh width)
    int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;

    MeshData meshData = new MeshData (verticesPerLine, verticesPerLine);
    int vertexIndex = 0;

    // Loop across each row and column vertex in the mesh
    for (int y = 0; y < height; y += meshSimplificationIncrement) {
      for (int x = 0; x < width; x += meshSimplificationIncrement) {
        
        // Add a vector 3 position for each vertex based on its index within the rows and columns, plus the centering and height offsets and smoothing curve
        meshData.vertices [vertexIndex] = new Vector3 (topLeftX + x, heightCurve.Evaluate(heightMap[x, y]) * heightMultiplyer, topLeftZ - y);

        // Provide each vertex with a uv percentage (0 - 1) position on the texture
        meshData.uvs [vertexIndex] = new Vector2 (x / (float)width, y / (float)height);

        // Ignore right and bottom edge vertices, define triangle pairs for each other vertex
        if (x < width - 1 && y < height - 1) {
          meshData.AddTriangle (vertexIndex                      , vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);  // Triangle defined by top-left,     bottom-right and bottom left vertices of a square
          meshData.AddTriangle (vertexIndex + verticesPerLine + 1, vertexIndex                      , vertexIndex + 1              );  // Triangle defined by bottom-right, top-left     and top-right   vertices of a square
        }

        vertexIndex++;
      }
    }

    // Return the mesh data rather than a mesh itself,
    // this is so that Unity's multi threading can be used to calculate map chunks seperately,
    // because Unity only allows meshes to be modified within the main thread
    return meshData;
  }
}

// Class to construct and store information about the triangles of the mesh, abstracted as a data rather than a mesh itself
public class MeshData {

  public Vector3[] vertices;  // Vertex vector positions
  public int[] triangles;     // Indexs in triplicates, representing each triangle of the mesh
  public Vector2[] uvs;       // % coordinates of the texture position for each vertex

  int triangleIndex;  // Stores the index at which new triangles should be created, increased by 3 after new triangles are created

  // Constructor 
  public MeshData(int meshWidth, int meshHeight) {
    vertices = new Vector3[meshWidth * meshHeight];
    uvs = new Vector2[meshWidth * meshHeight];
    triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];  // Number of squares in the mesh, and each square has two triangles of three vertices (2 * 3 = 6)
  }

  // Record the vertex index of each point of a new triangle
  public void AddTriangle(int a, int b, int c) {
    triangles [triangleIndex    ] = a;
    triangles [triangleIndex + 1] = b;
    triangles [triangleIndex + 2] = c;
    triangleIndex += 3;  // Increase index reference ready for the next traingle
  }

  // Generate the resulting mesh (this function is should be called outside of the multithreading in the main Unity thread)
  public Mesh CreateMesh() {
    Mesh mesh = new Mesh ();
    mesh.vertices = vertices;
    mesh.triangles = triangles;
    mesh.uv = uvs;
    mesh.RecalculateNormals ();  // Fix up the lighting
    return mesh;
  }

}