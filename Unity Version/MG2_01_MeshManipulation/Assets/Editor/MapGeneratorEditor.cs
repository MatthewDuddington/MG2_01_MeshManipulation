// Base code written while following tutorial: Procedural Landmass Generation (Unity 5) (c) Sebastian Lague 2016

using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(MapGenerator))]
public class MapGeneratorEditor : Editor {

  public override void OnInspectorGUI() {
    MapGenerator mapGen = (MapGenerator)target;

    if (DrawDefaultInspector ()) {
      if (mapGen.autoUpdate) {
        mapGen.GenerateMap ();
      }
    }

    if (GUILayout.Button ("Generate")) {
      mapGen.GenerateMap ();
    }
  }

}