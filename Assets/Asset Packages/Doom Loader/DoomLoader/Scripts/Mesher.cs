#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class Mesher : MonoBehaviour {
    [SerializeField] float obstacleHeight = 25f;

    [SerializeField] GameObject wallPrefab;
    [SerializeField] GameObject railingPrefab;
    [SerializeField] GameObject floorPrefab;
    [SerializeField] GameObject obstacleTopPrefab;
    [SerializeField] GameObject obstacleSidePrefab;

    public static Mesher Instance;

    GameObject level;
    GameObject railingParent;
    GameObject wallParent;
    GameObject obstacleParent;
    GameObject floorParent;

    public void Initialize() {
        Instance = this;
    }

    public void CreateMeshes(string mapName) {
        // Create game objects to hold static level.
        level = new GameObject("Level");
        railingParent = new GameObject("Railings");
        railingParent.transform.parent = level.transform;
        obstacleParent = new GameObject("Obstacles");
        obstacleParent.transform.parent = level.transform;
        wallParent = new GameObject("Walls");
        wallParent.transform.parent = level.transform;
        floorParent = new GameObject("Floor Planes");
        floorParent.transform.parent = level.transform;

        //Transform holder = new GameObject("MapMeshes").transform;
        //holder.transform.SetParent(transform);

        //sectors
        {
            Triangulator triangulator = new Triangulator();

            int index = 0;
            foreach (Sector s in MapLoader.sectors) {
                triangulator.Triangulate(s);

                //floor
                {
                    //GameObject sectorObject = new GameObject("Sector_" + index + "_floor");
                    //sectorObject.transform.SetParent(holder);
                    //MeshRenderer mr = sectorObject.AddComponent<MeshRenderer>();
                    //MeshFilter meshFilter = sectorObject.AddComponent<MeshFilter>();
                    Mesh mesh = new Mesh();
                    //meshFilter.mesh = mesh;

                    //if (!MaterialManager.Instance.OverridesFlat(s.floorTexture, sectorObject, mr))
                    //    mr.material = MaterialManager.Instance.defaultMaterial;

                    //if (mr.material.mainTexture == null) {
                    //    MaterialPropertyBlock materialProperties = new MaterialPropertyBlock();
                    //    materialProperties.SetTexture("_MainTex", TextureLoader.Instance.GetFlatTexture(s.floorTexture));
                    //    mr.SetPropertyBlock(materialProperties);
                    //}

                    mesh.name = "Sector_" + index + "_floor_mesh";

                    int vc = Triangulator.vertices.Count;

                    Vector3[] vertices = new Vector3[vc];
                    Vector3[] normals = new Vector3[vc];
                    Vector2[] uvs = new Vector2[vc];
                    Color[] colors = new Color[vc];
                    int[] indices = new int[vc];

                    int v = 0;
                    foreach (Vector2D p in Triangulator.vertices) {
                        vertices[v] = new Vector3(p.x, s.floorHeight, p.y);
                        indices[v] = v;
                        normals[v] = Vector3.up;
                        uvs[v] = new Vector2(p.x / MapLoader.flatUVdividor, p.y / MapLoader.flatUVdividor);
                        colors[v] = Color.white * s.brightness;
                        v++;
                    }

                    mesh.vertices = vertices;
                    mesh.triangles = indices;
                    mesh.normals = normals;
                    mesh.uv = uvs;
                    mesh.colors = colors;

                    mesh.RecalculateBounds();

                    //MeshCollider mc = sectorObject.AddComponent<MeshCollider>();
                    //mc.sharedMesh = mesh;

                    //SectorController controller = sectorObject.AddComponent<SectorController>();
                    //s.floorObject = sectorObject;
                    //controller.sector = s;
                    //controller.Init();

                    // Detemine sector type
                    int sectorType = 0; // 0: Floor, 1: Obstacle, 2: Hole, 3: Error
                    if (s.floorHeight == 0f) { sectorType = 0; }
                    else if (s.floorHeight == 0.3125f) { sectorType = 1; }
                    else if (s.floorHeight == 0.625f) { sectorType = 2; }
                    else {
                        sectorType = 3;
                        Debug.LogError("Sector has floor height which does not correspond to Static level. Use 0 for floor, 10 for obstacle, and 20 for hole");
                    }

                    // Save as floor
                    if (sectorType == 0) {
                        CreateFloorMesh(mesh, mapName, index, false);
                    }

                    // Save as obstacle
                    else if (sectorType == 1) {
                        CreateFloorMesh(mesh, mapName, index, true);
                    }

                    // Do nothing :)
                    else {
                        // :)
                    }
                }

                //ceiling
                //Triangulator.vertices.Reverse();
                //{
                //    GameObject sectorObject = new GameObject("Sector_" + index + "_ceiling");
                //    sectorObject.transform.SetParent(holder);
                //    //MeshRenderer mr = sectorObject.AddComponent<MeshRenderer>();
                //    MeshFilter meshFilter = sectorObject.AddComponent<MeshFilter>();
                //    Mesh mesh = new Mesh();
                //    meshFilter.mesh = mesh;
                //    mesh.name = "Sector_" + index + "_ceiling_mesh";

                    //if (!MaterialManager.Instance.OverridesFlat(s.ceilingTexture, sectorObject, mr))
                    //    mr.material = MaterialManager.Instance.defaultMaterial;

                    //if (mr.material.mainTexture == null) {
                    //    MaterialPropertyBlock materialProperties = new MaterialPropertyBlock();
                    //    materialProperties.SetTexture("_MainTex", TextureLoader.Instance.GetFlatTexture(s.ceilingTexture));
                    //    mr.SetPropertyBlock(materialProperties);
                    //}

                //    int vc = Triangulator.vertices.Count;

                //    Vector3[] vertices = new Vector3[vc];
                //    Vector3[] normals = new Vector3[vc];
                //    Vector2[] uvs = new Vector2[vc];
                //    Color[] colors = new Color[vc];
                //    int[] indices = new int[vc];

                //    int v = 0;
                //    foreach (Vector2D p in Triangulator.vertices) {
                //        vertices[v] = new Vector3(p.x, s.ceilingHeight, p.y);
                //        indices[v] = v;
                //        normals[v] = -Vector3.up;
                //        uvs[v] = new Vector2(p.x / MapLoader.flatUVdividor, p.y / MapLoader.flatUVdividor);
                //        colors[v] = Color.white * s.brightness;
                //        v++;
                //    }

                //    mesh.vertices = vertices;
                //    mesh.triangles = indices;
                //    mesh.normals = normals;
                //    mesh.uv = uvs;
                //    mesh.colors = colors;

                //    mesh.RecalculateBounds();

                //    MeshCollider mc = sectorObject.AddComponent<MeshCollider>();
                //    mc.sharedMesh = mesh;

                //    SectorController controller = sectorObject.AddComponent<SectorController>();
                //    s.ceilingObject = sectorObject;
                //    controller.sector = s;
                //    controller.Init();
                //}

                index++;
            }
        }

        //walls
        {
            int index = 0;
            foreach (Linedef l in MapLoader.linedefs) {
                if (l.Back != null && l.Front != null) {
                    //top part (front)
                    if (l.Front.Sector.ceilingHeight > l.Back.Sector.ceilingHeight)
                        l.TopFrontObject = CreateLineQuad
                            (
                                l.Front,
                                l.Back.Sector.ceilingHeight,
                                l.Front.Sector.ceilingHeight,
                                l.Front.tHigh,
                                l.Front.offsetX,
                                (l.flags & (1 << 3)) != 0 ? l.Front.offsetY : -l.Front.offsetY,
                                (l.flags & (1 << 3)) != 0 ? 0 : 1,
                                false,
                                l.Front.Sector.brightness,
                                true,
                                "Wall_" + index + "_top_front"
                            //holder
                            );

                    //top part (back)
                    if (l.Front.Sector.ceilingHeight < l.Back.Sector.ceilingHeight)
                        l.TopBackObject = CreateLineQuad
                            (
                                l.Back,
                                l.Front.Sector.ceilingHeight,
                                l.Back.Sector.ceilingHeight,
                                l.Back.tHigh,
                                l.Back.offsetX,
                                (l.flags & (1 << 3)) != 0 ? l.Back.offsetY : l.Back.offsetY,
                                (l.flags & (1 << 3)) != 0 ? 0 : 1,
                                true,
                                l.Back.Sector.brightness,
                                true,
                                "Wall_" + index + "_top_back"
                            //holder
                            );

                    //bottom part (front)
                    if (l.Front.Sector.minimumFloorHeight < l.Back.Sector.floorHeight)
                        l.BotFrontObject = CreateLineQuad
                            (
                                l.Front,
                                l.Front.Sector.minimumFloorHeight,
                                l.Back.Sector.floorHeight,
                                l.Front.tLow,
                                l.Front.offsetX,
                                l.Front.offsetY,
                                ((l.flags & (1 << 4)) != 0) ? 2 : 0,
                                false,
                                l.Front.Sector.brightness,
                                true,
                                "Wall_" + index + "_bot_front"
                            //holder
                            );

                    //bottom part (back)
                    if (l.Front.Sector.floorHeight > l.Back.Sector.floorHeight)
                        l.BotBackObject = CreateLineQuad
                            (
                                l.Back,
                                l.Back.Sector.floorHeight,
                                l.Front.Sector.floorHeight,
                                l.Back.tLow,
                                l.Back.offsetX,
                                l.Front.offsetY,
                                ((l.flags & (1 << 4)) != 0) ? 2 : 0,
                                true,
                                l.Back.Sector.brightness,
                                true,
                                "Wall_" + index + "_bot_back"
                            //holder
                            );

                    //middle (front)
                    if (l.Front.tMid != "-")
                        l.MidFrontObject = CreateLineQuad
                            (
                                l.Front,
                                Mathf.Max(l.Front.Sector.floorHeight, l.Back.Sector.floorHeight),
                                Mathf.Min(l.Front.Sector.ceilingHeight, l.Back.Sector.ceilingHeight),
                                l.Front.tMid,
                                l.Front.offsetX,
                                l.Front.offsetY,
                                ((l.flags & (1 << 4)) != 0) ? 1 : 0,
                                false,
                                l.Front.Sector.brightness,
                                false,
                                "Wall_" + index + "_mid_front"
                            //holder
                            );

                    //middle (back)
                    if (l.Back.tMid != "-")
                        l.MidBackObject = CreateLineQuad
                            (
                                l.Back,
                                Mathf.Max(l.Front.Sector.floorHeight, l.Back.Sector.floorHeight),
                                Mathf.Min(l.Front.Sector.ceilingHeight, l.Back.Sector.ceilingHeight),
                                l.Back.tMid,
                                l.Back.offsetX,
                                l.Back.offsetY,
                                ((l.flags & (1 << 4)) != 0) ? 1 : 0,
                                true,
                                l.Back.Sector.brightness,
                                false,
                                "Wall_" + index + "_mid_back"
                            //holder
                            );

                    if ((l.flags & (1 << 0)) != 0)
                        CreateInvisibleBlocker
                            (
                                l,
                                Mathf.Max(l.Front.Sector.floorHeight, l.Back.Sector.floorHeight),
                                Mathf.Min(l.Front.Sector.ceilingHeight, l.Back.Sector.ceilingHeight),
                                "Wall_" + index + "_blocker"
                            //holder
                            );

                } else //solid wall
                    if (l.Front == null) { return; }
                    CreateLineQuad
                        (
                            l.Front,
                            l.Front.Sector.minimumFloorHeight,
                            l.Front.Sector.maximumCeilingHeight,
                            l.Front.tMid,
                            l.Front.offsetX,
                            l.Front.offsetY,
                            ((l.flags & (1 << 4)) != 0) ? 1 : 0,
                            false,
                            l.Front.Sector.brightness,
                            true,
                            "Wall_" + index
                        //holder
                        );

                index++;
            }
        }
    }

    void CreateFloorMesh(Mesh mesh, string mapName, int index, bool isObstacle) {
        GameObject floorObject;
        if (isObstacle) {
            floorObject = PrefabUtility.InstantiatePrefab(obstacleTopPrefab as GameObject) as GameObject;
        } else {
            floorObject = PrefabUtility.InstantiatePrefab(floorPrefab as GameObject) as GameObject;
        }
        floorObject.transform.parent = floorParent.transform;

        if (isObstacle) {
            floorObject.transform.position = new Vector3(0f, obstacleHeight, 0f);
        } else {
            floorObject.transform.position = Vector3.zero;
        }

        floorObject.transform.localScale = Vector3.one;

        floorObject.GetComponent<MeshFilter>().mesh = mesh;
        floorObject.transform.GetChild(0).GetComponent<MeshFilter>().mesh = mesh;

        MeshCollider newMeshCollider = floorObject.AddComponent<MeshCollider>();
        newMeshCollider.sharedMesh = mesh;

        DestroyImmediate(floorObject.GetComponent<BoxCollider>());

        SaveMesh(mesh, mapName + " Floor " + index, false, false);
    }


    public GameObject CreateLineQuad(Sidedef s, float min, float max, string tex, int offsetX, int offsetY, int peg, bool invert, float brightness, bool blocks, string objname /*Transform holder*/) {
        if (max - min <= 0)
            return null;

        if (s.Line.start == s.Line.end)
            return null;

        if (tex == "-")
            tex = "DOORTRAK";

        //GameObject wallObject = new GameObject(objname);
        //s.gameObject = wallObject;
        //wallObject.transform.SetParent(holder);
        //MeshRenderer mr = wallObject.AddComponent<MeshRenderer>();
        //MeshFilter meshFilter = wallObject.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        //meshFilter.mesh = mesh;
        mesh.name = objname + "_mesh";
        //Texture mainTexture = null;

        //if (!MaterialManager.Instance.OverridesWall(tex, wallObject, mr))
        //    if (TextureLoader.NeedsAlphacut.ContainsKey(tex))
        //        mr.material = MaterialManager.Instance.alphacutMaterial;
        //    else
        //        mr.material = MaterialManager.Instance.defaultMaterial;

        //if (mr.material.mainTexture == null) {
        //    mainTexture = TextureLoader.Instance.GetWallTexture(tex);

        //    MaterialPropertyBlock materialProperties = new MaterialPropertyBlock();
        //    materialProperties.SetTexture("_MainTex", mainTexture);
        //    mr.SetPropertyBlock(materialProperties);
        //} else
        //    mainTexture = mr.material.mainTexture;

        int vc = 4;

        Vector3[] vertices = new Vector3[vc];
        Vector3[] normals = new Vector3[vc];
        Vector2[] uvs = new Vector2[vc];
        Color[] colors = new Color[vc];
        int[] indices = new int[6];

        vertices[0] = new Vector3(s.Line.start.Position.x, min, s.Line.start.Position.y);
        vertices[1] = new Vector3(s.Line.end.Position.x, min, s.Line.end.Position.y);
        vertices[2] = new Vector3(s.Line.start.Position.x, max, s.Line.start.Position.y);
        vertices[3] = new Vector3(s.Line.end.Position.x, max, s.Line.end.Position.y);

        //if (mainTexture != null) {
        //    float length = (s.Line.start.Position - s.Line.end.Position).GetLength();
        //    float height = max - min;
        //    float u = length / ((float)mainTexture.width / MapLoader.sizeDividor);
        //    float v = height / ((float)mainTexture.height / MapLoader.sizeDividor);
        //    float ox = (float)offsetX / (float)mainTexture.width;
        //    float oy = (float)offsetY / (float)mainTexture.height;

        //    if (peg == 2) {
        //        float sheight = s.Sector.ceilingHeight - s.Sector.floorHeight;
        //        float sv = sheight / ((float)mainTexture.height / MapLoader.sizeDividor);

        //        uvs[0] = new Vector2(ox, 1 - sv);
        //        uvs[1] = new Vector2(u + ox, 1 - sv);
        //        uvs[2] = new Vector2(ox, 1 - sv + v);
        //        uvs[3] = new Vector2(u + ox, 1 - sv + v);
        //    } else if (peg == 1) {
        //        uvs[0] = new Vector2(ox, oy);
        //        uvs[1] = new Vector2(u + ox, oy);
        //        uvs[2] = new Vector2(ox, v + oy);
        //        uvs[3] = new Vector2(u + ox, v + oy);
        //    } else {
        //        uvs[0] = new Vector2(ox, 1 - v - oy);
        //        uvs[1] = new Vector2(u + ox, 1 - v - oy);
        //        uvs[2] = new Vector2(ox, 1 - oy);
        //        uvs[3] = new Vector2(u + ox, 1 - oy);
        //    }
        //}

        //if (invert) {
        //    indices[0] = 0;
        //    indices[1] = 1;
        //    indices[2] = 2;
        //    indices[3] = 2;
        //    indices[4] = 1;
        //    indices[5] = 3;

        //    uvs = new Vector2[4] { uvs[1], uvs[0], uvs[3], uvs[2] };
        //} else {
        //    indices[0] = 2;
        //    indices[1] = 1;
        //    indices[2] = 0;
        //    indices[3] = 3;
        //    indices[4] = 1;
        //    indices[5] = 2;
        //}

        Vector3 normal = (vertices[0] - vertices[1]).normalized;
        float z = normal.z;
        float x = normal.x;
        normal.x = -z;
        normal.z = x;
        for (int i = 0; i < 4; i++) {
            normals[i] = invert ? -normal : normal;
            colors[i] = Color.white * brightness;
        }

        //mesh.vertices = vertices;
        //mesh.triangles = indices;
        //mesh.normals = normals;
        //mesh.uv = uvs;
        //mesh.colors = colors;

        //mesh.RecalculateBounds();

        //if (blocks) {
        //    MeshCollider mc = wallObject.AddComponent<MeshCollider>();
        //    mc.sharedMesh = mesh;
        //}

        /* CREATE STATIC LEVEL */

        // Figure out whether it will be a wall or a railing
        bool isWall = Vector3.Distance(vertices[0], vertices[2]) > 1f;

        float distanceBetweenVertices = Vector3.Distance(vertices[0], vertices[2]);
        //Debug.Log("distance btwn vertices: " + distanceBetweenVertices);
        int wallType = 0;   // 0 = Railing, 1 = Wall, 2 = Obstacle side

        if (distanceBetweenVertices == 0.625f) { wallType = 0; }
        else if (distanceBetweenVertices == 3.375f) { wallType = 1; }
        else if (distanceBetweenVertices == 4) { wallType = 2; }
        else { Debug.Log("Unrecognized wall type."); }

        if (s.IsFront) {
            Debug.Log("doing.");
            if (s.Other == null) { wallType = 1; }
            else if (s.Other.Sector.floorHeight == 0.3125f) { wallType = 2; }
            else if (s.Other.Sector.floorHeight == 0.625f) { wallType = 0; }
        }

        GameObject newObject;
        float newObjectHeight = 0f;

        // Railing
        if (wallType == 0) {
            newObject = PrefabUtility.InstantiatePrefab(railingPrefab as GameObject) as GameObject;
            newObject.transform.parent = wallParent.transform;
            newObjectHeight = 1f;
        }

        // Wall
        else if (wallType == 1) {
            newObject = PrefabUtility.InstantiatePrefab(wallPrefab as GameObject) as GameObject;
            newObject.transform.parent = railingParent.transform;
            newObjectHeight = 10f;
        }
        
        // Obstacle type
        else if (wallType == 2) {
            newObject = PrefabUtility.InstantiatePrefab(obstacleSidePrefab as GameObject) as GameObject;
            newObject.transform.parent = obstacleParent.transform;
            newObjectHeight = obstacleHeight;
        }

        else {
            Debug.LogError("Unrecognized wall type.");
            return null;
        }

        // Get center of new object
        Vector3 newObjectCenter = Vector3.Lerp(vertices[0], vertices[3], 0.5f);
        newObjectCenter.y = newObjectHeight / 2;
        newObject.transform.position = newObjectCenter;

        // Rotate new object
        newObject.transform.LookAt(newObject.transform.position - normal);

        // Scale newObject
        Vector3 newScale = new Vector3(Vector3.Distance(vertices[0], vertices[1]), newObjectHeight, 1f);
        newObject.transform.localScale = newScale;

        return newObject;
    }


    public Mesh CreateBillboardMesh(float width, float height, float pivotX, float pivotY) {
        Mesh mesh = new Mesh();
        mesh.name = "Billboard";

        Vector3[] vertices = new Vector3[4];
        Vector2[] uvs = new Vector2[4];
        int[] indices = new int[6];

        float x0 = -width * pivotX;
        float x1 = width * (1 - pivotX);
        float y0 = -height * pivotY;
        float y1 = height * (1 - pivotY);

        vertices[0] = new Vector3(x0, y0, 0);
        vertices[1] = new Vector3(x1, y0, 0);
        vertices[2] = new Vector3(x0, y1, 0);
        vertices[3] = new Vector3(x1, y1, 0);

        indices[0] = 2;
        indices[1] = 1;
        indices[2] = 0;
        indices[3] = 3;
        indices[4] = 1;
        indices[5] = 2;

        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(1, 0);
        uvs[2] = new Vector2(0, 1);
        uvs[3] = new Vector2(1, 1);

        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.uv = uvs;

        mesh.bounds = new Bounds(new Vector3(0, (y0 + y1) * .5f, 0), new Vector3(width, height, width) * 2f);

        return mesh;
    }

    public void CreateInvisibleBlocker(Linedef l, float min, float max, string objname/*, Transform holder*/) {
        if (max - min <= 0)
            return;

        if (l.start == l.end)
            return;

        GameObject blocker = new GameObject(objname);
        //blocker.transform.SetParent(holder);
        blocker.layer = 9;
        Mesh mesh = new Mesh();
        mesh.name = objname + "_mesh";

        Vector3[] vertices = new Vector3[8];
        Vector3[] normals = new Vector3[8];

        int[] indices = new int[12];

        vertices[0] = new Vector3(l.start.Position.x, min, l.start.Position.y);
        vertices[1] = new Vector3(l.end.Position.x, min, l.end.Position.y);
        vertices[2] = new Vector3(l.start.Position.x, max, l.start.Position.y);
        vertices[3] = new Vector3(l.end.Position.x, max, l.end.Position.y);

        vertices[4] = vertices[0];
        vertices[5] = vertices[1];
        vertices[6] = vertices[2];
        vertices[7] = vertices[3];

        indices[0] = 0;
        indices[1] = 1;
        indices[2] = 2;
        indices[3] = 2;
        indices[4] = 1;
        indices[5] = 3;

        indices[6] = 2;
        indices[7] = 1;
        indices[8] = 0;
        indices[9] = 3;
        indices[10] = 1;
        indices[11] = 2;

        Vector3 normal = (vertices[0] - vertices[1]).normalized;
        float z = normal.z;
        float x = normal.x;
        normal.x = -z;
        normal.z = x;
        for (int i = 0; i < 4; i++)
            normals[i] = -normal;
        for (int i = 4; i < 8; i++)
            normals[i] = normal;

        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.normals = normals;

        mesh.RecalculateBounds();

        MeshCollider mc = blocker.AddComponent<MeshCollider>();
        mc.sharedMesh = mesh;
    }

    public BoxCollider CreateLineCollider(Linedef l, float min, float max, string objname, Transform holder) {
        if (max - min <= 0)
            return null;

        if (l.start == l.end)
            return null;

        GameObject blocker = new GameObject(objname);
        blocker.transform.SetParent(holder);
        blocker.layer = 9;

        BoxCollider mc = blocker.AddComponent<BoxCollider>();
        Vector2 width = new Vector2(l.start.Position.x, l.start.Position.y) - new Vector2(l.end.Position.x, l.end.Position.y);
        mc.size = new Vector3(width.magnitude, max - min, .1f);
        Vector2 dir = width.normalized;
        blocker.transform.rotation = Quaternion.Euler(0, Mathf.Atan2(-dir.y, dir.x) * Mathf.Rad2Deg, 0);
        blocker.transform.position = (new Vector3(l.start.Position.x, min, l.start.Position.y) + new Vector3(l.end.Position.x, max, l.end.Position.y)) * .5f;

        return mc;
    }


    public static void SaveMesh(Mesh mesh, string name, bool makeNewInstance, bool optimizeMesh) {
#if UNITY_EDITOR
        string path = UnityEditor.EditorUtility.SaveFilePanel("Save Separate Mesh Asset", "Assets/Models/Level Geo/Level Floors/", name, "asset");
        if (string.IsNullOrEmpty(path)) return;

        path = UnityEditor.FileUtil.GetProjectRelativePath(path);

        Mesh meshToSave = (makeNewInstance) ? Object.Instantiate(mesh) as Mesh : mesh;

        if (optimizeMesh)
            UnityEditor.MeshUtility.Optimize(meshToSave);

        UnityEditor.AssetDatabase.CreateAsset(meshToSave, path);
        UnityEditor.AssetDatabase.SaveAssets();
#endif
    }
}

#endif