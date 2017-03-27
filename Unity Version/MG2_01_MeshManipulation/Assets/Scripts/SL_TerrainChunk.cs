using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SL_TerrainChunk {

  GameObject meshObject;
  Vector2 position;
  Bounds bounds;

  MeshRenderer meshRenderer;
  MeshFilter meshFilter;

  SL_EndlessTerrain.LoDInfo[] detailLevels;  // User defined set of detail levels and distances for each level to apply at
  SL_LoDMesh[] lodMeshes;                    // Array of meshes that will have the various Level of Detail variations of meshes stored within as they are generated
  int previousLoDIndex = -1;                 // Begin on -1 to ensure it will always be invalid on the first check and therefore recalculated at the start

  MapData mapData;
  bool mapDataRecieved;

  public SL_TerrainChunk(Vector2 coord, int size, SL_EndlessTerrain.LoDInfo[] detailLevels, Transform parent, Material material) {
    this.detailLevels = detailLevels;

    position = coord * size;                                      // Determine the scaled position of the chunk in game space
    bounds = new Bounds(position, Vector2.one * size);            // Create a bounds struct to calculate nearest perimiter point from
    Vector3 positionV3 = new Vector3(position.x, 0, position.y);  // Vector 3 version of position for ease of use

    // Instatiate the new terrain chunk
    meshObject = new GameObject("Terrain Chunk");
    meshRenderer = meshObject.AddComponent<MeshRenderer>();
    meshFilter = meshObject.AddComponent<MeshFilter>();
    meshRenderer.material = material;

    // Setup the terrain chuck to its correct transform
    meshObject.transform.position = positionV3 * SL_EndlessTerrain.scale;
    meshObject.transform.parent = parent;
    meshObject.transform.localScale = Vector3.one * SL_EndlessTerrain.scale;
    SetVisible(false);

    // Setup the initial Level of Detail chunk, storing them in this array enables them to be reloaded rather than reaclcuated when they are revisited
    lodMeshes = new SL_LoDMesh[detailLevels.Length];
    for (int i = 0; i < detailLevels.Length; i++) {
      lodMeshes[i] = new SL_LoDMesh(detailLevels[i].levelOfDetail, UpdateTerrainChunk);
    }

    // Get the data for constructing the terrain chunk mesh from the map generator, passing in the chunks position as the centre
    // The Map Generator's process is run on a thread and so we pass it the function we want executed afterwards as a callback
    SL_EndlessTerrain.mapGenerator.RequestMapData(position, OnMapDataReceieved);

    //      meshFilter.mesh = mapGeneratorGetMapChunkUnthreaded();
  }

  // After getting the map data, get the corisonding mesh data
  void OnMapDataReceieved(MapData mapData) {
    this.mapData = mapData;
    mapDataRecieved = true;

    Texture2D texture = SL_TextureGenerator.TextureFromColourMap (mapData.colourMap, SL_MapGenerator.mapChunkSize, SL_MapGenerator.mapChunkSize);
    meshRenderer.material.mainTexture = texture;

    UpdateTerrainChunk ();  // Update the chunk only when new data is recieved
  }

  // Once all the data is recieved generate the mesh
  //    void OnMeshDataReceieved(MeshData meshData) {
  //      meshFilter.mesh = meshData.CreateMesh ();
  //    }

  public void UpdateTerrainChunk() {

    if (mapDataRecieved) {
      // Check to see whether the player's position in relation to the closest point on perimiter of the chunk is within the max view distance
      float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance (SL_EndlessTerrain.viewerPosition));
      bool isVisible = viewerDistanceFromNearestEdge <= SL_EndlessTerrain.maxViewDistance;

      if (isVisible) {
        int lodIndex = 0;

        // Check which level of detail to set the chunk to based on the distance from the player and the values given in Detail Levels
        for (int i = 0; i < detailLevels.Length - 1; i++) {
          if (viewerDistanceFromNearestEdge > detailLevels [i].visibleDistanceThreshold) {
            lodIndex = i + 1;
          } else {
            break;  // Exit once the appropriate detail level has been found
          }
        }

        // Check whether the level of detail is the same...
        if (lodIndex != previousLoDIndex) {
          // ...if it has changed then check if the required mesh has already been calcualted before...
          SL_LoDMesh lodMesh = lodMeshes [lodIndex];
          if (lodMesh.hasMesh) {
            // ...if so, then just reload the correct mesh
            previousLoDIndex = lodIndex;
            meshFilter.mesh = lodMesh.mesh;
          }
          else if (!lodMesh.hasRequestedMesh) {
            // ...if not, then recalculate the mesh
            lodMesh.RequestMesh (mapData);
          }
        }  // ...if it is the same, then dont change the mesh from the current one

        SL_EndlessTerrain.terrainChunksVisibleLastUpdate.Add (this);  // Add the chunk to the list of chunks to be cleaned up on next frame
      }

      SetVisible (isVisible);
    }
  }

  public void SetVisible(bool isVisible) {
    meshObject.SetActive (isVisible);
  }

  public bool IsVisible() {
    return meshObject.activeSelf;
  }
}
