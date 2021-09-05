using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Astar : MonoBehaviour
{
    public static Astar Instance;

    public class Node
    {
        public Vector3 NodePosition;

        public Node Parent;

        public bool Accessible;
        public bool IsChecked;

        public List<Vector3> NodePositions = new List<Vector3>();
        public List<Node> Nodes = new List<Node>();
    }

    [SerializeField] float Row = 60;
    [SerializeField] float Column = 60;
    [SerializeField] float Distance = 1;

    [SerializeField] LayerMask TileMask;
    [SerializeField] Color GridAccessibleColor;
    [SerializeField] Color GridFillColor;
    [SerializeField] Color GridPathColor;
    [SerializeField] Color GridInaccessibleColor;

    public List<Vector3> _Positions = new List<Vector3>();
    public List<Vector3> _Path = new List<Vector3>();
    public List<Vector3> _AgentPath = new List<Vector3>();
    List<RaycastHit> _Hits = new List<RaycastHit>();
    public List<bool> _Accessibles = new List<bool>();
    public List<Node> _Nodes = new List<Node>();

    public bool ReachedDestination;

    private void Awake()
    {
        Instance = this;
    }

    GameObject[] FindGameObjectsWithLayer(int layer)
    {
        var goArray = FindObjectsOfType<GameObject>();
        var goList = new List<GameObject>();
        for (var i = 0; i < goArray.Length; i++)
        {
            if (goArray[i].layer == layer)
            {
                goList.Add(goArray[i]);
            }
        }
        if (goList.Count == 0)
        {
            return null;
        }
        return goList.ToArray();
    }

    public void Generate()
    {
        Rigidbody[] _Rigidbodies = ExtendedTools.FindObjectsOfTypeIncludingDisabled<Rigidbody>();
        foreach (GameObject _GameObject in _Rigidbodies.Where(item => item.transform.position != Vector3.zero && item.gameObject.layer != 1 << LayerMask.NameToLayer("Obstacle")).Select(item => item.gameObject))
            _GameObject.SetActive(true);
        foreach (GameObject _GameObject in ExtendedTools.FindObjectsOfTypeIncludingDisabled<Obstacle>().Select(item => item.gameObject))
        {
            SetLayer(_GameObject, LayerMask.NameToLayer("Ignore Raycast"), true);
        }
        _Positions.Clear();
        _Hits.Clear();
        _Accessibles.Clear();
        _Nodes.Clear();
        _Path.Clear();
        Vector2 Table;
        Table.x = 0;
        Table.y = 0;
        for (int x = 0; x < Row; x++)
        {
            Table.x += Distance;
            Table.y = 0;
            for (int y = 0; y < Column; y++)
            {
                Table.y += Distance;
                RaycastHit _Hit;
                if (Physics.Raycast(transform.position - (-transform.right * Table.y) - (-transform.forward * Table.x), Vector3.down, out _Hit, 1 << LayerMask.NameToLayer("Obstacle")))
                {
                    _Hits.Add(_Hit);
                    _Positions.Add(_Hit.point);
                    bool IsAccessible = false;
                    if (Physics.CheckSphere(_Hit.point, 0.25f, TileMask))
                        IsAccessible = true;
                    _Accessibles.Add(IsAccessible);
                }
            }
        }
        foreach (Vector3 _Position in _Positions)
        {
            int _Index = _Positions.IndexOf(_Position);
            bool _Accessible;

            if (_Accessibles[_Index])
                _Accessible = true;
            else
                _Accessible = false;

            Node _Node = new Node();
            List<Action> _Actions = new List<Action>();
            _Node.NodePosition = _Positions[_Index];
            Vector3 Up = _Positions[_Index] + Vector3.forward + Vector3.up;
            Vector3 Down = _Positions[_Index] + -Vector3.forward + Vector3.up;
            RaycastHit _Hit1;
            if (Physics.Raycast(Up, Vector3.down, out _Hit1, 1 << LayerMask.NameToLayer("Obstacle")))
                _Actions.Add(() => _Node.NodePositions.Add(_Positions[_Positions.IndexOf(_Hit1.point)]));
            RaycastHit _Hit2;
            if (Physics.Raycast(Down, Vector3.down, out _Hit2, 1 << LayerMask.NameToLayer("Obstacle")))
                _Actions.Add(() => _Node.NodePositions.Add(_Positions[_Positions.IndexOf(_Hit2.point)]));
            _Actions.Add(() => _Node.NodePositions.Add(_Positions[_Index + 1]));
            _Actions.Add(() => _Node.NodePositions.Add(_Positions[_Index - 1]));
            _Node.Accessible = _Accessible;
            foreach (Action _Action in _Actions)
            {
                try
                {
                    _Action();
                }
                catch { }
            }
            if (!_Nodes.Contains(_Node))
                _Nodes.Add(_Node);
        }
        foreach (GameObject _GameObject in ExtendedTools.FindObjectsOfTypeIncludingDisabled<Obstacle>().Select(item => item.gameObject))
        {
            SetLayer(_GameObject, LayerMask.NameToLayer("Obstacle"), true);
        }
        /*for (int x = TileGenerator.Instance.Offset.x; x < TileGenerator.Instance.Amount - TileGenerator.Instance.Offset.x; x++)
        {
            for (int z = TileGenerator.Instance.Offset.y; z < TileGenerator.Instance.Amount - TileGenerator.Instance.Offset.y; z++)
            {
                Vector3 _Position = new Vector3(x - TileGenerator.Instance.Offset.x, 0, z - TileGenerator.Instance.Offset.y);
                float _ObstacleChance = Mathf.PerlinNoise((x + 5 * TileGenerator.Instance.Refinement) / 10, (z + 5 * TileGenerator.Instance.Refinement) / 10);
                //int _ObstacleChance = Random.Range(0, 100);
                if (TileGenerator.Instance.ObstacleIndexes.Length > 0 && _ObstacleChance > 0.45)
                {
                    Collider[] _Colliders = Physics.OverlapSphere(_Position, 0.25f, 1 << LayerMask.NameToLayer("Tile"));
                    if (_Colliders.Length > 0)
                    {
                        int ObstacleIndex = UnityEngine.Random.Range(0, TileGenerator.Instance.ObstacleIndexes.Length - 1);
                        GameObject _Obstacle = Pool.Instance.PoolObjects[TileGenerator.Instance.ObstacleIndexes[ObstacleIndex]].GetReserve();
                        if (_Obstacle)
                        {
                            Vector3 _Offset = new Vector3(UnityEngine.Random.Range(-0.25f, 0.25f), 0, UnityEngine.Random.Range(-0.25f, 0.25f)) + TileGenerator.Instance.ObstacleOffset;
                            Vector3 _Rotation = new Vector3(UnityEngine.Random.Range(0, 5f), UnityEngine.Random.Range(0, 360f), UnityEngine.Random.Range(0, 5f));
                            _Obstacle.transform.position = _Position + _Offset;
                            _Obstacle.transform.eulerAngles = _Rotation;
                            _Obstacle.SetActive(true);
                        }
                    }
                }
            }
        }*/
    }

    void SetLayer(GameObject _GameObject, LayerMask _Layer, bool _SetChild)
    {
        _GameObject.layer = _Layer.value;
        if (_SetChild)
        {
            foreach (GameObject _Child in _GameObject.GetComponentsInChildren<Transform>(true).Select(item => item.gameObject))
            {
                _Child.layer = _Layer.value;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        foreach (Vector3 _Position in _Positions)
        {
            int _Index = _Positions.IndexOf(_Position);

            if (!_Path.Contains(_Position))
            {
                if (!_Nodes[_Index].IsChecked)
                {
                    if (_Accessibles[_Index])
                        Gizmos.color = GridAccessibleColor;
                    else
                        Gizmos.color = GridInaccessibleColor;
                }
                else
                    Gizmos.color = GridFillColor;
            }
            else
                Gizmos.color = GridPathColor;
            Gizmos.DrawCube(_Position, new Vector3(1, 0.0001f, 1));
        }
    }

    public void CheckPath(Vector3 _StartPosition, Vector3 _EndPosition)
    {
        _Path.Clear();
        ReachedDestination = false;
        foreach (Node _Node in _Nodes)
        {
            _Node.IsChecked = false;
            _Node.Parent = null;
        }
        Vector3 _Position = new Vector3(Mathf.Round(_StartPosition.x), Mathf.Round(_StartPosition.y), Mathf.Round(_StartPosition.z));
        Vector3 _RoundEndPosition = new Vector3(Mathf.Round(_EndPosition.x), Mathf.Round(_EndPosition.y), Mathf.Round(_EndPosition.z));
        for (int i = 0; i < _Positions.Count; i++)
        {
            _Position.y = _Positions[i].y;
            if (_Position == _Positions[i])
            {
                _RoundEndPosition.y = _Positions[i].y;
                CheckAvailable(_Nodes[i], _RoundEndPosition, null);
                break;
            }
        }
        /*List<Vector3> _ToPath = _Path;
        Vector3[] _ReversedPath = _Path.ToArray();
        //List<Vector3> _BackPath = _ReversedPath;
        _AgentPath = _ToPath;
        Array.Reverse(_ReversedPath);
        foreach (Vector3 _PathPosition in _ReversedPath.ToList())
            _AgentPath.Add(_PathPosition);*/
        //_BackPath.ForEach(item => _AgentPath.Add(item));
        /*Agent[] _Agents = FindObjectsOfType<Agent>();
        foreach (Agent _Agent in _Agents)
        {
            if (_Path.Count != 0)
            {
                _Agent.CurrentWaypointId = 0;
                _Agent.Waypoints = _Path;
                _Agent.Move = true;
            }
        }*/
    }

    public void CheckPath(Vector3 _StartPosition, Vector3 _EndPosition, Agent _Agent)
    {
        _Path.Clear();
        ReachedDestination = false;
        foreach (Node _Node in _Nodes)
        {
            _Node.IsChecked = false;
            _Node.Parent = null;
        }
        Vector3 _Position = new Vector3(Mathf.Round(_StartPosition.x), Mathf.Round(_StartPosition.y), Mathf.Round(_StartPosition.z));
        Vector3 _RoundEndPosition = new Vector3(Mathf.Round(_EndPosition.x), Mathf.Round(_EndPosition.y), Mathf.Round(_EndPosition.z));
        for (int i = 0; i < _Positions.Count; i++)
        {
            _Position.y = _Positions[i].y;
            if (_Position == _Positions[i])
            {
                _RoundEndPosition.y = _Positions[i].y;
                CheckAvailable(_Nodes[i], _RoundEndPosition, null);
                break;
            }
        }
        /*List<Vector3> _ToPath = _Path;
        Vector3[] _ReversedPath = _Path.ToArray();
        //List<Vector3> _BackPath = _ReversedPath;
        _AgentPath = _ToPath;
        Array.Reverse(_ReversedPath);
        foreach (Vector3 _PathPosition in _ReversedPath.ToList())
            _AgentPath.Add(_PathPosition);*/
        //_BackPath.ForEach(item => _AgentPath.Add(item));
        if (_Path.Count != 0)
        {
            _Agent.CurrentWaypointId = 0;
            _Agent.Waypoints = _Path;
            _Agent.Move = true;
        }
    }

    public void CheckAvailable(Node _Node, Vector3 _EndPosition, Node _Parent)
    {
        _Node.Parent = _Parent;
        _Node.IsChecked = true;
        Vector3 _RoundEndPosition = new Vector3(Mathf.Round(_EndPosition.x), Mathf.Round(_EndPosition.y), Mathf.Round(_EndPosition.z));
        List<Vector3> _NodePositions = _Node.NodePositions.OrderBy(item => Vector3.Distance(item, _RoundEndPosition)).ToList();
        for (int i = 0; i < _NodePositions.Count; i++)
        {
            Vector3 _NodePosition = _NodePositions[i];
            Node _NeighbourNode = null;
            for (int p = 0; p < _Positions.Count; p++)
            {
                _NodePosition.y = _Positions[p].y;
                if (_NodePosition == _Positions[p])
                {
                    _NeighbourNode = _Nodes[p];
                    _Positions[p] = _NodePosition;
                    break;
                }
                else
                    continue;
            }
            if (!_NeighbourNode.IsChecked && _NeighbourNode.Accessible)
                CheckAvailable(_NeighbourNode, _EndPosition, _Node);
        }
        if (IsNear(_RoundEndPosition, _Node.NodePosition))
        {
            TraceToOrigin(_Node);
        }
        /*if (!IsNear(_RoundEndPosition, _Node.NodePosition))
        {
            List<Vector3> _NodePositions = _Node.NodePositions.OrderBy(item => Vector3.Distance(item, _RoundEndPosition)).ToList();
            foreach (Vector3 _NodePosition in _NodePositions)
            {
                Node _NeighbourNode = null;
                for (int p = 0; p < _Positions.Count; p++)
                {
                    if (_NodePosition == _Positions[p])
                    {
                        _NeighbourNode = _Nodes[p];
                        break;
                    }
                    else
                        continue;
                }
                if (!_NeighbourNode.IsChecked && _NeighbourNode.Accessible)
                    CheckAvailable(_NeighbourNode, _EndPosition, _Node);
            }
        }
        else
        {
            TraceToOrigin(_Node);
        }*/
    }

    public void TraceToOrigin(Node _Node)
    {
        _Path.Add(_Node.NodePosition);
        if (_Node.Parent != null)
        {
            ReachedDestination = false;
            TraceToOrigin(_Node.Parent);
        }
        else
        {
            ReachedDestination = true;
            _Path.Reverse();
        }
    }

    bool IsNear(Vector3 _PositionA, Vector3 _PositionB)
    {
        return Vector3.Distance(_PositionA, _PositionB) <= 0.5;
    }
}
