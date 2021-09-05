using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pool : MonoBehaviour
{
    public static Pool Instance;

    [System.Serializable]
    public class PoolElement 
    {
        public GameObject Prefab;
        public int Amount = 25;
    }

    [System.Serializable]
    public class PoolObject
    {
        public PoolObject(List<GameObject> _GameObjects)
        {
            AllObjects = _GameObjects;
        }

        public List<GameObject> AllObjects;
        //public List<GameObject> Used;
        //public List<GameObject> Reserves;

        /*public GameObject GetUsed()
        {
            try
            {
                //UpdateUsed();
                //UpdateReserves();
                return Used[0];
            }
            catch { return null; }
        }*/
        public GameObject GetReserve()
        {
            for (int i = 0; i < AllObjects.Count; i++)
            {
                if (AllObjects[i] && !AllObjects[i].activeSelf)// && AllObjects[i].transform.position == Vector3.zero
                    return AllObjects[i];
            }
            return null;
        }

        /*public void UpdateUsed()
        {
            Used = AllObjects.Where(item => item.activeSelf).ToList();
        }
        public void UpdateReserves()
        {
            Reserves = AllObjects.Where(item => !item.activeSelf && item.transform.position == Vector3.zero).ToList();
        }*/
    }

    public int MaxReservesNumber;
    public int AllReservesNumber;
    [SerializeField] List<PoolElement> PoolElements;
    public List<PoolObject> PoolObjects;

    bool Done;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!Done)
        {
            MaxReservesNumber = 0;
            foreach (PoolObject _PoolObject in PoolObjects)
            {
                MaxReservesNumber += _PoolObject.AllObjects.Where(item => item && !item.activeSelf && item.transform.position == Vector3.zero).Count();
            }
            Done = true;
        }
        AllReservesNumber = 0;
        foreach (PoolObject _PoolObject in PoolObjects)
        {
            _PoolObject.AllObjects = _PoolObject.AllObjects.Where(item => item).ToList();
            AllReservesNumber += _PoolObject.AllObjects.Where(item => item && !item.activeSelf && item.transform.position == Vector3.zero).Count();
        }
        /*if (AllReservesNumber != 0)
            UpdateObjects();*/
    }

    public void Generate()
    {
        foreach (PoolElement _PoolElement in PoolElements)
        {
            List<GameObject> Clones = new List<GameObject>();
            for (int i = 0; i < _PoolElement.Amount; i++)
            {
                GameObject Clone = Instantiate(_PoolElement.Prefab, Vector3.zero, Quaternion.identity);
                Clone.SetActive(false);
                Clones.Add(Clone);
            }
            PoolObjects.Add(new PoolObject(Clones));
        }
        if (TileGenerator.Instance)
            TileGenerator.Instance.Generate();
        //UpdateObjects();
    }
    
    /*public void UpdateObjects()
    {
        foreach (PoolObject _PoolObjectElement in PoolObjects)
        {
            _PoolObjectElement.UpdateUsed();
            _PoolObjectElement.UpdateReserves();
        }
    }*/
}
