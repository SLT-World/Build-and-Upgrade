using UnityEngine;
using UnityEngine.UI;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance;

    public Building BuildingTarget;

    [SerializeField] GameObject BuildingInformationPanelUI;
    [SerializeField] Text LevelTextUI;
    [SerializeField] Slider LevelSliderUI;

    private void Start()
    {
        Instance = this;
    }

    private void Update()
    {
        if (BuildingTarget && BuildingInformationPanelUI)
        {
            LevelTextUI.text = BuildingTarget.CurrentLevel.ToString();
            //LevelSliderUI.maxValue = BuildingTarget.MaxLevel;
            LevelSliderUI.value = BuildingTarget.CurrentLevel;
            if (BuildingTarget.MaxLevel == 1)
            {
                LevelSliderUI.value = LevelSliderUI.maxValue;
            }
        }
    }

    public void Upgrade(int _Amount)
    {
        BuildingTarget.Upgrade(_Amount);
    }

    public void Panel(bool _Bool)
    {
        if (BuildingInformationPanelUI)
        {
            BuildingInformationPanelUI.SetActive(_Bool);
        }
    }
}
