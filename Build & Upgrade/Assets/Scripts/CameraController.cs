using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;
    public static Rect Selection = Rect.zero;

    [Header("Default")]
    [SerializeField] bool ScrollInput = true;
    [SerializeField] bool MouseInput = true;
    [SerializeField] bool KeyInput = true;
    [SerializeField] float Speed = 7.5f;
    [SerializeField] float ZoomSpeed = 25;
    [SerializeField] float RotateSpeed = 0.125f;
    [SerializeField] float MinimumHeight = 5;
    [SerializeField] float MaximumHeight = 50;
    [SerializeField] float SmoothTime = 0.125f;
    [SerializeField] float DeativateObjectDistance = 25f;

    Vector3 smoothMoveVelocity;
    Vector3 MoveAmount;
    Vector3 VMoveAmount;
    Vector3 DragOrigin;
    Vector3 DragTo;
    Vector3 RotationDragOrigin;
    Vector3 RotationDragTo;

    //List<GameObject> _Objects = new List<GameObject>();

    //bool Regenerate = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit _Hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out _Hit, 5000) && _Hit.collider != null)
            {
                Building _Building = _Hit.collider.GetComponentInParent<Building>();
                Obstacle _Obstacle = _Hit.collider.GetComponentInParent<Obstacle>();
                if (_Building)
                {
                    BuildingManager.Instance.BuildingTarget = _Building;
                    GameManager.Instance.InformationPanel(true);
                }
                if (_Obstacle)
                {
                    if (GameManager.Instance.MainBuilding)
                    {
                        /*Astar.Instance.Generate();
                        Astar.Instance.CheckPath(GameManager.Instance.MainBuilding.transform.position, _Obstacle.transform.position);
                        Agent[] _Agents = FindObjectsOfType<Agent>();
                        foreach (Agent _Agent in _Agents)
                        {
                            _Agent._Target = _Obstacle.transform;
                        }*/
                        Astar.Instance.Generate();
                        Astar.Instance.CheckPath(GameManager.Instance.MainBuilding.transform.position, _Obstacle.transform.position);
                        Vector3 _RoundEndPosition = new Vector3(Mathf.Round(_Obstacle.transform.position.x), Mathf.Round(_Obstacle.transform.position.y), Mathf.Round(_Obstacle.transform.position.z));
                        //bool _Found = false;
                        Vector3[] _CheckedPositions = Astar.Instance._Nodes.Where(item => item.IsChecked).Select(item => item.NodePosition).ToArray();
                        /*Vector3[] _WayPositions = Astar.Instance._Path.ToArray();
                        if (!_Found)
                        {*/
                        for (int i = 0; i < _CheckedPositions.Length; i++)
                        {
                            _RoundEndPosition.y = _CheckedPositions[i].y;
                            if (_RoundEndPosition == _CheckedPositions[i] && _Obstacle._Reward && _Obstacle._Reward.CanGet)
                            {
                                //_Found = false;
                                _Obstacle.GetRewards();
                                GameManager.Instance.RemovedObstacleTilePositions.Add(Mathf.RoundToInt(_Obstacle.transform.position.x));
                                GameManager.Instance.RemovedObstacleTilePositions.Add(0);
                                GameManager.Instance.RemovedObstacleTilePositions.Add(Mathf.RoundToInt(_Obstacle.transform.position.z));
                                Destroy(_Obstacle.gameObject);
                                break;
                            }
                                /*else
                                    _Found = false;*/
                        }
                        /*}
                        if (!_Found)
                        {
                            for (int i = 0; i < _WayPositions.Length; i++)
                            {
                                _RoundEndPosition.y = _WayPositions[i].y;
                                if (_RoundEndPosition == _WayPositions[i])
                                {
                                    _Found = false;
                                    _Obstacle.GetRewards();
                                    Destroy(_Obstacle.gameObject);
                                    break;
                                }
                                else
                                    _Found = false;
                            }
                        }*/
                    }
                }
            }
        }
        if (ScrollInput)
            HandleScrollInput();
        if (MouseInput)
            HandleMouseInput();
        if (KeyInput)
            HandleKeyInput();
    }

    private void FixedUpdate()
    {
        /*if (MoveAmount != Vector3.zero)
        {
            if (TileGenerator.Instance)
                TileGenerator.Instance.Generate();
        }*/
        Rigidbody[] _Rigidbodies = ExtendedTools.FindObjectsOfTypeIncludingDisabled<Rigidbody>();
        foreach (GameObject _GameObject in _Rigidbodies.Where(item => item.transform.position != Vector3.zero).Select(item => item.gameObject).ToList())
        {
            bool IsNear = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), _GameObject.transform.position) < DeativateObjectDistance;
            if (IsNear)
                _GameObject.SetActive(true);
            else
            {
                _GameObject.SetActive(false);
            }
        }
        /*_Objects = _Objects.Where(item => item).ToList();
        foreach (GameObject _GameObject in FindObjectsOfType<Rigidbody>().Select(item => item.gameObject).ToList())
        {
            if (!_Objects.Contains(_GameObject))
            {
                _Objects.Add(_GameObject);
            }
        }
        foreach (GameObject _GameObject in _Objects)
        {
            bool IsNear = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), _GameObject.transform.position) < DeativateObjectDistance;
            if (IsNear)
                _GameObject.SetActive(true);
            else
            {
                //if (_GameObject.activeSelf)
                //    Regenerate = true;
                _GameObject.SetActive(false);
            }
        }*/
        /*if (Regenerate)
        {
            TileGenerator.Instance.Offset = new Vector2(transform.position.x, transform.position.z);
            TileGenerator.Instance.Generate();
            Regenerate = false;
        }*/
        transform.position += MoveAmount * Time.fixedDeltaTime;
        transform.position += VMoveAmount * Time.fixedDeltaTime;
    }

    void HandleScrollInput()
    {
        float Scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Physics.Raycast(transform.position, -transform.up, 5) && Scroll < 0)
        {
            Scroll = 0;
            transform.position += Vector3.SmoothDamp(MoveAmount, new Vector3(0, 0.75f, 0) * Speed, ref smoothMoveVelocity, SmoothTime) * Time.fixedDeltaTime;
        }
        else if (transform.position.y > MaximumHeight && Scroll > 0)
        {
            Scroll = 0;
        }
        else if (transform.position.y < MinimumHeight && Scroll < 0)
        {
            Scroll = 0;
        }

        Vector3 VDirection = new Vector3(0, Scroll, 0).normalized;
        VMoveAmount = Vector3.SmoothDamp(VMoveAmount, transform.TransformDirection(VDirection) * ZoomSpeed, ref smoothMoveVelocity, SmoothTime);
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Plane _Plane = new Plane(Vector3.up, Vector3.zero);

            Ray _Ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float _Entry;

            if (_Plane.Raycast(_Ray, out _Entry))
            {
                DragOrigin = _Ray.GetPoint(_Entry);
            }
        }
        if (Input.GetMouseButton(0))
        {
            Plane _Plane = new Plane(Vector3.up, Vector3.zero);

            Ray _Ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float _Entry;

            if (_Plane.Raycast(_Ray, out _Entry))
            {
                DragTo = _Ray.GetPoint(_Entry);

                Vector3 Direction = (transform.position + DragOrigin - DragTo).normalized;
                MoveAmount = Vector3.SmoothDamp(MoveAmount, transform.TransformDirection(Direction) * Speed, ref smoothMoveVelocity, SmoothTime);
            }
        }
    }
    void HandleKeyInput()
    {
        float Horizontal = Input.GetAxisRaw("Horizontal");
        float Vertical = Input.GetAxisRaw("Vertical");
        if (Physics.Raycast(transform.position, transform.forward, 5) && Vertical > 0)
        {
            Vertical = 0;
            //transform.position += Vector3.SmoothDamp(MoveAmount, new Vector3(0, 0.75f, 0) * Speed, ref smoothMoveVelocity, SmoothTime) * Time.fixedDeltaTime;
        }
        if (Physics.Raycast(transform.position, -transform.forward, 5) && Vertical < 0)
        {
            Vertical = 0;
            //transform.position += Vector3.SmoothDamp(MoveAmount, new Vector3(0, 0.75f, 0) * Speed, ref smoothMoveVelocity, SmoothTime) * Time.fixedDeltaTime;
        }
        if (Physics.Raycast(transform.position, -transform.right, 5) && Horizontal < 0)
        {
            Horizontal = 0;
            //transform.position += Vector3.SmoothDamp(MoveAmount, new Vector3(0.75f, 0, 0) * Speed, ref smoothMoveVelocity, SmoothTime) * Time.fixedDeltaTime;
        }
        if (Physics.Raycast(transform.position, transform.right, 5) && Horizontal > 0)
        {
            Horizontal = 0;
            //transform.position += Vector3.SmoothDamp(MoveAmount, new Vector3(-0.75f, 0, 0) * Speed, ref smoothMoveVelocity, SmoothTime) * Time.fixedDeltaTime;
        }
        Vector3 Direction = new Vector3(Horizontal, 0, Vertical).normalized;
        MoveAmount = Vector3.SmoothDamp(MoveAmount, transform.TransformDirection(Direction) * Speed, ref smoothMoveVelocity, SmoothTime);
        if (Input.GetMouseButtonDown(2))
        {
            RotationDragOrigin = Input.mousePosition;
        }
        if (Input.GetMouseButton(2))
        {
            RotationDragTo = Input.mousePosition;

            float x = (RotationDragOrigin - RotationDragTo).x * RotateSpeed;
            float y = (RotationDragTo - RotationDragOrigin).y * RotateSpeed;
            transform.rotation *= Quaternion.Euler(new Vector3(0, x, 0));

            RotationDragOrigin = RotationDragTo;
        }
    }

    private void OnGUI()
    {
        /*GUI.color = Color.gray;
        GUI.backgroundColor = Color.white;
        if (_StartClick != -Vector3.one)
            GUI.Box(Selection, GUIContent.none);*/
        //GUI.DrawTexture(Selection, SelectionHighlight);
    }
}
