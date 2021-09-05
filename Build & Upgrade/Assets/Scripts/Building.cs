using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//[ExecuteAlways]
public class Building : MonoBehaviour
{
    public int BuildingId;
    public int MaxLevel = 7;
    public int CurrentLevel = 0;

    public bool NoWater = true;

    MeshFilter _MeshFilter;
    MeshCollider _MeshCollider;
    Reward _Reward;
    public GameManager.BuildingSave _BuildingSave;

    private void Start()
    {
    }

    private void Update()
    {
        if (CurrentLevel != 0)
        {
            if (_BuildingSave != null)
            {
                _BuildingSave.Index = BuildingId;
                _BuildingSave.Position = transform.position;
                _BuildingSave.Rotation = transform.rotation.eulerAngles;
                _BuildingSave.BuildingId = BuildingId;
                _BuildingSave.CurrentLevel = CurrentLevel;
                _BuildingSave.MaxLevel = MaxLevel;
                _BuildingSave.NoWater = NoWater;
            }
            //_MeshFilter.sharedMesh = FindObjectOfType<GameManager>().Buildings[BuildingId].BuildingMeshes[CurrentLevel - 1];
            if (!_MeshCollider)
                _MeshCollider = GetComponentInChildren<MeshCollider>();
            else
                _MeshCollider.sharedMesh = _MeshFilter.sharedMesh;
            if (!_MeshFilter)
                _MeshFilter = GetComponentInChildren<MeshFilter>();
            else
                _MeshFilter.sharedMesh = GameManager.Instance.Buildings[BuildingId].BuildingMeshes[CurrentLevel - 1];
            if (!_Reward)
                _Reward = GetComponent<Reward>();
            else
                _Reward.GetRewards();
        }
    }

    public bool CanPlace()
    {
        bool[] Checks = new bool[GameManager.Instance._Stats.Count];

        for (int c = 0; c < Checks.Length; c++)
        {
            if (GameManager.Instance._Stats[c] >= GameManager.Instance.Buildings[BuildingId].NeededStats[c])
                Checks[c] = true;
        }
        if (!Checks.Contains(false))
        {
            if (CurrentLevel < MaxLevel)
            {
                return true;
            }
        }
        return false;
    }

    public void Upgrade(int _Amount)
    {
        if (CanPlace())
        {
            CurrentLevel += _Amount;
            for (int c = 0; c < GameManager.Instance._Stats.Count; c++)
            {
                GameManager.Instance._Stats[c] -= GameManager.Instance.Buildings[BuildingId].NeededStats[c];
            }
        }
    }
}
