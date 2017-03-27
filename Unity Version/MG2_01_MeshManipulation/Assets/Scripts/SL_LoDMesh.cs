using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Holds the mesh for a specific chunk at a specific level of detail
class SL_LoDMesh {

  public Mesh mesh;
  public bool hasRequestedMesh;  // Whether the class has already requested that its mesh be calculated and is awaiting the background thread to finish
  public bool hasMesh;           // Whether a mesh has aleady been created for this LoD
  int levelOfDetail;             // The level of detail this mesh should be loaded for
  System.Action updateCallback;  // The function to be called back once the mesh data has been recieved

  public SL_LoDMesh(int levelOfDetail, System.Action updateCallback) {
    this.levelOfDetail = levelOfDetail;
    this.updateCallback = updateCallback;
  }

  // Call his function once the threaded functions have produced a result to build the mesh from
  void OnMeshDataReceived(SL_MeshData meshData) {
    mesh = meshData.CreateMesh ();
    hasMesh = true;

    updateCallback ();
  }

  // Fill the mesh with the required level of detail
  public void RequestMesh(MapData mapData) {
    hasRequestedMesh = true;
    SL_EndlessTerrain.mapGenerator.RequestMeshData (mapData, levelOfDetail, OnMeshDataReceived);
  }

}
