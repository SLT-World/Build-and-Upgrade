using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TileGenerator : MonoBehaviour
{
    public static TileGenerator Instance;

    [SerializeField] int PoolingIndex;
    public int[] ObstacleIndexes;
    public int Amount = 60;

    public float Refinement = 0.1f;

    //public Vector2Int Offset;

    public Vector3 ObstacleOffset = new Vector3(0, 0.5f, 0);
    public Vector2Int Offset;
    public bool RandomOffset = true;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        /*for (int i = 0; i < Amount; i++)
        {
            for (int j = 0; j < Amount; j++)
            {
                float Multiplier = 10;
                float Noise = Mathf.PerlinNoise(i * Refinement, j * Refinement);
                GameObject _GameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _GameObject.transform.position = new Vector3(i, Noise * Multiplier, j);
            }
        }*/
    }

    public void Generate()
    {
        if (RandomOffset)
        {
            Offset = new Vector2Int(Random.Range(0, Amount / 4), Random.Range(0, Amount / 4));
        }
        for (int x = Offset.x; x < Amount - Offset.x; x++)
        {
            for (int z = Offset.y; z < Amount - Offset.y; z++)
            {
                float _Noise = Mathf.PerlinNoise(x * Refinement, z * Refinement);
                Vector3 _Position = new Vector3(x - Offset.x, 0, z - Offset.y);
                Collider[] _Colliders = Physics.OverlapSphere(_Position, 0.25f, 1 << LayerMask.NameToLayer("Tile"));
                if (_Noise > 0.5 && _Colliders.Length == 0)
                {
                    GameObject _Tile = Pool.Instance.PoolObjects[PoolingIndex].GetReserve();
                    if (_Tile)
                    {
                        _Tile.transform.position = _Position;
                        _Tile.SetActive(true);
                        float _ObstacleNoise = Mathf.PerlinNoise((x + 5 * Refinement) / 10, (z + 5 * Refinement) / 10);
                        //int _ObstacleChance = Random.Range(0, 100);
                        bool _IsRemovedObstacle = false;
                        foreach (Vector3 _Vector3 in GameManager.Instance.RemovedObstacleVector3s)
                        {
                            //print(new Vector3(_Tile.transform.position.x, _Tile.transform.position.y, _Tile.transform.position.z));
                            //print(Vector3.Distance(_Vector3, new Vector3(_Tile.transform.position.x, _Vector3.y, _Tile.transform.position.z)));
                            _IsRemovedObstacle = IsNear(_Vector3, new Vector3(_Tile.transform.position.x, _Tile.transform.position.y, _Tile.transform.position.z));
                            if (_IsRemovedObstacle)
                            {
                                break;
                            }
                        }
                        if (ObstacleIndexes.Length > 0 && _ObstacleNoise > 0.45 && !_IsRemovedObstacle)
                        {
                            //int ObstacleIndex = Random.Range(0, ObstacleIndexes.Length - 1);
                            int Index = Mathf.RoundToInt((_Noise * ObstacleIndexes.Length / (float)ObstacleIndexes.Length * (float)ObstacleIndexes.Length - 1));
                            GameObject _Obstacle = Pool.Instance.PoolObjects[ObstacleIndexes[Index]].GetReserve();
                            if (_Obstacle)
                            {
                                Vector3 _Offset = new Vector3(Random.Range(-0.25f, 0.25f), 0, Random.Range(-0.25f, 0.25f)) + ObstacleOffset;
                                Vector3 _Rotation = new Vector3(Random.Range(0, 5f), Random.Range(0, 360f), Random.Range(0, 5f));
                                //Vector3 _Offset = new Vector3(_ObstacleNoise, 0, _ObstacleNoise) + ObstacleOffset;
                                //Vector3 _Rotation = new Vector3(_ObstacleNoise * 10, _ObstacleNoise * 75, _ObstacleNoise * 10);
                                _Obstacle.transform.position = _Position + _Offset;
                                _Obstacle.transform.eulerAngles = _Rotation;
                                _Obstacle.SetActive(true);
                            }
                        }
                        //NavMeshBaker.Instance.NavMeshSurfaces.Add(_Tile.GetComponent<NavMeshSurface>());
                    }
                }
            }
        }
        //Astar.Instance.Generate();
        /*for (int x = Offset.x; x < Amount - Offset.x; x++)
        {
            for (int z = Offset.y; z < Amount - Offset.y; z++)
            {
                Vector3 _Position = new Vector3(x - Offset.x, 0, z - Offset.y);
                float _ObstacleChance = Mathf.PerlinNoise((x + 5 * Refinement) / 10, (z + 5 * Refinement) / 10);
                //int _ObstacleChance = Random.Range(0, 100);
                if (ObstacleIndexes.Length > 0 && _ObstacleChance > 0.45)
                {
                    Collider[] _Colliders = Physics.OverlapSphere(_Position, 0.25f, 1 << LayerMask.NameToLayer("Tile"));
                    if (_Colliders.Length > 0)
                    {
                        int ObstacleIndex = Random.Range(0, ObstacleIndexes.Length - 1);
                        GameObject _Obstacle = Pool.Instance.PoolObjects[ObstacleIndexes[ObstacleIndex]].GetReserve();
                        if (_Obstacle)
                        {
                            Vector3 _Offset = new Vector3(Random.Range(-0.25f, 0.25f), 0, Random.Range(-0.25f, 0.25f)) + ObstacleOffset;
                            Vector3 _Rotation = new Vector3(Random.Range(0, 5f), Random.Range(0, 360f), Random.Range(0, 5f));
                            _Obstacle.transform.position = _Position + _Offset;
                            _Obstacle.transform.eulerAngles = _Rotation;
                            _Obstacle.SetActive(true);
                        }
                    }
                }
            }
        }*/
        //NavMeshBaker.Instance.Generate();
        //Grid.Instance.CreateGrid();
    }

    bool IsNear(Vector3 _PositionA, Vector3 _PositionB)
    {
        return Vector3.Distance(_PositionA, _PositionB) <= 0.5;
    }
}
