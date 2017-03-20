// Base code written while following tutorial: Procedural Landmass Generation (Unity 5) (c) Sebastian Lague 2016

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour {

  public Renderer textureRenderer;

  public void DrawTexture(Texture2D texture) {
    textureRenderer.sharedMaterial.mainTexture = texture;
    textureRenderer.transform.localScale = new Vector3 (texture.width, 1, texture.height);
  }

}
