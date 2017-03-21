// Base code written while following tutorial: Procedural Landmass Generation (Unity 5) (c) Sebastian Lague 2016
// Comments and supplimentory code (c) Matthew Duddington 2017

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour {

  public Renderer textureRenderer;
  public MeshFilter meshFilter;
  public MeshRenderer meshRenderer;

  public void DrawTexture(Texture2D texture) {
    textureRenderer.sharedMaterial.mainTexture = texture;
    textureRenderer.transform.localScale = new Vector3 (texture.width, 1, texture.height);
  }

  public void DrawMesh(MeshData meshData, Texture2D texture) {
    // Generate and apply the mesh to the mesh component
    // Uses 'shared' versions of calls so that the mesh and texture can be accessed outside of game mode
    meshFilter.sharedMesh = meshData.CreateMesh ();
    meshRenderer.sharedMaterial.mainTexture = texture;
  }

}
