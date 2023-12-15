using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGenerator : MonoBehaviour
{
    public Tilemap groundTilemap;
    public Tilemap obstacleTilemap;

    public TileBase[] groundTiles; // Array of ground tiles
    public TileBase obstacleTile;  // Obstacle tile

    public int chunkSize = 16;  // Size of each chunk
    public string seed = "your_seed";

    [Range(0f, 100f)]
    public float obstaclePercentage = 5f;

    [Range(0f, 100f)]
    public float groundVariationPercentage = 1f;

    private float RDM => Random.Range(0f, 100f);

    private Transform player;
    private HashSet<Vector3Int> generatedChunks = new HashSet<Vector3Int>();

    void Start()
    {
        Random.InitState(seed.GetHashCode());
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(GenerateChunksAroundPlayer());
    }

    IEnumerator GenerateChunksAroundPlayer()
    {
        while (true)
        {
            Vector3 playerPosition = player.position;
            Vector3Int playerChunkPosition = new Vector3Int(
                Mathf.FloorToInt(playerPosition.x / chunkSize) * chunkSize,
                Mathf.FloorToInt(playerPosition.y / chunkSize) * chunkSize,
                0
            );

            for (int xOffset = -chunkSize; xOffset <= chunkSize; xOffset += chunkSize)
            {
                for (int yOffset = -chunkSize; yOffset <= chunkSize; yOffset += chunkSize)
                {
                    Vector3Int neighborChunkPosition = playerChunkPosition + new Vector3Int(xOffset, yOffset, 0);

                    if (!generatedChunks.Contains(neighborChunkPosition))
                    {
                        GenerateChunk(neighborChunkPosition);
                        generatedChunks.Add(neighborChunkPosition);
                    }
                }
            }

            // TODO: if changing the player speed or FOV this might need to be changed aswell
            yield return new WaitForSeconds(1);
        }
    }


    void GenerateChunk(Vector3Int chunkPosition)
    {
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                Vector3Int tilePosition = new Vector3Int(chunkPosition.x + x, chunkPosition.y + y, 0);

                // Place ground tile
                int groundTileIndex = (RDM < groundVariationPercentage) ? Random.Range(1, groundTiles.Length) : 0;
                groundTilemap.SetTile(tilePosition, groundTiles[groundTileIndex]);

                if (RDM < obstaclePercentage)
                {
                    // Place obstacle
                    obstacleTilemap.SetTile(tilePosition, obstacleTile);
                }
            }
        }
    }
}
