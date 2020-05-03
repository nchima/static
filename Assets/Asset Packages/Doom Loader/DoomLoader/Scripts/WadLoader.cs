#if UNITY_EDITOR
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System;

public class WadLoader : MonoBehaviour 
{
    public static WadLoader Instance;

    //void Awake()
    //{
    //    Instance = this;
    //}

    public Color ambientLightColor = Color.white;
    public bool deathmatch;
    public static List<Lump> lumps = new List<Lump>();
    public string filePath = "Doom1.WAD";
    public string wadName;
    public string autoloadMapName = "E1M1";
    public UnityEngine.SceneManagement.Scene scene;
    public GameObject PlayerObject;

    public void ConvertMap()
    {
        Instance = this;

        Shader.SetGlobalColor("_AMBIENTLIGHT", ambientLightColor);

        if (string.IsNullOrEmpty(filePath))
            return;

        if (!Load(filePath))
            return;

        GetComponent<TextureLoader>().Initialize();
        GetComponent<Mesher>().Initialize();
        GetComponent<MapLoader>().Initialize();

        //TextureLoader.Instance.LoadAndBuildAll();

        if (!string.IsNullOrEmpty(autoloadMapName))
            if (MapLoader.Instance.Load(autoloadMapName))
            {
                Mesher.Instance.CreateMeshes(wadName, scene);

                //MapLoader.Instance.ApplyLinedefBehavior();

                //ThingManager.Instance.CreateThings(deathmatch);

                //if (PlayerStart.PlayerStarts[0] == null)
                //    Debug.LogError("PlayerStart1 == null");
                //else
                //{
                //    PlayerObject.transform.position = PlayerStart.PlayerStarts[0].transform.position;
                //    PlayerObject.transform.rotation = PlayerStart.PlayerStarts[0].transform.rotation;
                //}

                //PlayerObject.GetComponent<AudioSource>().clip = SoundLoader.LoadSound("DSPISTOL");
                //PlayerObject.GetComponent<AudioSource>().Play();
            }
    }

    public bool Load(string filePath)
    {
        string path = filePath;
        if (!File.Exists(path))
        {
            Debug.LogError("WadLoader: Load: File \"" + filePath + "\" does not exist!");
            return false;
        }

        FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        BinaryReader reader = new BinaryReader(stream, Encoding.ASCII);

        if (filePath.Length < 4)
        {
            reader.Close();
            stream.Close();
            Debug.LogError("WadLoader: Load: WAD length < 4!");
            return false;
        }

        try
        {
            stream.Seek(0, SeekOrigin.Begin);

            bool isiwad = (Encoding.ASCII.GetString(reader.ReadBytes(4)) == "IWAD"); //other option is "PWAD"
            if (isiwad) { }

            int numlumps = reader.ReadInt32();
            int lumpsoffset = reader.ReadInt32();

            stream.Seek(lumpsoffset, SeekOrigin.Begin);

            lumps.Clear();

            for (int i = 0; i < numlumps; i++)
            {
                int offset = reader.ReadInt32();
                int length = reader.ReadInt32();
                string name = Encoding.ASCII.GetString(reader.ReadBytes(8)).TrimEnd('\0').ToUpper();

                lumps.Add(new Lump(offset, length, name));
            }

            //load the whole wad into memory
            long bytes = 0;
            foreach(Lump l in lumps)
            {
                l.data = new byte[l.length];
                stream.Seek(l.offset, SeekOrigin.Begin);
                stream.Read(l.data, 0, l.length);
                bytes += l.length;
            }

            Debug.Log("Loaded WAD \"" + filePath + "\" (" + bytes + " bytes in lumps)");
        }
        catch(Exception e)
        {
            Debug.LogError("WadLoader: Load: Reader exception!");
            Debug.LogError(e);

            reader.Close();
            stream.Close();
            return false;
        }

        reader.Close();
        stream.Close();
        return true;
    }
}
#endif