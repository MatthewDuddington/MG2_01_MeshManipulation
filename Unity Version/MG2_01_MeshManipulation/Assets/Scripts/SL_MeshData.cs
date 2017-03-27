// Base code written while following tutorial: Procedural Landmass Generation (Unity 5) (c) Sebastian Lague 2016
// Comments and supplimentory code (c) Matthew Duddington 2017 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class to construct and store information about the triangles of the mesh, abstracted as a data rather than a mesh itself
public class SL_MeshData {

  public Vector3[] vertices;  // Vertex vector positions
  public int[] triangles;     // Indexs in triplicates, representing each triangle of the mesh
  public Vector2[] uvs;       // Normalised coordinates of the texture position for each vertex

  int triangleIndex;  // Stores the index at which new triangles should be created, increased by 3 after new triangles are created

  // Constructor 
  public SL_MeshData(int meshWidth, int meshHeight) {
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
