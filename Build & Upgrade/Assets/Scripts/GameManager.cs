using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using Discord;
using System.Linq;
using System;
using System.Text;
using System.Runtime.Serialization;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public bool HasSaved;

    [System.Serializable]
    public class BuildingElement
    {
        public string Name;
        public List<Mesh> BuildingMeshes;
        public Texture BuildingTexture;
        public List<int> NeededStats;
    }

    [System.Serializable]
    public class BuildingSaveList
    {
        public List<BuildingSave> Saves = new List<BuildingSave>();
    }

    [System.Serializable]
    public class BuildingSave
    {
        public int Index;
        public Vector3 Position;
        public Vector3 Rotation;
        public int BuildingId;
        public int MaxLevel = 7;
        public int CurrentLevel = 0;

        public bool NoWater = true;
    }

    public class Vector3SerializationSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context)
        {
            Vector3 v3 = (Vector3)obj;
            info.AddValue("x", v3.x);
            info.AddValue("y", v3.y);
            info.AddValue("z", v3.z);
        }
        public System.Object SetObjectData(System.Object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            Vector3 v3 = (Vector3)obj;
            v3.x = (float)info.GetValue("x", typeof(float));
            v3.y = (float)info.GetValue("y", typeof(float));
            v3.z = (float)info.GetValue("z", typeof(float));
            obj = v3;
            return obj;
        }
    }

    public Building MainBuilding;

    public List<int> _Stats = new List<int> { 1, 25, 25};
    public int StatsMax = 255;
    public List<BuildingElement> Buildings;

    [SerializeField] Text MenAmountTextUI;
    [SerializeField] Text WoodAmountTextUI;
    [SerializeField] Text FoodAmountTextUI;

    public BuildingSaveList BuildingSaves;

    public List<int> RemovedObstacleTilePositions = new List<int>();
    public List<Vector3> RemovedObstacleVector3s = new List<Vector3>();
    /*public List<int> BuildingIndexes = new List<int>();
    public List<int> BuildingPositions = new List<int>();
    public List<int> BuildingRotations = new List<int>();
    public List<Vector3> BuildingVector3s = new List<Vector3>();
    public List<Vector3> BuildingQuaternions = new List<Vector3>();*/

    public Discord.Discord _Discord;

    public void Load()
    {
        CameraController.Instance.transform.position = new Vector3();
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        List<byte> _Bytes = Load("Default");
        if (_Bytes != null && _Bytes.Count > 0)
        {
            Vector2Int TileOffset = new Vector2Int(Convert.ToInt32(_Bytes[0]), Convert.ToInt32(_Bytes[1]));
            TileGenerator.Instance.Offset = TileOffset;
            TileGenerator.Instance.RandomOffset = false;
            CameraController.Instance.transform.position = new Vector3(_Bytes[2], CameraController.Instance.transform.position.y, _Bytes[3]);
        }
        List<byte> _StatsBytes = Load("Stats");
        if (_StatsBytes != null && _StatsBytes.Count > 0)
        {
            for (int i = 0; i < _Stats.Count; i++)
            {
                _Stats[i] = _StatsBytes[i];
            }
        }
        List<byte> _RemovedObstacleBytes = Load("1");
        if (_RemovedObstacleBytes != null && _RemovedObstacleBytes.Count > 0)
        {
            int Number = 0;
            int[] Indexes = new int[Mathf.RoundToInt((float)_RemovedObstacleBytes.Count / (float)3)];
            for (int i = 0; i < Indexes.Length; i++)
            {
                Indexes[i] = Number;
                Number += 3;
            }
            for (int r = 0; r < _RemovedObstacleBytes.Count; r++)
            {
                RemovedObstacleTilePositions.Add(_RemovedObstacleBytes[r]);
                if (Indexes.Contains(r))
                    RemovedObstacleVector3s.Add(new Vector3(_RemovedObstacleBytes[r], _RemovedObstacleBytes[r+1], _RemovedObstacleBytes[r+2]));
            }
        }
        List<byte> _BuildingSaveBytes = Load("2");
        if (_BuildingSaveBytes != null && _BuildingSaveBytes.Count > 0)
        {
            BuildingSaveList _BuildingSaves = FromByteArray<BuildingSaveList>(_BuildingSaveBytes.ToArray());
            foreach (BuildingSave _Save in _BuildingSaves.Saves)
            {
                Building BuildingComponent = Instantiate(ObjectPlacement.Instance.Buildings[_Save.Index], _Save.Position, Quaternion.Euler(_Save.Rotation)).GetComponent<Building>();
                BuildingComponent.BuildingId = _Save.BuildingId;
                BuildingComponent.CurrentLevel = _Save.CurrentLevel;
                BuildingComponent.MaxLevel = _Save.MaxLevel;
                BuildingComponent.NoWater = _Save.NoWater;
                BuildingSaves.Saves.Add(_Save);
                if (!MainBuilding)
                    MainBuilding = BuildingComponent;
                BuildingComponent.gameObject.AddComponent<MeshCollider>();
            }
        }
        /*List<byte> _BuildingPositionBytes = Load("2");
        if (_BuildingPositionBytes != null && _BuildingPositionBytes.Count > 0)
        {
            int Number = 0;
            int[] Indexes = new int[Mathf.RoundToInt((float)_BuildingPositionBytes.Count / (float)3)];
            for (int i = 0; i < Indexes.Length; i++)
            {
                Indexes[i] = Number;
                Number += 3;
            }
            for (int r = 0; r < _BuildingPositionBytes.Count; r++)
            {
                BuildingPositions.Add(_BuildingPositionBytes[r]);
                if (Indexes.Contains(r))
                    BuildingVector3s.Add(new Vector3(_BuildingPositionBytes[r], _BuildingPositionBytes[r+1], _BuildingPositionBytes[r+2]));
            }
        }
        List<byte> _BuildingIndexesBytes = Load("3");
        if (_BuildingIndexesBytes != null && _BuildingIndexesBytes.Count > 0)
        {
            for (int r = 0; r < _BuildingIndexesBytes.Count; r++)
            {
                Instantiate(ObjectPlacement.Instance.Buildings[_BuildingIndexesBytes[r]], BuildingVector3s[r] + new Vector3(0, 0.5f, 0), Quaternion.Euler(BuildingQuaternions[r]));
                BuildingIndexes.Add(_BuildingIndexesBytes[r]);
            }
        }*/
        /*List<byte> _SaveBytes = Load("Save");
        if (_SaveBytes != null && _SaveBytes.Count > 0)
        {
            Vector2Int TileOffset = new Vector2Int(Convert.ToInt32(_SaveBytes[0]), Convert.ToInt32(_SaveBytes[1]));
            TileGenerator.Instance.Offset = TileOffset;
            TileGenerator.Instance.RandomOffset = false;
        }
        List<byte> _BuildingBytes = Load("Building");*/
        /*if (_BuildingBytes != null && _BuildingBytes.Count > 0)
        {
            for (int i = 0; i < _BuildingBytes.Count; i++)
            {
                print(_BuildingBytes[i]);
            }
        }*/
        Screen.fullScreen = false;
        /*int ObstacleAmount = 0;
        foreach (int _Index in TileGenerator.Instance.ObstacleIndexes)
        {
            ObstacleAmount += Pool.Instance.PoolObjects[_Index].AllObjects.Count;
        }*/
        /*_Discord = new Discord.Discord(882917401170157570, (System.UInt64)Discord.CreateFlags.Default);
        var DiscordActivityManager = _Discord.GetActivityManager();
        var _DiscordActivity = new Discord.Activity
        {
            //Details = "",
            //State = $"Buildings: {FindObjectsOfType<Building>().Length}\nObstacles: {ObstacleAmount}",
            //State = $"Buildings: {FindObjectsOfType<Building>().Length}\nObstacles: {ObstacleAmount}",
            Timestamps =
            {
                Start = 0,
                //End = 0,
            },
            Assets =
            {
                LargeText = "Build & Upgrade",
                LargeImage = "icon"
            }
        };
        DiscordActivityManager.UpdateActivity(_DiscordActivity, (_Result) => {
            if (_Result == Discord.Result.Ok)
                Debug.Log("Successful");
            else
                Debug.Log("Failed");
        });*/
        //if (!HasSaved)
        //{
            Pool.Instance.Generate();
        //}
    }

    private void Update()
    {
        if (MainBuilding && MainBuilding.CurrentLevel == 1)
        {
            for (int c = 0; c < _Stats.Count; c++)
            {
                _Stats[c] += Buildings[MainBuilding.BuildingId].NeededStats[c];
            }
            MainBuilding.Upgrade(1);
        }
        if (MenAmountTextUI)
            MenAmountTextUI.text = _Stats[0].ToString();
        if (WoodAmountTextUI)
            WoodAmountTextUI.text = _Stats[1].ToString();
        if (FoodAmountTextUI)
            FoodAmountTextUI.text = _Stats[2].ToString();
    }

    public void InformationPanel(bool _Bool)
    {
        BuildingManager.Instance.Panel(_Bool);
        ObjectPlacement.Instance.Panel(!_Bool);
    }

    private void OnApplicationQuit()
    {
        List<byte> _Bytes = new List<byte>();
        _Bytes.Add(Convert.ToByte(TileGenerator.Instance.Offset.x));
        _Bytes.Add(Convert.ToByte(TileGenerator.Instance.Offset.y));
        _Bytes.Add(Convert.ToByte(CameraController.Instance.transform.position.x));
        _Bytes.Add(Convert.ToByte(CameraController.Instance.transform.position.z));
        Save("Default", _Bytes);
        Save("Stats", _Stats.Select(item => Convert.ToByte(item)).ToList());
        Save("1", RemovedObstacleTilePositions.Select(item => Convert.ToByte(item)).ToList());
        List<byte> _BuildingSaveListBytes = new List<byte>();
        BinaryFormatter formatter = new BinaryFormatter();
        SurrogateSelector surrogateSelector = new SurrogateSelector();
        Vector3SerializationSurrogate vector3SS = new Vector3SerializationSurrogate();

        surrogateSelector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), vector3SS);
        formatter.SurrogateSelector = surrogateSelector;
        using (MemoryStream stream = new MemoryStream())
        {
            formatter.Serialize(stream, BuildingSaves);
            _BuildingSaveListBytes = stream.ToArray().ToList();
        }
        Save("2", _BuildingSaveListBytes);
        //Save("2", BuildingPositions.Select(item => Convert.ToByte(item)).ToList());
        //Save("3", BuildingIndexes.Select(item => Convert.ToByte(item)).ToList());
        /*List<byte> _SaveBytes = new List<byte>();
        _SaveBytes.Add(Convert.ToByte(TileGenerator.Instance.Offset.x));//TileOffsetX
        _SaveBytes.Add(Convert.ToByte(TileGenerator.Instance.Offset.y));//TileOffsetY
        Save("Save", _SaveBytes);
        if (MainBuilding)
        {
            List<byte> _BuildingBytes = new List<byte>();
            _BuildingBytes.Add(Convert.ToByte(MainBuilding.BuildingId));
            _BuildingBytes.Add(Convert.ToByte(MainBuilding.CurrentLevel));
            _BuildingBytes.Add(Convert.ToByte(MainBuilding.MaxLevel));
            Save("Building", _BuildingBytes);
        }*/
        //_Discord.Dispose();
    }

    /*public void SaveStats(List<int> _Stats)
    {
        BinaryFormatter _BinaryFormatter = new BinaryFormatter();
        string _Path = Application.persistentDataPath + $"/Stats.save";
        FileStream _FileStream = new FileStream(_Path, FileMode.Create);

        Packet _Packet = new Packet();
        for (int i = 0; i < _Stats.Count; i++)
            _Packet.Write(_Stats[i]);

        _BinaryFormatter.Serialize(_FileStream, _Packet);
        _FileStream.Close();
    }*/

    /*public void Save(Object _Object)
    {
        BinaryFormatter _BinaryFormatter = new BinaryFormatter();
        string _Path = Application.persistentDataPath + $"/{_Object.name}.save";
        FileStream _FileStream = new FileStream(_Path, FileMode.Create);

        //Packet _Packet = new Packet(_Object);

        _BinaryFormatter.Serialize(_FileStream, _Object);
        _FileStream.Close();
        print(_Path);
    }*/

    public void Save(string _Name, List<byte> _Bytes)
    {
        BinaryFormatter _BinaryFormatter = new BinaryFormatter();
        string _Path = Application.persistentDataPath + $"/{_Name}.save";
        FileStream _FileStream = new FileStream(_Path, FileMode.Create);

        _BinaryFormatter.Serialize(_FileStream, _Bytes);
        _FileStream.Close();
        print(_Path);
    }

    public List<byte> Load(string _Name)
    {
        List<byte> _Bytes = null;
        string _Path = Application.persistentDataPath + $"/{_Name}.save";
        if (File.Exists(_Path))
        {
            BinaryFormatter _BinaryFormatter = new BinaryFormatter();
            FileStream _FileStream = new FileStream(_Path, FileMode.Open);
            _Bytes = _BinaryFormatter.Deserialize(_FileStream) as List<byte>;
            _FileStream.Close();
        }
        return _Bytes;
    }

    public byte[] ToByteArray<T>(T obj)
    {
        if (obj == null)
            return null;
        BinaryFormatter bf = new BinaryFormatter();
        using (MemoryStream ms = new MemoryStream())
        {
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }
    }

    public T FromByteArray<T>(byte[] data)
    {
        if (data == null)
            return default(T);
        BinaryFormatter bf = new BinaryFormatter();
        using (MemoryStream ms = new MemoryStream(data))
        {
            SurrogateSelector surrogateSelector = new SurrogateSelector();
            Vector3SerializationSurrogate vector3SS = new Vector3SerializationSurrogate();

            surrogateSelector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), vector3SS);
            bf.SurrogateSelector = surrogateSelector;
            object obj = bf.Deserialize(ms);
            return (T)obj;
        }
    }

    /*public Packet Load(string _Name)
    {
        Packet _Packet = null;
        string _Path = Application.persistentDataPath + $"/{_Name}.save";
        if (File.Exists(_Path))
        {
            BinaryFormatter _BinaryFormatter = new BinaryFormatter();
            FileStream _FileStream = new FileStream(_Path, FileMode.OpenOrCreate);
            MemoryStream _MemoryStream = new();
            _BinaryFormatter.Serialize(_MemoryStream, _BinaryFormatter.Deserialize(_FileStream));
            _Packet = new Packet(_MemoryStream.ToArray());
            _FileStream.Close();
            _MemoryStream.Close();
        }
        return _Packet;
    }*/
}
