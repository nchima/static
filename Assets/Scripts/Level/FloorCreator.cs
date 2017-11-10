using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorCreator : MonoBehaviour {

    //[SerializeField] int gridSize;  // Grid size in both directions.
    //[SerializeField] float tileSize;    // Tile size in both directions.

    FloatRange tileSizeRange = new FloatRange(5f, 8f);

	[SerializeField] GameObject floorHolder;
    [SerializeField] GameObject floorTilePrefab;


    //private void Start()
    //{
    //    CreateGrid();
    //}


    public void CreateGrid(float worldSpaceGridSize)
    {
        Debug.Log("Doing.");

        // Calculate tile size & number of tiles from the grid size.
        int gridSize = 25;
        float tileSize = worldSpaceGridSize / gridSize;

        for (int i = 0; i < 100; i++)
        {
            tileSize = worldSpaceGridSize / gridSize;

            if (tileSize < tileSizeRange.min)
            {
                gridSize--;
            }

            else if (tileSize > tileSizeRange.max)
            {
                gridSize++;
            }

            else
            {
                break;
            }
        }

        Debug.Log("Grid size: " + gridSize + ", World space grid size: " + worldSpaceGridSize + ", Tile size: " + tileSize);

        // Get total size of grid in world space.
        //worldSpaceGridSize = gridSize * tileSize;

        // Find the position of the first tile (top left corner).
        Vector3 firstTilePosition = Vector3.zero;
        firstTilePosition.x -= ((worldSpaceGridSize * 0.5f) - (tileSize * 0.5f));
        firstTilePosition.z -= ((worldSpaceGridSize * 0.5f) - (tileSize * 0.5f));
        Vector3 currentTilePosition = firstTilePosition;

        // Begin iterating through grid and adding tiles one by one.
        for (int y = 0; y < gridSize; y++)
        {
            currentTilePosition.z = firstTilePosition.z + (y * tileSize);

            for (int x = 0; x < gridSize; x++)
            {
                currentTilePosition.x = firstTilePosition.x + (x * tileSize);

                GameObject newTile = CreateTile(
                    currentTilePosition,
                    tileSize,
                    MyMath.Map(Vector3.Distance(Vector3.zero, currentTilePosition), 10, Vector3.Distance(Vector3.zero, firstTilePosition), 0f, 1f)
                    //MyMath.Map(x, 0, gridSize-1, 0f, 0.5f) + MyMath.Map(y, 0, gridSize-1, 0.5f, 0f)
                    //MyMath.Map(x, 0, gridSize-1, 0.5f, 0f) + MyMath.Map(y, 0, gridSize-1, 0f, 0.5f)
                    //MyMath.Map(x + y + 1, 1, gridSize/2, 0f, 1f),
                    //MyMath.Map(x + y + 1, 1, gridSize/2, 1f, 0f)
                    );

                newTile.name = x.ToString() + ", " + y.ToString();
            }
        }

        // Resize and activate floor collider.
        floorHolder.GetComponent<BoxCollider>().size = new Vector3(
            worldSpaceGridSize,
            floorHolder.GetComponent<BoxCollider>().size.y,
            worldSpaceGridSize
            );
    }


    GameObject CreateTile(Vector3 position, float tileSize, float gunValue)
    {
        GameObject newTile = Instantiate(floorTilePrefab);

        newTile.transform.position = position;
        newTile.transform.Rotate(new Vector3(90f, 0f, 0f));
        newTile.transform.localScale = new Vector3(tileSize, tileSize, 1f);

        //float shotgunValue = MyMath.Map(Mathf.Abs(0.5f - gunValue), 0f, 0.5f, 0f, 1f);
        float shotgunValue = gunValue;
        float rifleValue = 1 - shotgunValue;

        //Debug.Log("Shotgun: " + gunValue + ", " + "Rifle: " + rifleValue);

        newTile.GetComponent<MeshRenderer>().material.color = new Color(shotgunValue + 0.1f, shotgunValue + 0.1f, shotgunValue + 0.1f);
        newTile.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(1f, shotgunValue * 10f + 1f);

        newTile.GetComponent<FloorTile>().shotgunValue = shotgunValue;
        newTile.GetComponent<FloorTile>().rifleValue = rifleValue;

        newTile.transform.parent = floorHolder.transform;

        return newTile;
    }
}
