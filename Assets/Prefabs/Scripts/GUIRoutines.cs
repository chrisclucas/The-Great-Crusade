using System;
using System.Collections.Generic;
using TheGreatCrusade;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace CommonRoutines
{
    public class GUIRoutines : MonoBehaviour
    {
        public static List<GameObject> guiList = new List<GameObject>();
        public static Canvas mapGraphicCanvas;

        /// <summary>
        /// Used to remove a gui element
        /// </summary>
        /// <param name="element"></param>
        public static void RemoveGUI(GameObject element)
        {
            if (guiList.Contains(element))
                guiList.Remove(element);
            DestroyImmediate(element);
        }

        /// <summary>
        /// Gets rid of all displayed gui's
        /// </summary>
        public static void RemoveAllGUIs()
        {
            // Get rid of any gui that is present
            // Copy list so the guis can be removed
            List<GameObject> removeList = new List<GameObject>();
            foreach (GameObject gui in guiList)
                removeList.Add(gui);


            // Get rid of all active guis
            foreach (GameObject gui in removeList)
                RemoveGUI(gui);
        }


        /// <summary>
        /// Sets up a canvas - not suitable for scrolling
        /// </summary>
        /// <param name="name"></param>
        /// <param name="panelWidth"></param>
        /// <param name="panelHeight"></param>
        /// <param name="canvasObject"></param>
        /// <returns></returns>
        public static GameObject CreateGUICanvas(string name, float panelWidth, float panelHeight, ref Canvas canvasObject, float xAnchorMin = 0.5f, float xAnchorMax = 0.5f, float yAnchorMin = 0.5f, float yAnchorMax = 0.5f)
        {
            GameObject guiInstance = new GameObject(name);
            guiList.Add(guiInstance);
            canvasObject = guiInstance.AddComponent<Canvas>();
            guiInstance.AddComponent<CanvasScaler>();
            guiInstance.AddComponent<GraphicRaycaster>();
            canvasObject.renderMode = RenderMode.ScreenSpaceOverlay;

            GameObject guiPanel = new GameObject("createGUICanvas");
            Image panelImage = guiPanel.AddComponent<Image>();
            panelImage.color = new Color32(0, 44, 255, 220);
            panelImage.rectTransform.anchorMax = new Vector2(xAnchorMax, yAnchorMax);
            panelImage.rectTransform.anchorMin = new Vector2(xAnchorMin, yAnchorMin);
            panelImage.rectTransform.sizeDelta = new Vector2(panelWidth, panelHeight);
            panelImage.rectTransform.anchoredPosition = new Vector2(0, 0);
            guiPanel.transform.SetParent(guiInstance.transform, false);

            return (guiInstance);
        }


        /// <summary>
        /// Creates a canvas that is setup for scrolling
        /// </summary>
        /// <param name="name"></param>
        /// <param name="panelWidth"></param>
        /// <param name="panelHeight"></param>
        /// <param name="scrollContentPanel"></param>
        /// <param name="canvasObject"></param>
        /// <returns></returns>
        public static GameObject CreateScrollingGUICanvas(string name, float panelWidth, float panelHeight, ref GameObject scrollContentPanel, ref Canvas canvasObject)
        {
            IORoutines.WriteToLogFile("createScrollingGUICanvas: screen height = " + UnityEngine.Screen.height);

            if (panelHeight < (UnityEngine.Screen.height - 50))
            {
                IORoutines.WriteToLogFile("createScrollingGUICanvas: panel height = " + panelHeight);
                // The height is small enough that scrolling isn't needed
                return CreateGUICanvas(name, panelWidth, panelHeight, ref canvasObject);
            }

            GameObject squareImage = (GameObject)Resources.Load("SquareObject");
            GameObject sliderHandleImage = (GameObject)Resources.Load("SliderHandleObject");

            GameObject guiInstance = new GameObject(name);
            guiList.Add(guiInstance);
            canvasObject = guiInstance.AddComponent<Canvas>();
            guiInstance.AddComponent<CanvasScaler>();
            guiInstance.AddComponent<GraphicRaycaster>();
            canvasObject.renderMode = RenderMode.ScreenSpaceOverlay;

            GameObject scrollRect = new GameObject("Scroll Rect");
            ScrollRect scrollable = scrollRect.AddComponent<ScrollRect>();
            Image scrollRectImage = scrollRect.AddComponent<Image>();

            scrollable.horizontal = false;
            scrollable.vertical = true;
            scrollable.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
            scrollable.inertia = false;
            scrollable.scrollSensitivity = -1;

            scrollRectImage.color = new Color32(0, 44, 255, 220);

            scrollRect.GetComponent<RectTransform>().sizeDelta = new Vector2(panelWidth, (UnityEngine.Screen.height - 50));
            scrollRect.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
            scrollRect.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
            scrollRect.transform.SetParent(guiInstance.transform, false);

            GameObject scrollViewport = new GameObject("ScrollViewport");
            scrollViewport.AddComponent<Mask>();
            scrollViewport.AddComponent<Image>();
            scrollViewport.GetComponent<RectTransform>().sizeDelta = new Vector2(panelWidth, (UnityEngine.Screen.height - 50));
            scrollViewport.transform.SetParent(scrollRect.transform);
            scrollViewport.GetComponent<RectTransform>().localPosition = new Vector2(0f, 0f);

            GameObject scrollContent = new GameObject("ScrollContent");
            scrollContent.AddComponent<RectTransform>();
            scrollContent.GetComponent<RectTransform>().sizeDelta = new Vector2(panelWidth, panelHeight);
            scrollContent.transform.SetParent(scrollViewport.transform, false);
            scrollContentPanel.transform.SetParent(scrollContent.transform, false);

            scrollable.content = scrollContent.GetComponent<RectTransform>();
            scrollable.viewport = scrollViewport.GetComponent<RectTransform>();


            GameObject scrollHandle = new GameObject("ScrollHandle");
            scrollHandle.AddComponent<Image>();
            scrollHandle.GetComponent<Image>().sprite = sliderHandleImage.GetComponent<Image>().sprite;
            scrollHandle.GetComponent<RectTransform>().sizeDelta = new Vector2(20, 400);

            GameObject scrollbarObject = new GameObject("ScrollbarObject");
            scrollbarObject.transform.SetParent(scrollRect.transform);
            Scrollbar verticalScroll = scrollbarObject.AddComponent<Scrollbar>();
            scrollbarObject.GetComponent<RectTransform>().sizeDelta = new Vector2(20, (UnityEngine.Screen.height - 50));
            scrollbarObject.GetComponent<RectTransform>().localPosition = new Vector2(panelWidth / 2, 0f);
            scrollable.verticalScrollbar = verticalScroll;
            verticalScroll.GetComponent<Scrollbar>().direction = Scrollbar.Direction.BottomToTop;
            verticalScroll.targetGraphic = scrollHandle.GetComponent<Image>();
            verticalScroll.handleRect = scrollHandle.GetComponent<RectTransform>();

            Image scrollbarImage = scrollbarObject.AddComponent<Image>();
            scrollbarImage.sprite = squareImage.GetComponent<Image>().sprite;

            GameObject scrollArea = new GameObject("ScrollArea");
            scrollArea.AddComponent<RectTransform>();
            scrollArea.transform.SetParent(scrollbarObject.transform, false);

            scrollHandle.transform.SetParent(scrollArea.transform, false);

            return (guiInstance);
        }

        /// <summary>
        /// Creates a button for a gui
        /// </summary>
        /// <param name="name"></param>
        /// <param name="buttonText"></param>
        /// <param name="xPosition"></param>
        /// <param name="yPosition"></param>
        /// <param name="canvasInstance"></param>
        /// <returns></returns>
        public static UnityEngine.UI.Button CreateButton(string name, string buttonText, float xPosition, float yPosition, Canvas canvasInstance)
        {
            GameObject tempPrefab;
            UnityEngine.UI.Button tempButton;

            tempPrefab = (GameObject)Resources.Load("OK Button");
            tempButton = Instantiate(tempPrefab).GetComponent<UnityEngine.UI.Button>();
            tempButton.transform.localScale = new Vector3(0.5f, 0.75f);
            tempButton.transform.SetParent(canvasInstance.transform, false);
            tempButton.image.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            tempButton.image.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            tempButton.image.rectTransform.sizeDelta = new Vector2(90, 30);
            tempButton.image.rectTransform.anchoredPosition = new Vector2(xPosition, yPosition);
            tempButton.name = name;
            tempButton.GetComponentInChildren<Text>().text = buttonText;

            if (!GlobalGameFields.localControl)
                tempButton.interactable = false;

            return (tempButton);
        }

        /// <summary>
        /// Creates a slider for a scrolling canvas
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sliderType"></param>
        /// <param name="xPosition"></param>
        /// <param name="yPosition"></param>
        /// <param name="canvasInstance"></param>
        /// <returns></returns>
        public static Slider CreateSlider(string name, string sliderType, float xPosition, float yPosition, Canvas canvasInstance)
        {
            GameObject tempPrefab;
            Slider tempSlider;

            tempPrefab = (GameObject)Resources.Load(sliderType);
            tempPrefab.transform.position = new Vector2(xPosition, yPosition);
            tempSlider = Instantiate(tempPrefab, canvasInstance.transform).GetComponent<Slider>();
            tempSlider.transform.SetParent(canvasInstance.transform, false);
            tempSlider.name = name;
            return (tempSlider);
        }

        /// <summary>
        /// Creates a text box for a gui
        /// </summary>
        /// <param name="textMessage"></param>
        /// <param name="name"></param>
        /// <param name="textWidth"></param>
        /// <param name="textHeight"></param>
        /// <param name="textX"></param>
        /// <param name="textY"></param>
        /// <param name="canvasInstance"></param>
        /// <returns></returns>
        public static GameObject CreateUIText(string textMessage, string name, float textWidth, float textHeight, float textX, float textY, Color textColor, Canvas canvasInstance, float xAnchorMin = 0.5f, float xAnchorMax = 0.5f, float yAnchorMin = 0.5f, float yAnchorMax = 0.5f)
        {
            GameObject textGameObject = new GameObject(name);
            textGameObject.transform.SetParent(canvasInstance.transform, false);
            var tempText = textGameObject.AddComponent<TextMeshProUGUI>();
            tempText.text = textMessage;
            tempText.rectTransform.anchorMax = new Vector2(xAnchorMin, yAnchorMin);
            tempText.rectTransform.anchorMin = new Vector2(xAnchorMax, yAnchorMax);
            tempText.rectTransform.anchoredPosition = new Vector2(textX, textY);
            tempText.rectTransform.sizeDelta = new Vector2(textWidth, textHeight);
            tempText.alignment = TextAlignmentOptions.Center;
            tempText.fontSize = 14;
            tempText.color = textColor;
            return (textGameObject);
        }

        /// <summary>
        /// Creates a text value on top of the hexes
        /// </summary>
        /// <param name="textMessage"></param>
        /// <param name="name"></param>
        /// <param name="textWidth"></param>
        /// <param name="textHeight"></param>
        /// <param name="textX"></param>
        /// <param name="textY"></param>
        /// <param name="canvasInstance"></param>
        public static void CreateHexText(string textMessage, string name, float textWidth, float textHeight, float textX, float textY, int fontSize, Color textColor, Canvas canvasInstance)
        {
            GameObject textGameObject = new GameObject(name);
            textGameObject.layer = 12; // Set the layer so it renders above the hex but beneath the counters
            textGameObject.transform.SetParent(canvasInstance.transform, false);
            var tempText = textGameObject.AddComponent<TextMeshPro>();
            tempText.text = textMessage;
            if (textMessage.Length < 6)
                tempText.fontSize = 120;
            else if (textMessage.Length < 10)
                tempText.fontSize = 106;
            else
                tempText.fontSize = 92;
            tempText.renderer.sortingLayerID = SortingLayer.NameToID("Text");
            tempText.rectTransform.anchoredPosition = new Vector2(textX, textY);
            tempText.rectTransform.sizeDelta = new Vector2(textWidth, textHeight);
            tempText.rectTransform.localScale = new Vector2(0.1f, 0.1f);
            tempText.alignment = TMPro.TextAlignmentOptions.Center;
            tempText.color = textColor;
            tempText.raycastTarget = false;
        }

        /// <summary>
        /// Creates text - these are the informational graphics on the board.  I had to create this since the CreateUIText and CreateHexText
        /// </summary>
        /// <param name="textMessage"></param>
        /// <param name="name"></param>
        /// <param name="textWidth"></param>
        /// <param name="textHeight"></param>
        /// <param name="textX"></param>
        /// <param name="textY"></param>
        /// <param name="fontSize"></param>
        /// <param name="textColor"></param>
        /// <param name="canvasInstance"></param>
        public static void CreateBoardText(string textMessage, string name, float textWidth, float textHeight, float textX, float textY, float scaling, float rotation, Color textColor, Canvas canvasInstance)
        {
            GameObject textGameObject = new GameObject(name);
            textGameObject.layer = 12; // Set the layer so it renders above the hex but beneath the counters
            textGameObject.transform.SetParent(canvasInstance.transform, false);
            var tempText = textGameObject.AddComponent<TextMeshPro>();
            tempText.text = textMessage;
            tempText.renderer.sortingLayerID = SortingLayer.NameToID("Text");
            tempText.rectTransform.anchoredPosition = new Vector2(textX, textY);
            tempText.rectTransform.sizeDelta = new Vector2(textWidth, textHeight);
            tempText.rectTransform.localScale = new Vector2(scaling, scaling);
            //tempText.transform.Rotate(0f, 0f, rotation);
            tempText.rectTransform.Rotate(0f, 0f, rotation);
            tempText.alignment = TMPro.TextAlignmentOptions.Center;
            tempText.color = textColor;
            tempText.raycastTarget = false;
        }

        /// <summary>
        /// Updates the hex value shown - for AI debugging
        /// </summary>
        /// <param name="hex"></param>
        public static void UpdateHexValueText(GameObject hex)
        {
            GameObject textGameObject = GameObject.Find(hex.name + "HexValueText");
            textGameObject.GetComponent<TextMeshPro>().text = Convert.ToString(hex.GetComponent<HexDatabaseFields>().hexValue);

        }

        /// <summary>
        /// Takes the string and returns the Unity color type that matches 
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Color ConvertStringToColor(string color)
        {
            if (color == "Red")
                return (Color.red);
            else if (color == "Black")
                return (Color.black);
            else if (color == "Yellow")
                return (Color.yellow);
            else if (color == "Green")
                return (Color.green);
            else if (color == "Grey")
                return (Color.grey);
            else if (color == "Gray")
                return (Color.gray);
            else if (color == "Blue")
                return (Color.blue);
            else if (color == "Cyan")
                return (Color.cyan);
            else if (color == "Magenta")
                return (Color.magenta);
            else if (color == "White")
                return (Color.white);
            else
                IORoutines.WriteToLogFile("ConvertStingToColor: unknown color passed - " + color);
            return (Color.black);
        }

        /// <summary>
        /// Creates an input filed for a gui
        /// </summary>
        /// <param name="name"></param>
        /// <param name="xPosition"></param>
        /// <param name="yPosition"></param>
        /// <param name="canvasInstance"></param>
        /// <returns></returns>
        public static InputField CreateInputField(string name, float xPosition, float yPosition, Canvas canvasInstance)
        {
            GameObject tempPrefab;
            InputField tempInputField;

            tempPrefab = (GameObject)Resources.Load("InputField");
            tempInputField = Instantiate(tempPrefab).GetComponent<InputField>();

            tempInputField.transform.localScale = new Vector3(1, 1);
            tempInputField.transform.SetParent(canvasInstance.transform, false);
            //tempInputField.image.rectTransform.sizeDelta = new Vector2(180, 30);
            tempInputField.image.rectTransform.anchoredPosition = new Vector2(xPosition, yPosition);
            tempInputField.name = name + "InputField";

            // For testing only
            //if (TransportScript.localComputerIPAddress == "192.168.1.73")
            //    tempInputField.text = "192.168.1.67";
            //else
            //    tempInputField.text = "192.168.1.73";


            //tempInputField.text = TransportScript.defaultRemoteComputerIPAddress;
            return (tempInputField);
        }

        /// <summary>
        /// Creates a unit with a toggle - for selection in a gui
        /// </summary>
        /// <param name="name"></param>
        /// <param name="xPosition"></param>
        /// <param name="yPosition"></param>
        /// <param name="canvasInstance"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static Toggle CreateUnitTogglePair(string name, float xPosition, float yPosition, Canvas canvasInstance, GameObject unit)
        {
            GameObject tempToggle;

            CreateUnitImage(unit, name + "Image", xPosition, yPosition, canvasInstance);
            tempToggle = CreateToggle(name, xPosition, yPosition - GlobalGameFields.GUIUNITIMAGESIZE, canvasInstance);

            tempToggle.name = name;

            return (tempToggle.GetComponent<Toggle>());
        }

        /// <summary>
        /// Creates an image of a specific unit for a gui
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="name"></param>
        /// <param name="xPosition"></param>
        /// <param name="yPosition"></param>
        /// <param name="canvasInstance"></param>
        /// <returns></returns>
        public static GameObject CreateUnitImage(GameObject unit, string name, float xPosition, float yPosition, Canvas canvasInstance)
        {
            GameObject tempPrefab;
            Image tempImage;

            tempPrefab = Instantiate(Resources.Load("GUI Image") as GameObject, new Vector3(xPosition, yPosition, 0), Quaternion.identity);
            tempImage = tempPrefab.GetComponent<Image>();
            tempImage.transform.SetParent(canvasInstance.transform, false);
            tempImage.sprite = unit.GetComponent<SpriteRenderer>().sprite;
            tempImage.rectTransform.sizeDelta = new Vector2(GlobalGameFields.GUIUNITIMAGESIZE, GlobalGameFields.GUIUNITIMAGESIZE);
            tempImage.name = name;
            return (tempImage.gameObject);
        }

        /// <summary>
        /// Creates a toggle for a gui
        /// </summary>
        /// <param name="name"></param>
        /// <param name="xPosition"></param>
        /// <param name="yPosition"></param>
        /// <param name="canvasInstance"></param>
        /// <returns></returns>
        public static GameObject CreateToggle(string name, float xPosition, float yPosition, Canvas canvasInstance, float xAnchorMin = 0.5f, float xAnchorMax = 0.5f, float yAnchorMin = 0.5f, float yAnchorMax = 0.5f)
        {
            GameObject tempPrefab;
            Toggle tempToggle;

            tempPrefab = Instantiate(Resources.Load("GUI Toggle") as GameObject, new Vector3(xPosition, yPosition, 0), Quaternion.identity);
            tempToggle = tempPrefab.GetComponent<Toggle>();

            tempToggle.transform.SetParent(canvasInstance.transform, false);
            tempToggle.transform.localScale = new Vector2(0.5f, 0.5f);
            tempToggle.name = name;
            tempToggle.GetComponent<RectTransform>().anchorMax = new Vector2(xAnchorMax, yAnchorMax);
            tempToggle.GetComponent<RectTransform>().anchorMin = new Vector2(xAnchorMin, yAnchorMin);
            if (!GlobalGameFields.localControl)
                tempToggle.interactable = false;

            return (tempPrefab);
        }
    }
}
