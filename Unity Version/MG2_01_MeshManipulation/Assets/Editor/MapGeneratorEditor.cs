// Base code written while following tutorial: Procedural Landmass Generation (Unity 5) (c) Sebastian Lague 2016

using System.Collections;
using UnityEngine;
using UnityEditor;

// Editor class to auto update the map content in the editor and add the 'generate' button in the inspector
[CustomEditor (typeof(MapGenerator))]
public class MapGeneratorEditor : Editor {

  public override void OnInspectorGUI() {
    MapGenerator mapGen = (MapGenerator)target;

    // If auto update is active, refresh the various properties of the generated content as variables change
    if (DrawDefaultInspector ()) {
      if (mapGen.autoUpdate) {
        mapGen.DrawMapInEditor ();
      }
    }

    // Add generate button to the inspector
    if (GUILayout.Button ("Generate")) {
      mapGen.DrawMapInEditor ();
    }
  }

}