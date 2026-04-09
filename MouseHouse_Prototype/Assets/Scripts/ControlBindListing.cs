using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;


public class ControlBindListing : MonoBehaviour
{
    public string keyName;

    public string inputName;


    public TextMeshProUGUI displayName;
    public TextMeshProUGUI keyNameText;

    public void UpdateDisplays()
    {
        keyNameText.text = keyName;
        displayName.text = inputName;
    }

}
