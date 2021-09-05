using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ObjectPlacement : MonoBehaviour
{
    public static ObjectPlacement Instance;

    [System.Serializable]
    class RendererElement
    {
        public RendererElement(Renderer _Renderer, List<Material> _Materials)
        {
            this._Renderer = _Renderer;
            Materials = _Materials;
        }

        public Renderer _Renderer;
        public List<Material> Materials;
    }

    public Transform TargetObject;
    [SerializeField] Transform WorkshopPanel;
    [SerializeField] GameObject SlotPanelPrefab;

    public List<GameObject> Buildings;
    public List<bool> MainBuildings;

    [Header("Default")]
    [SerializeField] Material BlueprintMaterial;
    [SerializeField] Material BlueprintCannotPlaceMaterial;

    [Header("Object")]
    [SerializeField] List<RendererElement> RendererElements;

    float MouseWheelRotation;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        RuntimePreviewGenerator.BackgroundColor = new Color(0, 0, 0, 0);
        if (SlotPanelPrefab && WorkshopPanel)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            float oldIntensity = RenderSettings.ambientIntensity;
            RenderSettings.ambientIntensity = 1f;
            foreach (GameObject _Object in Buildings)
            {
                if (MainBuildings[Buildings.IndexOf(_Object)])
                {
                    Transform Slot = Instantiate(SlotPanelPrefab, WorkshopPanel).transform;
                    Texture2D ModelPreviewTexture = RuntimePreviewGenerator.GenerateModelPreview(_Object.transform, Mathf.RoundToInt(256), Mathf.RoundToInt(256));

                    Slot.GetChild(0).GetComponent<Image>().sprite = Sprite.Create(ModelPreviewTexture, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f), 75);
                    Slot.gameObject.AddComponent<Button>().onClick.AddListener(() => PlaceObject(_Object));
                }
            }
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
            RenderSettings.ambientIntensity = oldIntensity;
        }
        /*RuntimePreviewGenerator.BackgroundColor = new Color(0, 0, 0, 0);
        if (SlotPanelPrefab && WorkshopPanel)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            float oldIntensity = RenderSettings.ambientIntensity;
            RenderSettings.ambientIntensity = 1f;
            foreach (GameObject _Object in Buildings)
            {
                Transform Slot = Instantiate(SlotPanelPrefab, WorkshopPanel).transform;
                Texture2D ModelPreviewTexture = RuntimePreviewGenerator.GenerateModelPreview(_Object.transform, Mathf.RoundToInt(256), Mathf.RoundToInt(256));

                Slot.GetChild(0).GetComponent<Image>().sprite = Sprite.Create(ModelPreviewTexture, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f), 75);
                Slot.gameObject.AddComponent<Button>().onClick.AddListener(() => PlaceObject(_Object));
            }
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
            RenderSettings.ambientIntensity = oldIntensity;
        }*/
    }

    void Update()
    {
        if (GameManager.Instance.MainBuilding)
        {
            RuntimePreviewGenerator.BackgroundColor = new Color(0, 0, 0, 0);
            if (SlotPanelPrefab && WorkshopPanel)
            {
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                float oldIntensity = RenderSettings.ambientIntensity;
                RenderSettings.ambientIntensity = 1f;
                foreach (GameObject _Object in Buildings)
                {
                    try
                    {
                        if (!MainBuildings[Buildings.IndexOf(_Object)] && !WorkshopPanel.GetChild(Buildings.IndexOf(_Object)))
                        {
                            Transform Slot = Instantiate(SlotPanelPrefab, WorkshopPanel).transform;
                            Texture2D ModelPreviewTexture = RuntimePreviewGenerator.GenerateModelPreview(_Object.transform, Mathf.RoundToInt(256), Mathf.RoundToInt(256));

                            Slot.GetChild(0).GetComponent<Image>().sprite = Sprite.Create(ModelPreviewTexture, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f), 75);
                            Slot.gameObject.AddComponent<Button>().onClick.AddListener(() => PlaceObject(_Object));
                        }
                    }
                    catch
                    {
                        if (!MainBuildings[Buildings.IndexOf(_Object)])
                        {
                            Transform Slot = Instantiate(SlotPanelPrefab, WorkshopPanel).transform;
                            Texture2D ModelPreviewTexture = RuntimePreviewGenerator.GenerateModelPreview(_Object.transform, Mathf.RoundToInt(256), Mathf.RoundToInt(256));

                            Slot.GetChild(0).GetComponent<Image>().sprite = Sprite.Create(ModelPreviewTexture, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f), 75);
                            Slot.gameObject.AddComponent<Button>().onClick.AddListener(() => PlaceObject(_Object));
                        }
                    }
                }
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
                RenderSettings.ambientIntensity = oldIntensity;
            }
        }
        RaycastHit _Hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out _Hit, Mathf.Infinity))
        {
            if (TargetObject)
            {
                TargetObject.position = new Vector3(Mathf.Round(_Hit.point.x), Mathf.Round(_Hit.point.y / 0.5f) * 0.5f, Mathf.Round(_Hit.point.z));
                MouseWheelRotation = Input.mouseScrollDelta.y;
                TargetObject.Rotate(Vector3.up, MouseWheelRotation * 45f);
                Building _Building = TargetObject.GetComponent<Building>();
                if (_Building)
                {
                    LayerMask _Layer = 1 << LayerMask.NameToLayer("Obstacle");
                    if (_Building.NoWater)
                        _Layer = 1 << LayerMask.NameToLayer("Water") | 1 << LayerMask.NameToLayer("Obstacle");
                    Collider[] _Colliders = Physics.OverlapSphere(TargetObject.position, 0.45f, _Layer);
                    if (_Colliders.Length == 0)
                    {
                        if (_Building.CanPlace())
                        {
                            foreach (RendererElement _RendererElement in RendererElements)
                            {
                                List<Material> _Materials = _RendererElement._Renderer.materials.ToList();
                                var ChangedMaterials = new Material[_Materials.Count];
                                for (var i = 0; i < _Materials.Count; i++)
                                    ChangedMaterials[i] = BlueprintMaterial;
                                _RendererElement._Renderer.materials = ChangedMaterials;
                            }
                            if (Input.GetMouseButtonDown(0))
                            {
                                _Building.Upgrade(1);
                                if (_Building.CurrentLevel > 0)
                                {
                                    GameManager.BuildingSave _BuildingSave = new GameManager.BuildingSave();
                                    _Building._BuildingSave = _BuildingSave;
                                    GameManager.Instance.BuildingSaves.Saves.Add(_BuildingSave);
                                    /*GameManager.Instance.BuildingIndexes.Add(_Building.BuildingId);
                                    GameManager.Instance.BuildingPositions.Add(Mathf.RoundToInt(TargetObject.position.x));
                                    GameManager.Instance.BuildingPositions.Add(Mathf.RoundToInt(TargetObject.position.y));
                                    GameManager.Instance.BuildingPositions.Add(Mathf.RoundToInt(TargetObject.position.z));
                                    GameManager.Instance.BuildingRotations.Add(Mathf.RoundToInt(TargetObject.rotation.x));
                                    GameManager.Instance.BuildingRotations.Add(Mathf.RoundToInt(TargetObject.rotation.y));
                                    GameManager.Instance.BuildingRotations.Add(Mathf.RoundToInt(TargetObject.rotation.z));*/
                                    RendererElements.ForEach(item => item._Renderer.materials = item.Materials.ToArray());
                                    List<MeshFilter> MeshFilters = TargetObject.GetComponentsInChildren<MeshFilter>().ToList();
                                    MeshFilters.ForEach(item => { if (!item.GetComponent<MeshCollider>()) item.gameObject.AddComponent<MeshCollider>().sharedMesh = item.mesh; });
                                    RendererElements.Clear();
                                    TargetObject = null;
                                }
                                if (!GameManager.Instance.MainBuilding)
                                    GameManager.Instance.MainBuilding = _Building;
                            }
                        }
                        if (!_Building.CanPlace())
                        {
                            foreach (RendererElement _RendererElement in RendererElements)
                            {
                                List<Material> _Materials = _RendererElement._Renderer.materials.ToList();
                                var ChangedMaterials = new Material[_Materials.Count];
                                for (var i = 0; i < _Materials.Count; i++)
                                    ChangedMaterials[i] = BlueprintCannotPlaceMaterial;
                                _RendererElement._Renderer.materials = ChangedMaterials;
                            }
                        }
                    }
                    else
                    {
                        foreach (RendererElement _RendererElement in RendererElements)
                        {
                            List<Material> _Materials = _RendererElement._Renderer.materials.ToList();
                            var ChangedMaterials = new Material[_Materials.Count];
                            for (var i = 0; i < _Materials.Count; i++)
                                ChangedMaterials[i] = BlueprintCannotPlaceMaterial;
                            _RendererElement._Renderer.materials = ChangedMaterials;
                        }
                    }
                }
                /*else
                {
                    foreach (RendererElement _RendererElement in RendererElements)
                    {
                        List<Material> _Materials = _RendererElement._Renderer.materials.ToList();
                        var ChangedMaterials = new Material[_Materials.Count];
                        for (var i = 0; i < _Materials.Count; i++)
                            ChangedMaterials[i] = BlueprintMaterial;
                        _RendererElement._Renderer.materials = ChangedMaterials;
                    }
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (_Building)
                        {
                            _Building.Upgrade(1);
                            if (_Building.CurrentLevel > 0)
                            {
                                RendererElements.ForEach(item => item._Renderer.materials = item.Materials.ToArray());
                                List<MeshFilter> MeshFilters = TargetObject.GetComponentsInChildren<MeshFilter>().ToList();
                                MeshFilters.ForEach(item => { if (!item.GetComponent<MeshCollider>()) item.gameObject.AddComponent<MeshCollider>().sharedMesh = item.mesh; });
                                RendererElements.Clear();
                                TargetObject = null;
                            }
                            if (!GameManager.Instance.MainBuilding)
                                GameManager.Instance.MainBuilding = _Building;
                        }
                        else
                        {
                            RendererElements.ForEach(item => item._Renderer.materials = item.Materials.ToArray());
                            List<MeshFilter> MeshFilters = TargetObject.GetComponentsInChildren<MeshFilter>().ToList();
                            MeshFilters.ForEach(item => { if (!item.GetComponent<MeshCollider>()) item.gameObject.AddComponent<MeshCollider>().sharedMesh = item.mesh; });
                            RendererElements.Clear();
                            TargetObject = null;
                        }
                    }
                }*/
                if (Input.GetMouseButtonDown(1))
                {
                    Destroy(TargetObject.gameObject);
                    RendererElements.Clear();
                }
            }
        }
    }

    public void PlaceObject(GameObject Prefab)
    {
        if (TargetObject)
        {
            Destroy(TargetObject.gameObject);
            RendererElements.Clear();
        }
        TargetObject = Instantiate(Prefab).transform;
        List<Renderer> Renderers = TargetObject.GetComponentsInChildren<Renderer>().ToList();
        Renderers.ForEach(item => RendererElements.Add(new RendererElement(item, item.materials.ToList())));
        Renderers.ForEach(item => { if (item.GetComponent<MeshCollider>()) Destroy(item.GetComponent<MeshCollider>()); });
    }

    public void Panel(bool _Bool)
    {
        if (WorkshopPanel)
        {
            WorkshopPanel.gameObject.SetActive(_Bool);
        }
    }
}
