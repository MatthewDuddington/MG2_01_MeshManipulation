using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour {

  public const float maxViewDistance = 450;  // How far the player should be able to see
  public Transform viewer;                   // The player whos view is being assessed
  public static Vector2 viewerPosition;      // Easy access to the position of that player

  int chunkSize;
  int chunksVisibleInViewDistance;           // The number of chunks that will be rendered at the given view distance

  Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
  List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

  void Start() {
    chunkSize = MapGenerator.mapChunkSize - 1;
    chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);
  }

  void Update() {
    viewerPosition = new Vector2 (viewer.position.x, viewer.position.z);
    UpdateVisibleChunks ();
  }

  void UpdateVisibleChunks() {
    for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++) {
      terrainChunksVisibleLastUpdate [i].SetVisible (false);
    }
    terrainChunksVisibleLastUpdate.Clear ();

    // Get the coords of the chunk the viewer is within
    int currentChunkCoordX = Mathf.RoundToInt (viewerPosition.x / chunkSize);
    int currentChunkCoordY = Mathf.RoundToInt (viewerPosition.y / chunkSize);

    for (int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++) {
      for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++) {
        Vector2 viewedChunkCoord = new Vector2 (currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

        // CHeck whether the visible chunk has already been instatiated
        if (terrainChunkDictionary.ContainsKey (viewedChunkCoord)) {
          terrainChunkDictionary [viewedChunkCoord].UpdateTerrainChunk ();

          // Check whether the terrain chunk is still visible
          if (terrainChunkDictionary[viewedChunkCoord].IsVisible()) {
            terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
          }

        } else {
          terrainChunkDictionary.Add (viewedChunkCoord, new TerrainChunk (viewedChunkCoord, chunkSize, transform));
        }
      }
    }
  }

  public class TerrainChunk {

    GameObject meshObject;
    Vector2 position;
    Bounds bounds;

    public TerrainChunk(Vector2 coord, int size, Transform parent) {
      position = coord * size;
      bounds = new Bounds(position, Vector2.one * size);
      Vector3 positionV3 = new Vector3(position.x, 0, position.y);  // Vector 3 version of position for ease of use

      meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
      meshObject.transform.position = positionV3;
      meshObject.transform.localScale = Vector3.one * size / 10f;  // Divide by 10 as default size is 10
      meshObject.transform.parent = parent;
      SetVisible(false);
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
