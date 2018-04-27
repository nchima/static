using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPlacer : MonoBehaviour {

    [Serializable]
    class EnemyInfo {
        public GameObject prefab;
        public int firstAppearance;
        public int baseDifficulty;
        public bool isAvailable { get { return GameManager.levelManager.LevelNumber >= firstAppearance; } }
        [HideInInspector] public float tempChance;
    }
    //[SerializeField] EnemyInfo simpleEnemyInfo;
    //[SerializeField] EnemyInfo laserEnemyInfo;
    //[SerializeField] EnemyInfo meleeEnemyInfo;
    //[SerializeField] EnemyInfo tankEnemyInfo;
    //[SerializeField] EnemyInfo hoverEnemyInfo;
    //[SerializeField] EnemyInfo bossEnemyInfo;
    //List<EnemyInfo> enemyInfos;

    [SerializeField] GameObject simpleEnemyPrefab;
    [SerializeField] GameObject laserEnemyPrefab;
    [SerializeField] GameObject meleeEnemyPrefab;
    [SerializeField] GameObject tankEnemyPrefab;
    [SerializeField] GameObject hoverEnemyPrefab;
    [SerializeField] GameObject bossEnemyPrefab;

    //[Serializable]
    //class LevelEnemyNumbers {
    //    public int simpleEnemyAmmount;
    //    public int laserEnemyAmmount;
    //    public int meleeEnemyAmmount;
    //    public int simpleEnemyAmmount;
    //    public int simpleEnemyAmmount;
    //}

    int currentEnemiesToPlace = 10;
    GameObject[] placedEnemies;


    private void Awake() {
        //enemyInfos = new List<EnemyInfo>();
        //enemyInfos.Add(simpleEnemyInfo);
        //enemyInfos.Add(laserEnemyInfo);
        //enemyInfos.Add(meleeEnemyInfo);
        //enemyInfos.Add(tankEnemyInfo);
        //enemyInfos.Add(hoverEnemyInfo);
        //enemyInfos.Add(bossEnemyInfo);
    }


    public void PlaceEnemies(LevelInfo levelInfo) {
        Bounds floorBounds = GetLevelFloorBounds();
        if (placedEnemies != null) { DestroyAllPlacedEnemies(); }
        placedEnemies = new GameObject[levelInfo.TotalEnemyAmount];

        // Place each type of enemy
        for (int i = 0; i < levelInfo.simpleEnemyAmount; i++) { PlaceObject(simpleEnemyPrefab, floorBounds); }
        for (int i = 0; i < levelInfo.laserEnemyAmount; i++) { PlaceObject(laserEnemyPrefab, floorBounds); }
        for (int i = 0; i < levelInfo.meleeEnemyAmount; i++) { PlaceObject(meleeEnemyPrefab, floorBounds); }
        for (int i = 0; i < levelInfo.tankEnemyAmount; i++) { PlaceObject(tankEnemyPrefab, floorBounds); }
        for (int i = 0; i < levelInfo.hoverEnemyAmount; i++) { PlaceObject(hoverEnemyPrefab, floorBounds); }
        for (int i = 0; i < levelInfo.bossEnemyAmount; i++) { PlaceObject(bossEnemyPrefab, floorBounds); }
    }


    /*
    public void PlaceEnemies() {
        Bounds floorBounds = GetLevelFloorBounds();

        if (placedEnemies != null) { DestroyAllPlacedEnemies(); }

        GetEnemyChances();

        placedEnemies = new GameObject[currentEnemiesToPlace];

        for (int i = 0; i < currentEnemiesToPlace; i++) {
            float rand = UnityEngine.Random.value;

            for (int j = 0; j < enemyInfos.Count; j++) {
                if (!enemyInfos[j].isAvailable) { return; }

                float shangChance;
                if (j == 0) { shangChance = 0; }
                else { shangChance = enemyInfos[j - 1].tempChance; }

                float xiaChance;
                if (j == enemyInfos.Count - 1) { xiaChance = 1; }
                else { xiaChance = enemyInfos[j + 1].tempChance; }

                if (rand > shangChance && rand <= xiaChance) {
                    PlaceEnemy(enemyInfos[j].prefab, floorBounds);
                }
            }
        }

        GameManager.levelManager.SetEnemiesActive(false);
    }
    */


    // Figure out each enemy's chance of apperaing for the current level.
    //void GetEnemyChances() {

    //    // Get the total difficulty of all available enemies.
    //    int totalDifficulty = 0;
    //    int availableEnemies = 0;
    //    foreach (EnemyInfo enemyInfo in enemyInfos) {
    //        if (enemyInfo.isAvailable) {
    //            totalDifficulty += enemyInfo.baseDifficulty;

    //            // If this is the first level that this enemy is available, give it a 100% chance
    //            if (enemyInfo.firstAppearance == GameManager.levelManager.LevelNumber) {
    //                GiveEnemyFullChance(enemyInfo);
    //                return;
    //            }

    //            availableEnemies++;
    //        }
    //    }

    //    // Figure out each enemy's chance of being spawned.
    //    float last = 0f;
    //    foreach (EnemyInfo enemyInfo in enemyInfos) {
    //        if (enemyInfo.isAvailable && totalDifficulty > 0) {
    //            float chance = 1 - (enemyInfo.baseDifficulty / totalDifficulty);
    //            enemyInfo.tempChance = last + chance;
    //            last = chance;
    //        }
    //    }
    //}


    //void GiveEnemyFullChance(EnemyInfo fullChanceEnemy) {
    //    foreach(EnemyInfo enemyInfo in enemyInfos) {
    //        if (enemyInfo == fullChanceEnemy) { enemyInfo.tempChance = 1f; }
    //        else { enemyInfo.tempChance = 0f; }
    //    }
    //}


    public Bounds GetLevelFloorBounds() {
        // Combine bounds of all floor planes
        Transform floorPlaneHolder = GameObject.Find("Floor Planes").transform;
        Bounds floorBounds = new Bounds();
        for (int i = 0; i < floorPlaneHolder.childCount; i++) {
            floorBounds.Encapsulate(floorPlaneHolder.GetChild(i).GetComponent<Renderer>().bounds);
        }
        return floorBounds;
    }


    public void PlaceObject(GameObject prefab) {
        PlaceObject(prefab, GetLevelFloorBounds());
    }


    public void PlaceObject(GameObject enemyPrefab, Bounds floorBounds) {

        Vector3 testPosition = RandomPositionOnFloorBounds(floorBounds);

        for (int i = 0; i < 100; i++) {
            // Get a location
            testPosition = RandomPositionOnFloorBounds(floorBounds);

            // Check whether this location is over floor.
            float overlapRadius;
            if (enemyPrefab.GetComponent<NavMeshAgent>() != null) overlapRadius = enemyPrefab.GetComponent<NavMeshAgent>().radius * 1.5f;
            else overlapRadius = enemyPrefab.GetComponent<SphereCollider>().radius * 1.5f;
            if (!RaycastTowardsFloor(testPosition + Vector3.right * overlapRadius)) { continue; }
            if (!RaycastTowardsFloor(testPosition + Vector3.back * overlapRadius)) { continue; }
            if (!RaycastTowardsFloor(testPosition + Vector3.left * overlapRadius)) { continue; }
            if (!RaycastTowardsFloor(testPosition + Vector3.forward * overlapRadius)) { continue; }

            // See if this location is overlapping a wall, obstacle, or another enemy.
            Collider[] overlaps = Physics.OverlapSphere(testPosition, overlapRadius);
            bool onTopOfSomethingBad = false;
            foreach (Collider c in overlaps) {
                if (c.tag == "Player" || c.tag == "Enemy" || c.tag == "Obstacle" || c.tag == "Wall") {
                    onTopOfSomethingBad = true;
                }
            }
            if (onTopOfSomethingBad) { continue; }

            testPosition.y += 5f;
            Instantiate(enemyPrefab, testPosition, Quaternion.identity);
            return;
        }
    }


    void DestroyAllPlacedEnemies() {
        for (int i = 0; i < placedEnemies.Length; i++) {
            if (placedEnemies[i] == null) { continue; }
            Destroy(placedEnemies[i]);
        }
    }


    bool RaycastTowardsFloor(Vector3 rayStart) {
        rayStart.y += 5f;
        if (Physics.Raycast(rayStart, Vector3.down, 10f, 1 << 20)) {
            Debug.DrawRay(rayStart, Vector3.down * 10f, Color.green, 5f);
            return true;
        } else {
            Debug.DrawRay(rayStart, Vector3.down * 10f, Color.red, 5f);
            return false;
        }
    }


    Vector3 RandomPositionOnFloorBounds(Bounds floorBounds) {
        Vector3 randPos = floorBounds.center;
        randPos.x += UnityEngine.Random.Range(-floorBounds.extents.x, floorBounds.extents.x);
        randPos.y = 0f;
        randPos.z += UnityEngine.Random.Range(-floorBounds.extents.z, floorBounds.extents.z);
        return randPos;
    }
}
