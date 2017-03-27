// Base code written while following tutorial: Procedural Landmass Generation (Unity 5) (c) Sebastian Lague 2016
// Comments and supplimentory code (c) Matthew Duddington 2017

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SL_EndlessTerrain : MonoBehaviour {

  public const float scale = 10f;  // How big the reletive size of the map should be within the game

  // Define how far the player can move away from their previous known location when the chunk was updated
  // (saves on recalculations when the player is static or moving slowly)
  // square distance stored to save on square root functions during comparisons
  const float viewerMoveThresholdForChunkUpdate = 25f;
  const float sqr_viewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

  public LoDInfo[] detailLevels;        // User defiend set of distances at which the different level of detail meshes should be applied
  public static float maxViewDistance;  // How far the player should be able to see

  public Transform viewer;               // The player whos view is being assessed
  public static Vector2 viewerPosition;  // Easy access to the position of that player
  Vector2 viewerPositionOld;             // Where the vierwer was previously seen when the chunk was updated

  public Material mapMaterial;

  public static SL_MapGenerator mapGenerator;  // Easy access to the map generator

  int chunkSize;
  int chunksVisibleInViewDistance;  // The number of chunks that will be rendered at the given view distance

  Dictionary<Vector2, SL_TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, SL_TerrainChunk>();  // List of all chunks which has been loaded into the game so far, which grows as the player explores
  public static List<SL_TerrainChunk> terrainChunksVisibleLastUpdate = new List<SL_TerrainChunk>();          // List of which chunks were visible in the previous frame 

  void Start() {
    mapGenerator = FindObjectOfType<SL_MapGenerator> ();

    maxViewDistance = detailLevels [detailLevels.Length - 1].visibleDistanceThreshold;  // Max view should be equal to the value of the last LoD element in the array
    chunkSize = SL_MapGenerator.mapChunkSize - 1;
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

    // Loop through all the chunks in the distance grid surrounding the chunk the viewer is within
    for (int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++) {
      for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++) {
        Vector2 viewedChunkCoord = new Vector2 (currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

        // Check whether the visible chunk has already been instatiated...
        if (terrainChunkDictionary.ContainsKey (viewedChunkCoord)) {
          // ...if so, then update it
          terrainChunkDictionary [viewedChunkCoord].UpdateTerrainChunk ();
        } else {
          // ...otherwise instantiate a new chunk and add it to the map dictionary
          terrainChunkDictionary.Add (viewedChunkCoord, new SL_TerrainChunk (viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
        }
      }
    }
  }
    
  [System.Serializable]
  public struct LoDInfo {
    public int levelOfDetail;
    public float visibleDistanceThreshold;
  }

}
