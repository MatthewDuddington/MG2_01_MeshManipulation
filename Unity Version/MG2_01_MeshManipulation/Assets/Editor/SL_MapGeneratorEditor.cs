// Base code written while following tutorial: Procedural Landmass Generation (Unity 5) (c) Sebastian Lague 2016
// Comments and supplimentory code (c) Matthew Duddington 2017 

using System.Collections;
using UnityEngine;
using UnityEditor;

// Editor class to auto update the map content in the editor and add the 'generate' button in the inspector
[CustomEditor (typeof(SL_MapGenerator))]
public class SL_MapGeneratorEditor : Editor {

  public override void OnInspectorGUI() {
    SL_MapGenerator mapGen = (SL_MapGenerator)target;

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