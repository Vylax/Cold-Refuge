using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGenerator : MonoBehaviour
{
    public Tilemap groundTilemap;
    public Tilemap obstacleTilemap;

    public TileBase[] groundTiles; // Array of ground tiles
    public TileBase[] obstacleTiles;  // Array of obstacle tiles
    public GameObject[] trees; // Array of tree prefabs

    public int chunkSize = 16;  // Size of each chunk
    public string seed = "your_seed";

    [Range(0f, 100f)]
    public float obstaclePercentage = 5f;

    [Range(0f, 100f)]
    public float groundVariationPercentage = 1f;

    [Range(0f, 100f)]
    public float treePercentage = 2f;

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


                if (RDM < treePercentage)
                {
                    // Place tree
                    int treeIndex = Random.Range(0, trees.Length);

                    // Get the size of the tree prefab
                    Vector3 treeSize = trees[treeIndex].GetComponent<SpriteRenderer>().bounds.size;

                    // Calculate the offset to center the tree on the tile
                    Vector3 offset = new Vector3(treeSize.x / 2f + 0.5f, treeSize.y / 2f, 0f);

                    GameObject tree = Instantiate(trees[treeIndex], tilePosition + offset, Quaternion.identity);

                    // Adjust the sorting order based on the tree's y-coordinate
                    SpriteRenderer treeRenderer = tree.GetComponent<SpriteRenderer>();
                    SpriteRenderer woodRenderer = tree.transform.Find("sapin_tronc").GetComponent<SpriteRenderer>();
                    treeRenderer.sortingOrder = Mathf.FloorToInt(10000-tilePosition.y);
                    woodRenderer.sortingOrder = Mathf.FloorToInt(-10);
                }
                else if (RDM < obstaclePercentage)
                {
                    // Place obstacle
                    int obstacleTileIndex = Random.Range(0, obstacleTiles.Length);
                    obstacleTilemap.SetTile(tilePosition, obstacleTiles[obstacleTileIndex]);
                }
            }
        }
    }
}
