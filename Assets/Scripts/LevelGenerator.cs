using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour {

    [SerializeField] bool dontGenerate = false;  // Debug: used for testing layouts.

    // LEVEL GEN VARIABLES
    public float baseLevelSize; // The size of the level (the level is always square so this is the width and height.)
    [SerializeField] float levelSizeIncrease;   // How much the size of each level increases each level.

    // Enemies
    int currentNumberOfEnemies; // The number of enemies in the current level.

    [SerializeField] EnemiesPerLevel[] enemiesPerLevel;

    //[SerializeField] int basicEnemiesAddedPerLevel = 7;

    //[SerializeField] int firstLevelWithTankEnemies = 2;
    //[SerializeField] int tankEnemiesAddedPerLevel = 1;

    //[SerializeField] int firstLevelWithMeleeEnemies = 2;
    //[SerializeField] int meleeEnemiesAddedPerLevel = 3;

    //[SerializeField] int firstLevelWithLaserEnemies = 5;
    //[SerializeField] int laserEnemiesAddedPerLevel = 1; 

    // Guaranteed Empty Space
    [SerializeField] FloatRange emptyAreaRange = new FloatRange(0.25f, 0.75f);   // Guaranteed empty space as percentage of level's total area.
    [SerializeField] FloatRange plazaSizeRange = new FloatRange(1f, 20f);
    [SerializeField] FloatRange corridorWidthRange = new FloatRange(5f, 10f);
    List<GameObject> emptySpaces;

    // Obstacles
    float numberOfObstacles;
    [SerializeField] IntRange numberOfObstaclesRange = new IntRange(15, 40); // The number of obstacles in a level.
    [SerializeField] FloatRange obstacleSizeRange = new FloatRange(4f, 30f);
    float numberOfTrees;
    [SerializeField] GameObject obstacleContainer;
    
    // NAVMMESH STUFF
    [SerializeField] Transform navMeshRoot;

    // PREFAB REFERENCES
    [SerializeField] private GameObject basicEnemyPrefab;
    [SerializeField] private GameObject tankEnemyPrefab;
    [SerializeField] private GameObject meleeEnemyPrefab;
    [SerializeField] private GameObject snailEnemyPrefab;
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] GameObject emptySpacePrefab;

    GameManager gameManager;
    Transform player;
    [SerializeField] Transform floor;
    [SerializeField] Transform[] walls;


    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        //floor = GameObject.Find("Floor").transform;
        emptySpaces = new List<GameObject>();

        //GetComponent<FloorCreator>().CreateGrid(baseLevelSize);
    }


    public void Generate()
    {
        if (dontGenerate) return;

        if (GameManager.levelManager.currentLevelNumber != 0) baseLevelSize += levelSizeIncrease;

        currentNumberOfEnemies = 0;
        numberOfObstacles = numberOfObstaclesRange.Random + GameManager.levelManager.currentLevelNumber * 4;

        // Clear level of all current obstacles and enemies.
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Destroy(go);
        }

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Obstacle"))
        {
            Destroy(go);
        }

        foreach (EnemyShot shot in FindObjectsOfType<EnemyShot>())
        {
            Destroy(shot.gameObject);
        }

        foreach (PlayerMissile playerMissile in FindObjectsOfType<PlayerMissile>())
        {
            Destroy(playerMissile.gameObject);
        }

        SetupWallsAndFloor();

        SetupEmptySpace();

        // Put all things in the level.
        for (int i = 0; i < numberOfObstacles; i++) {
            PlaceObstacle();
        }

        for (int i = 0; i < Random.Range(5, emptySpaces.Count); i++)
        {
            PlaceColumn();
        }

        obstacleContainer.transform.position = new Vector3(0f, -19.59f, 0f);

        //for (int i = 0; i < gameManager.levelNumber * basicEnemiesAddedPerLevel; i++)
        //{
        //    float rand = Random.value;
        //    if (rand <= 0.25f) PlaceEnemy(basicEnemyPrefab);
        //    else if (rand <= 0.5f) PlaceEnemy(tankEnemyPrefab);
        //    else if (rand <= 0.75f) PlaceEnemy(meleeEnemyPrefab);
        //    else PlaceEnemy(snailEnemyPrefab);
        //}

        AddEnemies();

        //Debug.Log("Number of enemies: " + numberOfEnemies);
        gameManager.currentEnemyAmt = currentNumberOfEnemies;

        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            enemy.GetComponent<Enemy>().willAttack = false;
        }

        // Delete all empty space.
        for (int i = 0; i < emptySpaces.Count; i++) Destroy(emptySpaces[i]);
        emptySpaces.Clear();
    }


    void AddEnemies()
    {
        EnemiesPerLevel enemiesPerCurrentLevel = enemiesPerLevel[GameManager.levelManager.currentLevelNumber];

        for (int i = 0; i < enemiesPerCurrentLevel.basicEnemies; i++) PlaceEnemy(basicEnemyPrefab);
        for (int i = 0; i < enemiesPerCurrentLevel.meleeEnemies; i++) PlaceEnemy(meleeEnemyPrefab);
        for (int i = 0; i < enemiesPerCurrentLevel.tankEnemies; i++) PlaceEnemy(tankEnemyPrefab);
        for (int i = 0; i < enemiesPerCurrentLevel.snailEnemies; i++) PlaceEnemy(snailEnemyPrefab);
        currentNumberOfEnemies = enemiesPerCurrentLevel.total;
    }


    void SetupEmptySpace()
    {
        // Get the area of space (in square units) which should be guaranteed empty.
        float emptyArea = ((baseLevelSize*2) * (baseLevelSize*2)) * emptyAreaRange.Random;
        float currentPlazaArea = 0f;

        if (emptySpaces.Count > 0) emptySpaces.Clear();

        int loopSafeguard1 = 0;
        // Fill up level with empty space.
        while (currentPlazaArea < emptyArea)
        {
            // Create a plaza.
            GameObject newPlaza = Instantiate(emptySpacePrefab);

            bool plazaTransformChosen = false;
            Vector3 newPlazaPosition = Vector3.zero;
            Vector3 newPlazaScale = Vector3.zero;

            int loopSafeguard2 = 0;
            while (!plazaTransformChosen)
            {
                // Give the new plaza a random size and location.
                newPlazaScale = new Vector3(
                        plazaSizeRange.Random,
                        1f,
                        plazaSizeRange.Random
                    );

                newPlazaPosition = new Vector3(
                        Random.Range(-baseLevelSize + newPlaza.transform.localScale.x / 2, baseLevelSize - newPlaza.transform.localScale.x / 2),
                        1f,
                        Random.Range(-baseLevelSize + newPlaza.transform.localScale.z / 2, baseLevelSize - newPlaza.transform.localScale.z / 2)
                    );

                if (Physics.OverlapBox(newPlazaPosition, newPlazaScale * 0.5f, Quaternion.identity, 1<<15).Length == 0)
                {
                    plazaTransformChosen = true;
                }

                loopSafeguard2++;
                if (loopSafeguard2 > 1000) break;
            }

            newPlaza.transform.localScale = newPlazaScale;
            newPlaza.transform.position = newPlazaPosition;

            // Add the new plaza's area to the total amount of empty space.
            currentPlazaArea += newPlaza.transform.localScale.x * newPlaza.transform.localScale.z;

            // If this is not the first empty space in the current level, see if it needs a connecting corridor.
            if (emptySpaces.Count > 0)
            {
                //Debug.Log("Checking to see if we need to generate a corridor.");

                // See if the new plaza is overlapping any other instance of empty space.
                Vector3 newPlazaHalfExtents = new Vector3(newPlaza.transform.localScale.x / 2, newPlaza.transform.localScale.y, newPlaza.transform.localScale.z / 2);
                bool newPlazaIsOverlappingEmptySpace = Physics.OverlapBox(newPlaza.transform.position, newPlazaHalfExtents, newPlaza.transform.rotation, 1 << 15).Length-1 > 0;
                //Debug.Log("New plaza overlapping this many empty areas: " + Physics.OverlapBox(newPlaza.transform.position, newPlazaHalfExtents, newPlaza.transform.rotation, 1 << 15).Length);

                // If the new plaza is not overlapping empty space, create a corridor to another random instance of empty space.
                if (!newPlazaIsOverlappingEmptySpace)
                {
                    //Debug.Log("Creating corridor.");

                    // Select a random instance of empty space and get its position.
                    Vector3 corridorEnd = emptySpaces[Random.Range(0, emptySpaces.Count)].transform.position;

                    // Get the point half way between the new plaza and the chosen end point.
                    Vector3 corridorPosition = newPlaza.transform.position + (corridorEnd - newPlaza.transform.position) / 2;

                    // Get a rotation so that the new corridor is oriented in the correct manner.
                    Quaternion corridorRotation = Quaternion.LookRotation(Vector3.Normalize(corridorPosition - corridorEnd), Vector3.up);

                    // Instantiate the new corridor and set its width and length.
                    GameObject newCorridor = Instantiate(emptySpacePrefab, corridorPosition, corridorRotation);
                    newCorridor.transform.localScale = new Vector3(
                            corridorWidthRange.Random,
                            1f,
                            Vector3.Distance(newPlaza.transform.position, corridorEnd)
                        );

                    // Add the new corridor to the list of empty spaces, then update the area of the level that has been filled with empty space.
                    emptySpaces.Add(newCorridor);
                }
            }

            // Add the new plaza to the list of empty space.
            emptySpaces.Add(newPlaza);
            //Debug.Log("Number of empty spaces: " + emptySpaces.Count);
            //Debug.Log("Total Empty Area: " + currentPlazaArea + ", Empty Area To Fill: " + emptyArea);

            loopSafeguard1++;
            if (loopSafeguard1 > 1000) break;
        }
    }


    public void SetupWallsAndFloor()
    {
       // Give the correct size and position to the floor and walls.
       floor.localScale = new Vector3(
           baseLevelSize * 0.2f,
           1f,
           baseLevelSize * 0.2f
           );

        FloorTile[] floorTiles = FindObjectsOfType<FloorTile>();
        for (int i = 0; i < floorTiles.Length; i++) Destroy(floorTiles[i].gameObject);

        //GetComponent<FloorCreator>().CreateGrid(baseLevelSize);

        for (int i = 0; i < walls.Length; i++)
        {
            walls[i].localScale = new Vector3(
                baseLevelSize,
                walls[i].localScale.y,
                walls[i].localScale.z
                );

            if (walls[i].position.z > 0) walls[i].position = new Vector3(0f, walls[i].transform.position.y, baseLevelSize * 0.5f);
            else if (walls[i].position.z < 0) walls[i].position = new Vector3(0f, walls[i].transform.position.y, -baseLevelSize * 0.5f);
            else if (walls[i].position.x > 0) walls[i].position = new Vector3(baseLevelSize * 0.5f, walls[i].transform.position.y, 0f);
            else walls[i].position = new Vector3(-baseLevelSize * 0.5f, walls[i].transform.position.y, 0f);
        }
    }


    void PlaceObstacle()
    {
        Vector3 newPosition = Vector3.zero;
        Vector3 newScale = Vector3.zero;
        Quaternion newRotation = Quaternion.identity;

        bool placed = false;
        int loopSafeguard = 0;

        while (!placed)
        {
            // Get size
            newScale = new Vector3(
                obstacleSizeRange.Random,
                20f,
                obstacleSizeRange.Random
            );

            // Get my position
            newPosition = new Vector3(
                Random.Range(-baseLevelSize + newScale.x / 2, baseLevelSize - newScale.x / 2),
                newScale.y * 0.5f,
                Random.Range(-baseLevelSize + newScale.z / 2, baseLevelSize - newScale.z / 2)
            );

            // Get a random rotation.
            //newRotation = Quaternion.Euler(newRotation.x, Random.Range(-180f, 180f), newRotation.y);

            // Test this location with an overlap box that is high enough to catch the player in midair.
            // Also make it a little bit larger than the actual obstacle.
            Collider[] overlaps = Physics.OverlapBox(newPosition, new Vector3(newScale.x/2, 400, newScale.z/2), newRotation);

            placed = true;

            // Make sure this obstacle is in a good location.
            foreach (Collider collider in overlaps)
            {
                // Make sure the obstacle is not on top of a player/enemy and not within designated empty space.
                if (collider.tag == "Player" || collider.tag == "Enemy" || collider.tag == "Empty Space") //|| collider.tag == "Obstacle" || collider.tag == "Wall")
                {
                    //Debug.Log("Obstacle was going to be placed on: " + collider.tag);
                    placed = false;
                    break;
                }
            }

            if (placed)
            {
                // Make sure the obstacle is not too close to another one so as to create tempting but impassible gap.
                // See if there are any obstacles around me.
                Vector3 overlapBoxExtents = new Vector3(
                        (newScale.x / 2) + player.GetComponent<CapsuleCollider>().radius * 3f,
                        newScale.y / 2,
                        (newScale.z / 2) + player.GetComponent<CapsuleCollider>().radius * 3f
                    );

                foreach (Collider nearbySolid in Physics.OverlapBox(newPosition, overlapBoxExtents, newRotation))
                {
                    // See if this obstacle is not also overlapping the position of the new obstacle itself.
                    if (!nearbySolid.bounds.Intersects(new Bounds(newPosition, newScale)))
                    {
                        if (nearbySolid.tag == "Obstacle")
                        {
                            // If it's not, then this position is inappropriate.
                            placed = false;
                            break;
                        }

                        else if (nearbySolid.tag == "Wall")
                        {
                            //Debug.Log("Moved obstacle to touch wall.");

                            // Figure out which direction the wall is in.
                            Vector3 toWall = nearbySolid.ClosestPointOnBounds(newPosition) - newPosition;
                            Debug.DrawLine(nearbySolid.ClosestPointOnBounds(newPosition), newPosition, Color.green, 5f);
                            //Debug.Log("Direction to wall: " + toWall);

                            //if (toWall == Vector3.zero)
                            //{
                            //    Debug.Log("Zero vector");
                            //    Debug.Log("New position: " + newPosition);
                            //    Debug.Log("Closest point to new position: " + nearbySolid.ClosestPointOnBounds(newPosition));
                            //    Debug.DrawRay(newPosition, Vector3.up * 100f, Color.cyan);
                            //    Debug.Break();
                            //}

                            // Get the half extent of the obstacle on the proper coordinate.
                            Vector3 halfExtent = newScale;
                            halfExtent.Scale(toWall.normalized);
                            halfExtent *= 0.5f;
                            //Debug.Log("Half extent: " + halfExtent);

                            // Move the obstacle in the direction of the wall by it's distance from the wall minus it's half extent from above.
                            newPosition += (toWall - halfExtent);
                            //Debug.Log("Moving to: " + (newPosition + (toWall - halfExtent)));
                            //Debug.DrawLine(newPosition, newPosition + (toWall - halfExtent), Color.red, 1f);
                            //Debug.Break();
                            placed = true;
                            break;
                        }
                    }
                }
            }

            loopSafeguard++;
            if (loopSafeguard > 100) return;
        }

        // Instantiate the obstacle.
        GameObject newObstacle = Instantiate(obstaclePrefab);
        newObstacle.transform.position = newPosition;
        newObstacle.transform.localScale = newScale;
        newObstacle.transform.rotation = newRotation;
        newObstacle.transform.parent = obstacleContainer.transform;
    }


    void PlaceColumn()
    {
        Vector3 newPosition = Vector3.zero;
        Vector3 newScale = Vector3.zero;
        Quaternion newRotation = Quaternion.identity;

        bool placed = false;
        int loopSafeguard = 0;

        while (!placed)
        {
            // Get size
            newScale = new Vector3(5f, 20f, 5f);

            // Get my position
            newPosition = new Vector3(
                Random.Range(-baseLevelSize + newScale.x / 2, baseLevelSize - newScale.x / 2),
                newScale.y * 0.5f,
                Random.Range(-baseLevelSize + newScale.z / 2, baseLevelSize - newScale.z / 2)
            );

            // Get a random rotation.
            //newRotation = Quaternion.Euler(newRotation.x, Random.Range(-180f, 180f), newRotation.y);

            // Test this location with an overlap box that is high enough to catch the player in midair.
            // Also make it a little bit larger than the actual obstacle.
            Collider[] overlaps = Physics.OverlapBox(newPosition, new Vector3(newScale.x + 5f, 400, newScale.z + 5f), newRotation);

            // Make sure this obstacle isn't going to be placed on top of the player or an enemy or empty space.
            placed = true;
            bool inEmptySpace = false;
            foreach (Collider collider in overlaps)
            {
                if (collider.tag == "Empty Space") inEmptySpace = true;

                if (collider.tag == "Player" || collider.tag == "Enemy" || collider.tag == "Obstacle" || collider.tag == "Wall")
                {
                    placed = false;
                    break;
                }
            }

            if (!inEmptySpace)
            {
                placed = false;
            }

            loopSafeguard++;
            if (loopSafeguard > 100) return;
        }

        // Instantiate the obstacle.
        GameObject newObstacle = Instantiate(obstaclePrefab);
        newObstacle.transform.position = newPosition;
        newObstacle.transform.localScale = newScale;
        newObstacle.transform.rotation = newRotation;
        newObstacle.transform.parent = obstacleContainer.transform;
    }



    void GenerateNavMesh()
    {
        //List<NavMeshBuildMarkup> markups = new List<NavMeshBuildMarkup>();
        //List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
        //NavMeshBuilder.CollectSources(navMeshRoot, 1 << 8 | 1 << 9, NavMeshCollectGeometry.PhysicsColliders, 0, markups, sources);
        //navMeshRoot.GetComponent<NavMeshSurface>().BuildNavMesh();
        //navMeshRoot.GetComponent<NavMeshSurface>().navMeshData;
    }


    void PlaceEnemy(GameObject enemyToPlace)
    {
        GameObject newEnemy = Instantiate(enemyToPlace);
        Vector3 newPosition = Vector3.zero;

        //Debug.Log(newEnemy.name);

        bool placed = false;
        int loopSafeguard = 0;

        while (!placed)
        {
            // Get my position
            newPosition = new Vector3(
                Random.Range(-baseLevelSize + newEnemy.GetComponent<Collider>().bounds.extents.x, baseLevelSize - newEnemy.GetComponent<Collider>().bounds.extents.x),
                2f,
                Random.Range(-baseLevelSize + newEnemy.GetComponent<Collider>().bounds.extents.z, baseLevelSize - newEnemy.GetComponent<Collider>().bounds.extents.z)
            );

            // Test this location
            bool inEmptySpace = false;
            bool collidingWithObject = false;
            Collider[] overlaps = Physics.OverlapSphere(newPosition, newEnemy.GetComponent<Collider>().bounds.extents.x * 1.5f);
            foreach (Collider c in overlaps)
            {
                if (c.tag == "Player" || c.tag == "Enemy" || c.tag == "Obstacle" || c.tag == "Wall")
                {
                    Debug.Log("Tried to place enemy on " + c.tag);
                    collidingWithObject = true;
                    continue;
                }

                if (c.tag == "Empty Space")
                {
                    inEmptySpace = true;
                }
            }

            if (!collidingWithObject && inEmptySpace)
            {
                placed = true;
            }

            loopSafeguard++;
            if (loopSafeguard > 100)
            {
                Debug.Log("Infinite Loop");
                Destroy(newEnemy);
                return;
            }
        }

        newEnemy.transform.position = newPosition;

        //currentNumberOfEnemies++;
        //Debug.Log("Number of enemies in this level: " + numberOfEnemies);
    }
}






    /*
     * 
     * christmas bells were ringing
     * in the idea of all that i've lost
     * if you ever see that big picture
     * i bet a mirror isn't that robust
     * 
     * but you'll never get sick where i come from
     * there's a ministry in every bite
     * 
     * now I'm a believer
     * a big time believer
     * 
     * you gotta hand it to the other side
     * at least they're forgiven
     * and they've got a rhythm that you can't deny
     * 
     * i tend to hate a good kisser
     * the smell of mistletoe is so plain
     * i've become such a bad singer
     * my girlfriend called me by my last name
     * 
     * they say when you go back to where you come from
     * you're bound to lose that self control
     * 
     * now i'm a believer
     * a true blue believer
     * 
     * you gotta hand it to the birds and the bees
     * at least they're human
     * but i've got enough of that sweet disease
     * 
     */
