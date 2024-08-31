using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilemapPlacer : MonoBehaviour
{
    public GameObject tilePrefab;

    /// <summary>
    /// Spawns the tilemap based on the given noise map.
    /// </summary>
    /// <param name="noiseMap">The noise map used to generate the tilemap.</param>
    /// <param name="mapWidth">The width of the map.</param>
    /// <param name="regions">The different terrain regions with their respective colors and heights.</param>
    /// <param name="tileGroup">The transform that the tile prefabs will be parented to.</param>
    public void Spawn(float[,] noiseMap, float mapWidth, TerrainType[] regions, Transform tileGroup)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        var tileMeshRenderer = tilePrefab.GetComponent<MeshRenderer>();
        var tileSize = tileMeshRenderer.bounds.size;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var y2 = y - height + tileSize.z / 2;
                var x2 = x - width + tileSize.x / 2;
                GameObject tileGameObject;
                if (y == 0 && x == 0)
                {
                    tileGameObject = Instantiate(tilePrefab, -new Vector3(x2, 0, y2), Quaternion.identity, tileGroup);
                }
                else if (x == 0 && y > 0)
                {
                    tileGameObject = Instantiate(tilePrefab, -new Vector3(x2 - (x * tileSize.x / 2), 0, y2 - y + (tileSize.z * y)), Quaternion.identity, tileGroup);
                }
                else
                {
                    tileGameObject = Instantiate(tilePrefab, -new Vector3(x2 - x + (tileSize.x * x), 0, y2 + (y * tileSize.z / 2)), Quaternion.identity, tileGroup);
                }

                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        tileGameObject.GetComponent<Renderer>().material.color = regions[i].colour;
                        break;
                    }
                }
            }
        }
    }


    private void OnValidate()
    {
        if (tilePrefab == null)
        {
            throw new ArgumentNullException("Tile prefab cannot be null.");
        }
    }
}
