using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;


public class ControlBindListing : MonoBehaviour
{
    public string keyName;

    public string inputName;

    public string actionName;

    public TextMeshProUGUI displayName;
    public TextMeshProUGUI keyNameText;
    
    public ControlsSettingsManager manager;

    public void UpdateDisplays()
    {
        keyNameText.text = keyName;
        displayName.text = inputName;
    }

    public void Clicked()
    {
        manager.StartListeningForNewKey(this);
    }

}
