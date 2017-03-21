using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour {

  public const float maxViewDistance = 450;  // How far the player should be able to see
  public Transform viewer;                   // The player whos view is being assessed
  public static Vector2 viewerPosition;      // Easy access to the position of that player

  public Material mapMaterial;

  static MapGenerator mapGenerator;  // Easy access to the map generator

  int chunkSize;
  int chunksVisibleInViewDistance;           // The number of chunks that will be rendered at the given view distance

  Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
  List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();  // List of which chunks were visible in the previous frame 

  void Start() {
    mapGenerator = FindObjectOfType<MapGenerator> ();
    chunkSize = MapGenerator.mapChunkSize - 1;
    chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);
  }

  void Update() {
    viewerPosition = new Vector2 (viewer.position.x, viewer.position.z);
    UpdateVisibleChunks ();
  }

  void UpdateVisibleChunks() {
    // Hide all the chunks from the previous frame
    for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++) {
      terrainChunksVisibleLastUpdate [i].SetVisible (false);
    }
    // Clear memory of which chunks were visible
    terrainChunksVisibleLastUpdate.Clear ();

    // Get the coords of the chunk the viewer is within
    int currentChunkCoordX = Mathf.RoundToInt (viewerPosition.x / chunkSize);
    int currentChunkCoordY = Mathf.RoundToInt (viewerPosition.y / chunkSize);

    for (int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++) {
      for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++) {
        Vector2 viewedChunkCoord = new Vector2 (currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

        // Check whether the visible chunk has already been instatiated
        if (terrainChunkDictionary.ContainsKey (viewedChunkCoord)) {
          terrainChunkDictionary [viewedChunkCoord].UpdateTerrainChunk ();

          // Check whether the terrain chunk is still visible
          if (terrainChunkDictionary[viewedChunkCoord].IsVisible()) {
            // Keep the chunk in the dictionary if it is visible
            terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
          }

        } else {
          terrainChunkDictionary.Add (viewedChunkCoord, new TerrainChunk (viewedChunkCoord, chunkSize, transform, mapMaterial));
        }
      }
    }
  }

  public class TerrainChunk {

    GameObject meshObject;
    Vector2 position;
    Bounds bounds;

    MapData mapData;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    public TerrainChunk(Vector2 coord, int size, Transform parent, Material material) {
      position = coord * size;
      bounds = new Bounds(position, Vector2.one * size);
      Vector3 positionV3 = new Vector3(position.x, 0, position.y);  // Vector 3 version of position for ease of use

      // Instatiate the new terrain chunk
      meshObject = new GameObject("Terrain Chunk");
      meshRenderer = meshObject.AddComponent<MeshRenderer>();
      meshFilter = meshObject.AddComponent<MeshFilter>();
      meshRenderer.material = material;

      // Setup the terrain chuck to its correct transform
      meshObject.transform.position = positionV3;
      meshObject.transform.parent = parent;
      SetVisible(false);

      // Get the data for constructing the terrain chunk mesh from the map generator
      // The Map Generator's process is run on a thread and so we pass it the function we want executed afterwards as a callback
      mapGenerator.RequestMapData(OnMapDataRecieved);
    }

    // After getting the map data, get the corisonding mesh data
    void OnMapDataRecieved(MapData mapData) {
      mapGenerator.RequestMeshData (mapData, OnMeshDataRecieved);
    }

    // Once all the data is recieved generate the mesh
    void OnMeshDataRecieved(MeshData meshData) {
      meshFilter.mesh = meshData.CreateMesh ();
    }

    public void UpdateTerrainChunk() {
      float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance (viewerPosition));
      bool isVisible = viewerDistanceFromNearestEdge <= maxViewDistance;
      SetVisible (isVisible);
    }

    public void SetVisible(bool isVisible) {
      meshObject.SetActive (isVisible);
    }

    public bool IsVisible() {
      return meshObject.activeSelf;
    }
  }

}
