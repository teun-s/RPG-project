using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIToggleManager : MonoBehaviour
{

    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject skillsPanel;
    [SerializeField] private Toggle inventoryToggle;
    [SerializeField] private Toggle skillsToggle;
    [SerializeField] private GameObject equipmentPanel;
    [SerializeField] private Toggle equipmentToggle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (inventoryPanel == null ||  skillsPanel == null || inventoryToggle == null || skillsToggle == null || equipmentPanel == null || equipmentToggle == null)
        {
            Debug.Log("UIToggleManager is missing serialized variables!");
            return;
        }
        inventoryPanel.SetActive(inventoryToggle.isOn);
        skillsPanel.SetActive(skillsToggle.isOn);
        equipmentPanel.SetActive(equipmentToggle.isOn);

        inventoryToggle.onValueChanged.AddListener((isOn) => OnToggleChanged(isOn, inventoryPanel));
        skillsToggle.onValueChanged.AddListener((isOn) => OnToggleChanged(isOn, skillsPanel));
        equipmentToggle.onValueChanged.AddListener((isOn) => OnToggleChanged(isOn, equipmentPanel));


    }

    private void OnToggleChanged(bool isOn, GameObject panel)
    {
        panel.SetActive(isOn);
        Debug.Log($"Toggled panel {panel.name} to {(isOn? "visible" : "hidden")}");
        UIEventManager.Instance.NotifyViewSwitched();
    }

}
