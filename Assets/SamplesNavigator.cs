using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;
using System.IO;

public class SamplesNavigator : MonoBehaviour
{
    #region Variables

    private TMP_Dropdown sampleDropdown;
    private MonoBehaviour previousScriptComponent;
    private Dictionary<string, Type> scriptDictionary = new Dictionary<string, Type>();
    private string previousOption = "";
    internal ConfigData configData;
    internal string path;

    #endregion

    #region Initialization

    void Start()
    {
        ReadConfigData();
        InitializeDropdowns();
        AttachEventListeners();
    }

    void InitializeDropdowns()
    {
        // Get references to dropdown components
        sampleDropdown = GameObject.Find("sampleDropdown").GetComponent<TMP_Dropdown>();
        // Populate script dictionary and dropdown options
        PopulateScriptDictionary();
        PopulateDropdowns();
    }

    void PopulateScriptDictionary()
    {
        // Add script names and types to the dictionary
        scriptDictionary.Add("SDK QuickStart", typeof(Signaling));
        scriptDictionary.Add("Secure authentication with tokens", typeof(AuthenticationWorkflow));
        scriptDictionary.Add("Cloud Proxy", typeof(CloudProxy));
        scriptDictionary.Add("Secure Data Encryption", typeof(DataEncryption));
        scriptDictionary.Add("Geofencing", typeof(Geofencing));
        scriptDictionary.Add("Store channel and user data", typeof(Storage));
        scriptDictionary.Add("Stream channels", typeof(StreamChannel));

    }

    void PopulateDropdowns()
    {
        if (configData.appId == "")
        {
            Debug.Log("Please provide an App ID to run the sample game");
            return;
        }
        // Define dropdown options
        List<string> scriptNames = new List<string>(scriptDictionary.Keys);

        // Assign options to the dropdowns
        sampleDropdown.AddOptions(scriptNames);
    }

    void AttachEventListeners()
    {
        // Attach event listeners
        sampleDropdown.onValueChanged.AddListener(OnSampleDropdownValueChanged);
    }

    #endregion

    #region Event Handlers

    void OnSampleDropdownValueChanged(int index)
    {
        // Handle sample dropdown value change
        DestroyPreviousScript();
        CreateOrGetScript(index);
    }

    #endregion

    #region Custom Methods

    void ReadConfigData()
    {
        path = Path.Combine(Application.dataPath, "utils", "Config.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            configData = JsonUtility.FromJson<ConfigData>(json);
        }
        else
        {
            Debug.LogError("Config file not found!");
        }
    }

    void CreateOrGetScript(int index)
    {
        string selectedOption = sampleDropdown.options[index].text;

        // Check if there was a previous option
        if (!string.IsNullOrEmpty(previousOption))
        {
            if (scriptDictionary.TryGetValue(previousOption, out Type previousScriptType) && previousOption != selectedOption)
            {
                DestroyPreviousScript();
            }
        }

        // Store the current option as the previous option
        previousOption = selectedOption;

        // Get the corresponding script type from the dictionary
        if (scriptDictionary.TryGetValue(selectedOption, out Type scriptType))
        {
            // Create or get an instance of the selected script type
            MonoBehaviour scriptInstance = gameObject.GetComponent(scriptType) as MonoBehaviour;
            scriptInstance = gameObject.AddComponent(scriptType) as MonoBehaviour;

            // Store the script component as the previous script component
            previousScriptComponent = scriptInstance;
        }
    }

    void DestroyPreviousScript()
    {
        if (previousScriptComponent != null)
        {
            Destroy(previousScriptComponent);
            Debug.Log("Destroyed previous script: " + previousScriptComponent.GetType());
            previousScriptComponent = null;
            previousOption = "";
        }
    }

    #endregion
}