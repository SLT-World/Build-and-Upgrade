using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour
{
    [SerializeField] float MovementSpeed = 0.5f;
    [SerializeField] float RotationSpeed = 1f;
    [SerializeField] float StopAtDistance = .75f;
    public bool Move;
    public Transform _Target;
    [SerializeField] Vector3 _TargetPosition;
    [SerializeField] Vector3 _Destination;

    [SerializeField] bool StopWhenEnd = true;
    public List<Vector3> Waypoints = new List<Vector3>();
    //public List<Vector3> ToWaypoints = new List<Vector3>();
    //public List<Vector3> BackWaypoints = new List<Vector3>();
    public int CurrentWaypointId;

    public bool IsTo;
    public bool IsBack;

    Rigidbody _Rigidbody;

    private void Start()
    {
        _Rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Move)
        {
            var LookDirection = (_Destination - transform.position).normalized;
            _Rigidbody.MovePosition(Vector3.Lerp(_Rigidbody.position, _Rigidbody.position + transform.forward, MovementSpeed * Time.deltaTime));
            RaycastHit _Hit;
            if (Physics.Raycast(transform.position, transform.forward, out _Hit, 0.75f))
                LookDirection += _Hit.normal * 5 * Time.deltaTime;
            if (Physics.Raycast(transform.position - (transform.right * 0.25f), transform.forward, out _Hit, 0.75f))
                LookDirection += _Hit.normal * 5 * Time.deltaTime;
            if (Physics.Raycast(transform.position + (transform.right * 0.25f), transform.forward, out _Hit, 0.75f))
                LookDirection += _Hit.normal * 5 * Time.deltaTime;
            var LookAtRotation = Quaternion.LookRotation(LookDirection);
            var LookAtRotationOnly_Y = Quaternion.Euler(transform.rotation.eulerAngles.x, LookAtRotation.eulerAngles.y, transform.rotation.eulerAngles.z);
            Quaternion Rotation = Quaternion.Slerp(transform.rotation, LookAtRotationOnly_Y, RotationSpeed * Time.deltaTime);
            _Rigidbody.MoveRotation(Rotation);
            //List<Vector3> _FilteredWaypoints = Waypoints.Where(item => item != _Destination).OrderBy(item => Vector3.Distance(item, transform.position)).ToList();
            /*if (Waypoints.Count > 0 && IsTo)
                FollowWaypoints(_FilteredWaypoints);
            else if (IsBack)
                FollowWaypoints(Waypoints);*/
            if (_Target)
            {
                _TargetPosition = _Target.position;
                if (Vector3.Distance(transform.position, _TargetPosition) <= StopAtDistance)
                {
                    Obstacle _Obstacle = _Target.GetComponent<Obstacle>();
                    if (_Obstacle)
                    {
                        _Obstacle.GetRewards();
                        Destroy(_Obstacle.gameObject);
                    }
                }
                else
                {
                    IsTo = true;
                    IsBack = false;
                }
            }
            else if (IsTo)
            {
                CurrentWaypointId = 0;
                Vector3[] _Waypoints = Waypoints.ToArray();
                Array.Reverse(_Waypoints);
                Waypoints = _Waypoints.ToList();
                IsTo = false;
                IsBack = true;
            }
            if (Vector3.Distance(transform.position, _TargetPosition) <= StopAtDistance)
            {
                _TargetPosition = GameManager.Instance.MainBuilding.transform.position;
                //Astar.Instance.CheckPath(transform.position, GameManager.Instance.MainBuilding.transform.position, this);
                CurrentWaypointId = 0;
                Vector3[] _Waypoints = Waypoints.ToArray();
                Array.Reverse(_Waypoints);
                Waypoints = _Waypoints.ToList();
                IsTo = false;
                IsBack = true;
            }
            if (Waypoints.Count > 0)
                FollowWaypoints(Waypoints);
        }
        /*if (!_Target && _Destination != Vector3.zero)
        {
            if (Vector3.Distance(transform.position, _Destination) <= StopAtDistance)
                Move = false;
            else
                Move = true;
        }*/
    }

    void FollowWaypoints(List<Vector3> _Waypoints)
    {
        try
        {
            if (Vector3.Distance(transform.position, _Waypoints[CurrentWaypointId]) >= 0.75f)
            {
                _Destination = _Waypoints[CurrentWaypointId];
            }
            else if (Vector3.Distance(transform.position, _Waypoints[CurrentWaypointId]) <= 0.75f)
            {
                CurrentWaypointId += 1;
                CurrentWaypointId = CurrentWaypointId >= _Waypoints.Count ? 0 : CurrentWaypointId;
                Move = true;
            }
            else
                _Destination = _Waypoints[CurrentWaypointId];
            if (StopWhenEnd && IsBack)
            {
                if (CurrentWaypointId == _Waypoints.Count - 1)
                    Move = false;
            }
        }
        catch { CurrentWaypointId = 0; }
    }
}
