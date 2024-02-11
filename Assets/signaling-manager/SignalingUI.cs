using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class SignalingUI : MonoBehaviour
{
    internal Canvas canvas;
#pragma warning disable 0649
    [SerializeField] int maxMessages = 25;
    internal List<Tuple<GameObject, GameObject>> messages = new List<Tuple<GameObject, GameObject>>();
#pragma warning restore 0649

    public virtual void Start() { }

    public GameObject AddInputField(string fieldName, Vector3 position, string placeholderText, Vector2 size)
    {
        // Create input field.
        TMP_DefaultControls.Resources resources = new TMP_DefaultControls.Resources();
        GameObject inputFieldGo = TMP_DefaultControls.CreateInputField(resources);
        inputFieldGo.name = fieldName;
        inputFieldGo.transform.SetParent(canvas.transform, false);

        TMP_InputField tmpInputField = inputFieldGo.GetComponent<TMP_InputField>();
        RectTransform inputFieldTransform = tmpInputField.GetComponent<RectTransform>();
        inputFieldTransform.sizeDelta = size;
        inputFieldTransform.localPosition = position;

        TMP_Text textComponent = inputFieldGo.GetComponentInChildren<TMP_Text>();
        textComponent.alignment = TextAlignmentOptions.Center;
        tmpInputField.placeholder.GetComponent<TMP_Text>().text = placeholderText;

        return inputFieldGo;
    }

    public virtual void Update() { }

    // Create a button
    public virtual GameObject AddButton(string BName, Vector3 BPos, string BText, Vector2 BSize)
    {
        // Create the button game object using the TMP_DefaultControls utility class
        GameObject Button = TMP_DefaultControls.CreateButton(new TMP_DefaultControls.Resources());
        // Set the game object settings
        Button.name = BName;
        Button.transform.SetParent(canvas.transform, false);
        Button.transform.localPosition = BPos;
        Button.transform.localScale = Vector3.one;
        RectTransform rectTransform = Button.GetComponent<RectTransform>();
        rectTransform.sizeDelta = BSize;
        // Set the text of the button's child TMP_Text component and return the button game object
        Button.GetComponentInChildren<TMP_Text>().text = BText;
        return Button;
    }

    public virtual GameObject AddDropdown(string Dname, Vector3 DPos, Vector2 DSize)
    {
        GameObject dropDownGo = TMP_DefaultControls.CreateDropdown(new TMP_DefaultControls.Resources());
        dropDownGo.name = Dname;
        dropDownGo.transform.SetParent(canvas.transform);
        dropDownGo.transform.localPosition = DPos;
        dropDownGo.transform.localScale = Vector3.one;
        RectTransform rectTransform = dropDownGo.GetComponent<RectTransform>();
        rectTransform.sizeDelta = DSize;
        return dropDownGo;
    }

    public virtual GameObject AddLabel(string LName, Vector3 LPos, string LText, int labelFontSize)
    {
        GameObject labelGo = new GameObject(LName);
        TextMeshProUGUI label = labelGo.AddComponent<TextMeshProUGUI>();
        label.transform.SetParent(canvas.transform);
        label.transform.localPosition = LPos;
        label.text = LText;
        label.fontSize = labelFontSize;

        // Adjust the RectTransform size based on your requirements
        RectTransform labelRectTransform = labelGo.GetComponent<RectTransform>();
        // You can set the size based on the text content or use a fixed size
        labelRectTransform.sizeDelta = new Vector2(150, 20);
        return labelGo;
    }

    // Add text messages dynamically to the panel
    public void AddTextToDisplay(string text, Color bgColor, TextAlignmentOptions alignment)
    {
        // Create a new UI panel for background
        GameObject newPanelObject = new GameObject("DynamicPanel");
        newPanelObject.transform.SetParent(GameObject.Find("Panel").transform); // Set the parent (assuming chatPanel is a GameObject)

        RectTransform panelRectTransform = newPanelObject.AddComponent<RectTransform>();
        panelRectTransform.localScale = Vector3.one;

        // Add Image component for background color
        Image panelImage = newPanelObject.AddComponent<Image>();
        panelImage.color = bgColor;

        // Create TextMeshPro text
        GameObject newTextObject = new GameObject("DynamicText");
        newTextObject.transform.SetParent(newPanelObject.transform); // Set the parent as the new panel

        TextMeshProUGUI textMesh = newTextObject.AddComponent<TextMeshProUGUI>();
        textMesh.text = text;
        textMesh.fontSize = 25;
        textMesh.color = Color.white;

        // Set the alignment of the TextMeshProUGUI component
        textMesh.alignment = alignment;

        messages.Add(new Tuple<GameObject, GameObject>(newTextObject, newPanelObject)); // Using Tuple

        // Set the RectTransform properties based on alignment
        RectTransform textRectTransform = newTextObject.GetComponent<RectTransform>();
        textRectTransform.localScale = Vector3.one;

        switch (alignment)
        {
            case TextAlignmentOptions.Left:
                textRectTransform.pivot = new Vector2(0f, 0.5f);
                textRectTransform.anchorMin = new Vector2(0f, 0.5f);
                textRectTransform.anchorMax = new Vector2(0f, 0.5f);
                break;
            case TextAlignmentOptions.Right:
                textRectTransform.pivot = new Vector2(1f, 0.5f);
                textRectTransform.anchorMin = new Vector2(1f, 0.5f);
                textRectTransform.anchorMax = new Vector2(1f, 0.5f);
                break;
        }

        // Set the size of the text box to match the text
        textRectTransform.sizeDelta = new Vector2(textMesh.preferredWidth, textMesh.preferredHeight);

        // Optionally, adjust the position of newTextObject within the panel
        textRectTransform.anchoredPosition = Vector2.zero;
    }

    public void ClearMessages()
    {
        foreach(var message in messages)
        {
            Destroy(message.Item1);
            Destroy(message.Item2);
        }
    }
}
