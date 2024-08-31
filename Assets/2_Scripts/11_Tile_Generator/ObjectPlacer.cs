using UnityEngine;
using System.Collections.Generic;

public class ObjectPlacer : MonoBehaviour
{
    [System.Serializable]
    public class PlaceableObject
    {
        public string name;
        public GameObject prefab;
        [Range(0f, 1f)]
        public float spawnChance;
        public Vector2Int cellSize;
    }

    public MapGenerator mapGenerator;
    public Transform groupHolder;
    public PlaceableObject[] objects;
    [Range(0f, 1f)] public float rigidness = 0.2f; // Replace spreadFactor with rigidness
    // Remove pathWidth

    public int minObjects = 5;
    public int maxObjects = 20;
    public int seed;
    private System.Random random;

    private PlaceableObject[,] placedObjects;
    private List<Vector2Int> placedObjectPositions = new List<Vector2Int>();
    private int objectsPlaced = 0;

    public List<Vector2> PlacedObjectPositions { get; private set; }

    public float offsetY = 0f; // Add this line

    private void TryPlaceObject(int x, int y)
    {
        foreach (var obj in objects)
        {
            if (random.NextDouble() <= obj.spawnChance && CanPlaceObject(x, y, obj.cellSize))
            {
                PlaceObject(x, y, obj);
                objectsPlaced++;
                return;
            }
        }
    }

    private bool CanPlaceObject(int x, int y, Vector2Int size)
    {
        if (x + size.x > mapGenerator.mapWidth - gridPadding || y + size.y > mapGenerator.mapHeight - gridPadding ||
            x < gridPadding || y < gridPadding)
            return false;

        for (int dy = -1; dy <= size.y; dy++)
        {
            for (int dx = -1; dx <= size.x; dx++)
            {
                int checkX = x + dx;
                int checkY = y + dy;
                if (checkX >= gridPadding && checkX < mapGenerator.mapWidth - gridPadding &&
                    checkY >= gridPadding && checkY < mapGenerator.mapHeight - gridPadding)
                {
                    if (placedObjects[checkX, checkY] != null || random.NextDouble() < rigidness * 0.3f)
                        return false;
                }
            }
        }

        return true;
    }

    private void PlaceObject(int x, int y, PlaceableObject obj)
    {
        for (int dy = 0; dy < obj.cellSize.y; dy++)
        {
            for (int dx = 0; dx < obj.cellSize.x; dx++)
            {
                int cellX = x + dx;
                int cellY = y + dy;
                if (cellX < mapGenerator.mapWidth - gridPadding && cellY < mapGenerator.mapHeight - gridPadding)
                {
                    placedObjects[cellX, cellY] = obj;
                }
            }
        }
        placedObjectPositions.Add(new Vector2Int(x, y));
    }


    public void PopulateGrid()
    {
        random = new System.Random(seed);
        placedObjects = new PlaceableObject[mapGenerator.mapWidth, mapGenerator.mapHeight];
        placedObjectPositions.Clear();
        objectsPlaced = 0;

        while (objectsPlaced < maxObjects)
        {
            int x = random.Next(gridPadding, mapGenerator.mapWidth - gridPadding);
            int y = random.Next(gridPadding, mapGenerator.mapHeight - gridPadding);

            if (placedObjects[x, y] == null)
            {
                TryPlaceObject(x, y);
            }

            if (objectsPlaced >= minObjects && random.NextDouble() < 0.1f)
                break;
        }
    }

    public void SpawnObjects()
    {
        if (mapGenerator == null) return;

        Vector3 mapOffset = new Vector3(
            -mapGenerator.mapWidth * CELL_SIZE / 2f,
            0,
            -mapGenerator.mapHeight * CELL_SIZE / 2f
        );
        Vector3 cellOffset = new Vector3(CELL_SIZE / 2, 0, CELL_SIZE / 2);

        // Draw placed objects
        if (placedObjects != null)
        {
            for (int y = gridPadding; y < mapGenerator.mapHeight - gridPadding; y++)
            {
                for (int x = gridPadding; x < mapGenerator.mapWidth - gridPadding; x++)
                {
                    if (placedObjects[x, y] != null)
                    {
                        Vector3 position = new Vector3(x * CELL_SIZE, offsetY, y * CELL_SIZE) + mapOffset + cellOffset;
                        Instantiate(placedObjects[x, y].prefab, position, Quaternion.identity, groupHolder);
                    }
                }
            }

            // // Draw placed object positions
            // foreach (var pos in placedObjectPositions)
            // {
            //     Vector3 position = new Vector3(pos.x * CELL_SIZE, 0, pos.y * CELL_SIZE) + mapOffset + cellOffset;
            //     Gizmos.DrawWireCube(position, new Vector3(CELL_SIZE * placedObjects[pos.x, pos.y].cellSize.x, 0.1f, CELL_SIZE * placedObjects[pos.x, pos.y].cellSize.y));
            // }
        }
    }

    private const float CELL_SIZE = 2f;

    private void OnDrawGizmos()
    {
        if (mapGenerator == null) return;

        Vector3 mapOffset = new Vector3(
            -mapGenerator.mapWidth * CELL_SIZE / 2f,
            0,
            -mapGenerator.mapHeight * CELL_SIZE / 2f
        );
        Vector3 cellOffset = new Vector3(CELL_SIZE / 2, 0, CELL_SIZE / 2);

        // Draw padding
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f); // Semi-transparent yellow
        for (int y = 0; y < mapGenerator.mapHeight; y++)
        {
            for (int x = 0; x < mapGenerator.mapWidth; x++)
            {
                if (x < gridPadding || x >= mapGenerator.mapWidth - gridPadding ||
                    y < gridPadding || y >= mapGenerator.mapHeight - gridPadding)
                {
                    Vector3 position = new Vector3(x * CELL_SIZE, 0, y * CELL_SIZE) + mapOffset + cellOffset;
                    Gizmos.DrawCube(position, new Vector3(CELL_SIZE, 0.1f, CELL_SIZE));
                }
            }
        }

        // Draw grid
        Gizmos.color = Color.gray;
        for (int x = 0; x <= mapGenerator.mapWidth; x++)
        {
            Vector3 start = new Vector3(x * CELL_SIZE, 0, 0) + mapOffset;
            Vector3 end = new Vector3(x * CELL_SIZE, 0, mapGenerator.mapHeight * CELL_SIZE) + mapOffset;
            Gizmos.DrawLine(start, end);
        }
        for (int y = 0; y <= mapGenerator.mapHeight; y++)
        {
            Vector3 start = new Vector3(0, 0, y * CELL_SIZE) + mapOffset;
            Vector3 end = new Vector3(mapGenerator.mapWidth * CELL_SIZE, 0, y * CELL_SIZE) + mapOffset;
            Gizmos.DrawLine(start, end);
        }

        // Draw placed objects
        if (placedObjects != null)
        {
            Gizmos.color = Color.red;
            for (int y = gridPadding; y < mapGenerator.mapHeight - gridPadding; y++)
            {
                for (int x = gridPadding; x < mapGenerator.mapWidth - gridPadding; x++)
                {
                    if (placedObjects[x, y] != null)
                    {
                        Vector3 position = new Vector3(x * CELL_SIZE, offsetY, y * CELL_SIZE) + mapOffset + cellOffset;
                        Gizmos.DrawCube(position, new Vector3(CELL_SIZE, 0.1f, CELL_SIZE));
                    }
                }
            }

            // Draw placed object positions
            foreach (var pos in placedObjectPositions)
            {
                Vector3 position = new Vector3(pos.x * CELL_SIZE, offsetY, pos.y * CELL_SIZE) + mapOffset + cellOffset;
                Gizmos.DrawWireCube(position, new Vector3(CELL_SIZE * placedObjects[pos.x, pos.y].cellSize.x, 0.1f, CELL_SIZE * placedObjects[pos.x, pos.y].cellSize.y));
            }
        }
    }

    public int gridPadding = 1;
}