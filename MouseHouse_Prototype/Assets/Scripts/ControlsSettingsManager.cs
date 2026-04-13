using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class ControlsSettingsManager : MonoBehaviour
{

    public List<ControlBindListing> listings = new List<ControlBindListing>();

    ControlBindListing listeningForKey;

    public GameObject listingPrefab;


    public Transform listingsContainer;

    public CanvasGroup bindingsGroup;

    public TextMeshProUGUI instructionsText;

    private const string instructionListening = "Listening for new binding... Press any key to rebind, or press ESC to cancel.";

    private const string instructionDefault = "Click on a binding to change it.";
    string saveFilePath
    {
        get { return Path.Combine(Application.persistentDataPath + @"KeybindsData.txt") ; }
    }

    private void Start()
    {
        InputSystem.onAnyButtonPress.Call(call => OnNewKeyHit(call));
        LoadFromFile();
        PopulateListings();
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


        foreach(ControlBindListing listing in listings)
        {
            //Needs to be reworked
            listing.keyName = PlayerMovement.main.controlScheme.actions[listing.inputName].name;
        }
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

        foreach (InputAction action in PlayerMovement.main.controlScheme.actions)
        {
            if (action == null)
                continue;

            foreach(InputBinding binding in action.bindings)
            {
                //Remove binding.isComposite to make it list in the input listing
                //composite bindings cannot be rebound (Unity's handling makes it difficult)
                if (!binding.groups.Contains("Keyboard") || binding.path.Contains("delta") || binding.isPartOfComposite)
                    continue;

                GameObject newListing = Instantiate(listingPrefab, listingsContainer);
                ControlBindListing listing = newListing.GetComponent<ControlBindListing>();
                listing.keyName = binding.ToDisplayString();
                listing.actionName = action.name;
                listing.inputName = action.name + " " + binding.name;
                listing.manager = this;


                listing.UpdateDisplays();
            }

        }
    }

    public void StartListeningForNewKey(ControlBindListing target)
    {
        listeningForKey = target;
    }

    public void CloseMenu()
    {
        bindingsGroup.interactable = false;
        bindingsGroup.blocksRaycasts = false;
        bindingsGroup.alpha = 0;
    }
}


