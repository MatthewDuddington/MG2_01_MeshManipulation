using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour {

  const float scale = 10f;

  const float viewerMoveThresholdForChunkUpdate = 25f;
  const float sqr_viewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

  public LoDInfo[] detailLevels;
  public static float maxViewDistance;  // How far the player should be able to see

  public Transform viewer;                   // The player whos view is being assessed
  public static Vector2 viewerPosition;      // Easy access to the position of that player
  Vector2 viewerPositionOld;

  public Material mapMaterial;

  static MapGenerator mapGenerator;  // Easy access to the map generator

  int chunkSize;
  int chunksVisibleInViewDistance;  // The number of chunks that will be rendered at the given view distance

  Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
  static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();  // List of which chunks were visible in the previous frame 

  void Start() {
    mapGenerator = FindObjectOfType<MapGenerator> ();

    maxViewDistance = detailLevels [detailLevels.Length - 1].visibleDistanceThreshold;  // Max view should be equal to the value of the last LoD element in the array
    chunkSize = MapGenerator.mapChunkSize - 1;
    chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);

    UpdateVisibleChunks ();  // Draw the chunks at least once at the start of the program
  }

  void Update() {
    viewerPosition = new Vector2 (viewer.position.x, viewer.position.z) / scale;  // Divide by scale to carry effects across each variable

    // Update chunk information only on frames where the player has moved sufficently
    if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqr_viewerMoveThresholdForChunkUpdate) {
      viewerPositionOld = viewerPosition;  // Reset record of last viewer position where an update occoured
      UpdateVisibleChunks ();
    }
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
        } else {
          terrainChunkDictionary.Add (viewedChunkCoord, new TerrainChunk (viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
        }
      }
    }
  }

  public class TerrainChunk {

    GameObject meshObject;
    Vector2 position;
    Bounds bounds;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    LoDInfo[] detailLevels;
    LoDMesh[] lodMeshes;
    int previousLoDIndex = -1;

    MapData mapData;
    bool mapDataRecieved;

    public TerrainChunk(Vector2 coord, int size, LoDInfo[] detailLevels, Transform parent, Material material) {
      this.detailLevels = detailLevels;

      position = coord * size;
      bounds = new Bounds(position, Vector2.one * size);
      Vector3 positionV3 = new Vector3(position.x, 0, position.y);  // Vector 3 version of position for ease of use

      // Instatiate the new terrain chunk
      meshObject = new GameObject("Terrain Chunk");
      meshRenderer = meshObject.AddComponent<MeshRenderer>();
      meshFilter = meshObject.AddComponent<MeshFilter>();
      meshRenderer.material = material;

      // Setup the terrain chuck to its correct transform
      meshObject.transform.position = positionV3 * scale;
      meshObject.transform.parent = parent;
      meshObject.transform.localScale = Vector3.one * scale;
      SetVisible(false);

      lodMeshes = new LoDMesh[detailLevels.Length];
      for (int i = 0; i < detailLevels.Length; i++) {
        lodMeshes[i] = new LoDMesh(detailLevels[i].levelOfDetail, UpdateTerrainChunk);
      }

      // Get the data for constructing the terrain chunk mesh from the map generator, passing in the chunks position as the centre
      // The Map Generator's process is run on a thread and so we pass it the function we want executed afterwards as a callback
      mapGenerator.RequestMapData(position, OnMapDataReceieved);

      //      meshFilter.mesh = mapGeneratorGetMapChunkUnthreaded();
    }

    // After getting the map data, get the corisonding mesh data
    void OnMapDataReceieved(MapData mapData) {
//      mapGenerator.RequestMeshData (mapData, OnMeshDataReceieved);
      this.mapData = mapData;
      mapDataRecieved = true;

      Texture2D texture = TextureGenerator.TextureFromColourMap (mapData.colourMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
      meshRenderer.material.mainTexture = texture;

      UpdateTerrainChunk ();  // Update the chunk only when new data is recieved
    }

    // Once all the data is recieved generate the mesh
//    void OnMeshDataReceieved(MeshData meshData) {
//      meshFilter.mesh = meshData.CreateMesh ();
//    }

    public void UpdateTerrainChunk() {
      if (mapDataRecieved) {

        float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance (viewerPosition));
        bool isVisible = viewerDistanceFromNearestEdge <= maxViewDistance;

        if (isVisible) {
          int lodIndex = 0;

          for (int i = 0; i < detailLevels.Length - 1; i++) {
            if (viewerDistanceFromNearestEdge > detailLevels [i].visibleDistanceThreshold) {
              lodIndex = i + 1;
            } else {
              break;
            }
          }

          if (lodIndex != previousLoDIndex) {
            LoDMesh lodMesh = lodMeshes [lodIndex];
            if (lodMesh.hasMesh) {
              previousLoDIndex = lodIndex;
              meshFilter.mesh = lodMesh.mesh;
            }
            else if (!lodMesh.hasRequestedMesh) {
              lodMesh.RequestMesh (mapData);
            }
          }

          terrainChunksVisibleLastUpdate.Add (this);  // Add the chunk to the list of chunks to be cleaned up on next frame
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

  class LoDMesh {

    public Mesh mesh;
    public bool hasRequestedMesh;
    public bool hasMesh;
    int levelOfDetail;
    System.Action updateCallback;

    public LoDMesh(int levelOfDetail, System.Action updateCallback) {
      this.levelOfDetail = levelOfDetail;
      this.updateCallback = updateCallback;
    }

    void OnMeshDataReceived(MeshData meshData) {
      mesh = meshData.CreateMesh ();
      hasMesh = true;

      updateCallback ();
    }

    public void RequestMesh(MapData mapData) {
      hasRequestedMesh = true;
      mapGenerator.RequestMeshData (mapData, levelOfDetail, OnMeshDataReceived);
    }

  }
    
  [System.Serializable]
  public struct LoDInfo {
    public int levelOfDetail;
    public float visibleDistanceThreshold;
  }

}
