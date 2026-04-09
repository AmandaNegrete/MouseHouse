using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using System.IO;

public class ControlsSettingsManager : MonoBehaviour
{

    public List<ControlBindListing> listings = new List<ControlBindListing>();

    ControlBindListing listeningForKey;

    public GameObject listingPrefab;


    public Transform listingsContainer;

   string saveFilePath
    {
        get { return ""; }
    }

    private void Start()
    {
        InputSystem.onAnyButtonPress.Call(call => OnNewKeyHit(call));
        PopulateListings();
    }

    public void OnNewKeyHit(InputControl key)
    {
        if(listeningForKey == null)
        {
            return;
        }
        PlayerMovement.main.controlScheme.actions[listeningForKey.keyName].ApplyBindingOverride(key.path);
        listeningForKey.keyName = key.displayName;


        listeningForKey.UpdateDisplays();

    }

    public void SaveToFile()
    {
        string fileContents = PlayerMovement.main.controlScheme.actions.SaveBindingOverridesAsJson();
        //Write to file
        File.WriteAllText(saveFilePath, fileContents);

    }

    public void LoadFromFile()
    {
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
                if (!binding.groups.Contains("Keyboard") || binding.path.Contains("delta"))
                    continue;


                GameObject newListing = Instantiate(listingPrefab, listingsContainer);
                ControlBindListing listing = newListing.GetComponent<ControlBindListing>();
                listing.keyName = binding.ToDisplayString();
                listing.inputName = action.name + " " + binding.name;

                listing.UpdateDisplays();
            }

        }


    }
}


