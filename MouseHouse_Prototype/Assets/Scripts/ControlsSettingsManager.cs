using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;    
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

public class ControlsSettingsManager : MonoBehaviour
{

    public List<ControlBindListing> listings = new List<ControlBindListing>();

    ControlBindListing listeningForKey;

    public GameObject listingPrefab;


    public Transform listingsContainer;

    public CanvasGroup bindingsGroup;

    public TextMeshProUGUI instructionsText;

    public string currentBindingGroup = "Keyboard";

    private const string instructionListening = "Listening for new binding... Press any key to rebind";

    private const string instructionDefault = "Click on a control binding to change it";
    string saveFilePath
    {
        get { return Path.Combine(Application.persistentDataPath + @"KeybindsData.txt") ; }
    }

    private void Start()
    {
        instructionsText.text = instructionDefault;
        InputSystem.onAnyButtonPress.Call(call => OnNewKeyHit(call));

        // Detect controller
        InputSystem.onAnyButtonPress.Call(OnNewKeyHit);
        InputSystem.onAnyButtonPress.Call(OnAnyControlUsed);
        InputSystem.onDeviceChange += OnDeviceChange;
        currentBindingGroup = DetectBindingGroup();
        InputSystem.onEvent += OnInputEvent;

        StartCoroutine(InitWhenReady());
    }


    public void OnNewKeyHit(InputControl key)
    {
        if(listeningForKey == null)
        {
            return;
        }


        InputAction action = PlayerMovement.main.controlScheme.actions[listeningForKey.actionName];
        
        //Currently cannot change composite binding.

        action.ApplyBindingOverride(key.path);

        listeningForKey.keyName = key.displayName;


        listeningForKey.UpdateDisplays();

        listeningForKey = null;
        if (instructionsText != null) instructionsText.text = instructionDefault;
    }

    public void SaveToFile()
    {
        string fileContents = PlayerMovement.main.controlScheme.actions.SaveBindingOverridesAsJson();
        //Write to file
        File.WriteAllText(saveFilePath, fileContents);
    }

    public void ResetToDefault()
    {
        PlayerMovement.main.controlScheme.actions.RemoveAllBindingOverrides();

        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
        }

        PopulateListings();
        listeningForKey = null;
        //foreach (ControlBindListing listing in listings)
        //{
        //    //Needs to be reworked
        //    listing.keyName = PlayerMovement.main.controlScheme.actions[listing.inputName].name;
        //}
    }

    public void LoadFromFile()
    {
        //Don't run on main menu
        if (!File.Exists(saveFilePath))
            return;

        string jsonString = File.ReadAllText(saveFilePath);
        PlayerMovement.main.controlScheme.actions.LoadBindingOverridesFromJson(jsonString);
    }

    public void PopulateListings()
    {
          if (PlayerMovement.main == null || PlayerMovement.main.controlScheme == null)
        {
            Debug.LogWarning("PlayerMovement.main is null. Skipping PopulateListings.");
            return;
        }
        listings.Clear();
        for (int i = listingsContainer.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(listingsContainer.GetChild(i).gameObject);
        }

        foreach (InputAction action in PlayerMovement.main.controlScheme.actions)
        {
            if (action == null)
                continue;

            foreach(InputBinding binding in action.bindings)
            {
                //Remove binding.isComposite to make it list in the input listing
                //composite bindings cannot be rebound (Unity's handling makes it difficult)
                if (string.IsNullOrEmpty(binding.groups) || !binding.groups.Contains(currentBindingGroup) || binding.path.Contains("delta") || binding.isPartOfComposite)
                    continue;

                GameObject newListing = Instantiate(listingPrefab, listingsContainer);
                ControlBindListing listing = newListing.GetComponent<ControlBindListing>();
                listing.keyName = binding.ToDisplayString();
                listing.actionName = action.name;
                listing.inputName = action.name + " " + binding.name;
                listing.manager = this;
                listings.Add(listing);
                listing.UpdateDisplays();
            }

        }
    }

    public void StartListeningForNewKey(ControlBindListing target)
    {
        instructionsText.text = instructionListening;
        listeningForKey = target;
    }

    public void CloseMenu()
    {
        bindingsGroup.interactable = false;
        bindingsGroup.blocksRaycasts = false;
        bindingsGroup.alpha = 0;
    }


    private void OnDestroy()
    {
        // Clean up subscription
        InputSystem.onDeviceChange -= OnDeviceChange;
        InputSystem.onEvent -= OnInputEvent;
    }

    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        // Don't switch while rebinding a control
        if (listeningForKey != null) return;

        // Only use button presses
        if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>()) return;

        string newGroup = (device is Gamepad) ? "Gamepad" : "Keyboard";

        if (newGroup != currentBindingGroup)
        {
            currentBindingGroup = newGroup;
            RefreshListings();
        }
    }


    private string DetectBindingGroup()
    {
        if (Gamepad.current != null) return "Gamepad";
        return "Keyboard";
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        // Refresh when a device is added or removed
        if (change == InputDeviceChange.Added || change == InputDeviceChange.Removed || change == InputDeviceChange.Enabled || change == InputDeviceChange.Disconnected)
        {
            string newGroup = DetectBindingGroup();
            if (newGroup != currentBindingGroup)
            {
                currentBindingGroup = newGroup;
                RefreshListings();
            }
        }
    }


    private void OnAnyControlUsed(InputControl control)
    {
        if (control == null) return;

        // Don't switch while rebinding a control
        if (listeningForKey != null) return;

        string newGroup;
        if (control.device is Gamepad || control.device is DualShockGamepad || (control.device.layout != null && control.device.layout.Contains("Gamepad")))
        {
            newGroup = "Gamepad";
        }
        else
        {
            newGroup = "Keyboard";
        }

        if (newGroup != currentBindingGroup)
        {
            currentBindingGroup = newGroup;
            RefreshListings();
        }
    }

    private void RefreshListings()
    {
        PopulateListings();
    }

    private IEnumerator InitWhenReady()
    {
        float waitTime = 2f;
        float time = 0f;
        while (PlayerMovement.main == null && time < waitTime)
        {
            time += Time.deltaTime;
            yield return null;
        }

        if (PlayerMovement.main == null)
        {
            Debug.LogError("PlayerMovement.main not found");
            yield break;
        }

        LoadFromFile();
        PopulateListings();
    }
}


