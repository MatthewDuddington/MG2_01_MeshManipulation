// Base code written while following tutorial: Procedural Landmass Generation (Unity 5) (c) Sebastian Lague 2016
// Comments and supplimentory code (c) Matthew Duddington 2017

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Refreshes the drawable content of the gameobjects in the scene
public class SL_MapDisplay : MonoBehaviour {

  public Renderer textureRenderer;   // Controlls the application of the material to the mesh
  public MeshFilter meshFilter;      // Where the vertices are stored
  public MeshRenderer meshRenderer;  // Controlls drawing the mesh to the screen

  // Apply the texture to the renderer component
  public void DrawTexture(Texture2D texture) {
    textureRenderer.sharedMaterial.mainTexture = texture;
    textureRenderer.transform.localScale = new Vector3 (texture.width, 1, texture.height);
  }

  // Generate and apply the mesh to the mesh component
  public void DrawMesh(SL_MeshData meshData, Texture2D texture) {
    // Use 'shared' versions of calls so that the mesh and texture can be accessed outside of game mode
    meshFilter.sharedMesh = meshData.CreateMesh ();
    meshRenderer.sharedMaterial.mainTexture = texture;
  }

}
