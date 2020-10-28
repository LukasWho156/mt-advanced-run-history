using HarmonyLib;
using ShinyShoe;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AdvancedRunHistory.CustomUIStuff
{
    /// <summary>
    /// A utility class to facilitate the create of custom UI elements from in-game templates.
    /// </summary>
    /// <remarks>
    /// Note that I have extremely limited knowledge of how Unity works, so this is all very hacky and probably
    /// against Unity's design philosophy. But meh, I could care less.
    /// Some remarks on how to use the elements:
    /// <para>- All elements must be given a parent and a position on their creation. The elements are positioned
    /// retalive to the top left corner of their parent. Positive y values correspond to downwards.</para>
    /// <para>- Quite a few of the elements currently have (partially) fixed dimensions. This is to make them
    /// work properly. There's probably a way to change those, but I'd have to dig deeper into their inner
    /// workings than I'm willing to right now.</para>
    /// <para>- You should call ApplyScreenInput for all sliders and dropdowns in your screen's / dialog's
    /// ApplyScreenInput method if you want them to work properly.</para>
    /// <para>- For sliders and dropdowns, a good way to process input is to use
    /// <c>GameUISelectableDropdown.optionChosenSignal.AddListener()</c>
    /// <c>GameUISelectableSlider.ValueSelectSignal.AddListener()</c>, respectively.</para>
    /// <para>- For buttons and toggles, you can handle their input by checking for
    /// <c>triggeredMappingID == InputManager.Controls.Clicked &&
    /// triggeredUI.IsGameUIComponent(selectable)</c>. For toggles, you can then call
    /// <c>bool GameUISelectableToggle.Toggle()</c> to toggle their state and get the new state.</para>
    /// </remarks>
    class CustomUIManager
    {
        // These values are used by dropdowns, toggles and sliders created with this class.
        public const int STANDARD_UI_ELEMENT_WIDTH = 250;
        public const int STANDARD_UI_TOGGLE_WIDTH = 240;
        public const int STANDARD_UI_ELEMENT_HEIGHT = 50;

        // Flag to keep the manager from initalizing twice.
        private static bool initalized;
        // Custom dialog input listeners, currently unused.
        private static Dictionary<ScreenDialog, ICustomDialogInputListener> listeners = new Dictionary<ScreenDialog, ICustomDialogInputListener>();

        // A bunch of templates.
        private static GameUISelectableButton buttonTemplate;
        private static GameUISelectableDropdown dropdownTemplate;
        private static GameUISelectableToggle toggleTemplate;
        private static GameUISelectableSlider sliderTemplate;
        private static TextMeshProUGUI labelTemplate;
        private static ScreenDialog dialogTemplate;

        /// <summary>
        /// Check if the necessary UI elements have been initalized.
        /// </summary>
        /// <remarks>
        /// Currently, this check is done by just setting a flag at the end of the initalization method.
        /// Hacky, but seems to work for now.
        /// </remarks>
        /// <returns><c>true</c> if initalized, otherwise <c>false</c></returns>
        public static bool AreTemplatesInitalized()
        {
            return initalized;
        }

        /// <summary>
        /// Initalize a bunch of UI element templates: a dialog, a button, a dropdown menu, a slider, a toggle button and
        /// a label.
        /// </summary>
        /// <remarks>
        /// This method needs to be called before trying to instantiate any UI elements with <c>CreateCustomXXX</c>
        /// methods, otherwise those will throw unchecked NullPointerExceptions (I should probably handle those better).
        /// Currently, this is achieved by the <see cref="InitCustomUIElementsPatch"/>. Also, once it has been called,
        /// another call to this method has no effect.
        /// </remarks>
        /// <param name="settingsScreen">The settings screen. Using this to store the dialog template so that it doesn't
        /// get unloaded. There's probably a less hacky way to do this.</param>
        /// <param name="dialog">The dialog to be used as a template.</param>
        /// <param name="settingsDialog">The Settings Dialog, used to get a variety of UI elements.</param>
        public static void InitalizeTemplates(SettingsScreen settingsScreen,
            ScreenDialog dialog,
            SettingsDialog settingsDialog)
        {
            // This only needs to run once.
            if (!AreTemplatesInitalized())
            {
                initalized = true;
                // Just store a bunch of UI elements from the settings screen as templates. They will always be loaded,
                // so we don't have to care about manipulating them in any way yet.
                buttonTemplate = Traverse.Create(settingsDialog).Field("keyMappingButton").GetValue<GameUISelectableButton>();
                dropdownTemplate = Traverse.Create(settingsDialog).Field("gameSpeedDropdown").GetValue<GameUISelectableDropdown>(); ;
                toggleTemplate = Traverse.Create(settingsDialog).Field("googlyEyesToggle").GetValue<GameUISelectableToggle>();
                sliderTemplate = Traverse.Create(Traverse.Create(settingsDialog).Field("scrollSensitivityControl").GetValue<ScrollSensitivityControl>()).Field("slider").GetValue<GameUISelectableSlider>();
                labelTemplate = settingsDialog.transform.Find("Content/Columns/Column 1/Audio Section/Global volume control").GetComponentInChildren<TextMeshProUGUI>();
                // We actually neeed to instantiate a copy of the dialog template right now, as the Patch Notes dialog will
                // unload as soon as you leave the Main Menu screen. Also, we can remove its contents while we're at it.
                dialogTemplate = GameObject.Instantiate(dialog, settingsScreen.transform);
                foreach(Transform child in dialogTemplate.transform.Find("Content"))
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Create a <c>ScreenDialog</c> object, assign it to a parent <c>Component</c> and return it.
        /// </summary>
        /// <param name="parent">The dialog's supposed parent.</param>
        /// <param name="name">The Unity name of this dialog.</param>
        /// <param name="label">What is written in the header of the dialog.</param>
        /// <returns>The created <c>ScreenDialog</c></returns>
        public static ScreenDialog CreateCustomDialog(Component parent, string name, string label)
        {
            ScreenDialog dialog = GameObject.Instantiate(dialogTemplate, parent.transform);
            dialog.name = name;
            dialog.GetComponentInChildren<TextMeshProUGUI>().text = label;
            return dialog;
        }

        /// <summary>
        /// Create a label at the specified position, assign it to a parent <c>Component</c> and return it.
        /// </summary>
        /// <param name="parent">The label's supposed parent.</param>
        /// <param name="message">What the label should say.</param>
        /// <param name="x">The x-position of the label, relative to the container's upper left corner.</param>
        /// <param name="y">The y-position of the label, relative to the container's upper left corner.</param>
        /// <param name="width">The width of the label. If this is too small, the text will scale down. Otherwise things are probably fine.</param>
        /// <param name="height">The height of the label. If this is too small, the text will scale down. 50 seems to work well.</param>
        /// <returns>The created label.</returns>
        public static TextMeshProUGUI AddLabelToComponent(Component parent, string message, float x, float y, float width, float height)
        {
            TextMeshProUGUI label = GameObject.Instantiate(labelTemplate, parent.transform);
            label.name = "Label";
            label.text = message;
            (label.transform as RectTransform).anchorMin = new Vector2(0, 1);
            (label.transform as RectTransform).anchorMax = new Vector2(0, 1);
            (label.transform as RectTransform).offsetMin = new Vector2(x, -y - height);
            (label.transform as RectTransform).offsetMax = new Vector2(x + width, -y);
            return label;
        }

        /// <summary>
        /// Create a button at the specified position, assign it to a parent <c>Component</c> and return it.
        /// </summary>
        /// <param name="parent">The buttons's supposed parent.</param>
        /// <param name="name">The Unity name of the button.</param>
        /// <param name="label">The button's text.</param>
        /// <param name="x">The x-position of the button, relative to the container's upper left corner.</param>
        /// <param name="y">The y-position of the button, relative to the container's upper left corner.</param>
        /// <param name="width">The width of the button. 200-300 seem like decent values.</param>
        /// <param name="height">The height of the button. 60 seems to be the "standard" value.</param>
        /// <returns>The created button.</returns>
        public static GameUISelectableButton AddButtonToComponent(Component parent, string name, string label, float x, float y, float width, float height)
        {
            GameUISelectableButton button = GameObject.Instantiate(buttonTemplate, parent.transform);
            button.name = name;
            (button.transform as RectTransform).offsetMin = new Vector2(x, -y - height);
            (button.transform as RectTransform).offsetMax = new Vector2(x + width, -y);
            button.GetComponentInChildren<TextMeshProUGUI>().text = label;
            return button;
        }

        /// <summary>
        /// Create a toggle button at the specified position, assign it to a parent <c>Component</c> and return it.
        /// </summary>
        /// <remarks>
        /// Uses <c>STANDARD_UI_ELEMENT_WIDTH</c> and <c>STANDARD_UI_ELEMENT_HEIGHT</c> to determine the
        /// width and height of this object. Using other values might lead to weird results.
        /// </remarks>
        /// <param name="parent">The toggle buttons's supposed parent.</param>
        /// <param name="name">The Unity name of the toggle button.</param>
        /// <param name="x">The x-position of the toggle button, relative to the container's upper left corner.</param>
        /// <param name="y">The y-position of the toggle button, relative to the container's upper left corner.</param>
        /// <returns>The created toggle button.</returns>
        public static GameUISelectableToggle AddToggleToComponent(Component parent, string name, float x, float y)
        {
            GameUISelectableToggle toggle = GameObject.Instantiate(toggleTemplate, parent.transform);
            toggle.name = name;
            (toggle.transform as RectTransform).anchorMin = new Vector2(0, 1);
            (toggle.transform as RectTransform).anchorMax = new Vector2(0, 1);
            (toggle.transform as RectTransform).offsetMin = new Vector2(x, -y - STANDARD_UI_ELEMENT_HEIGHT);
            (toggle.transform as RectTransform).offsetMax = new Vector2(x + STANDARD_UI_TOGGLE_WIDTH, -y);
            return toggle;
        }

        /// <summary>
        /// Create a dropdown menu at the specified position, assign it to a parent <c>Component</c> and return it.
        /// </summary>
        /// <remarks>
        /// Uses <c>STANDARD_UI_ELEMENT_WIDTH</c> and <c>STANDARD_UI_ELEMENT_HEIGHT</c> to determine the
        /// width and height of this object. Using other values might lead to weird results.
        /// </remarks>
        /// <param name="parent">The dropdown menu's supposed parent.</param>
        /// <param name="name">The Unity name of the dropdown menu.</param>
        /// <param name="options">A list of availalbe options, as a string. The first one is automatically
        /// set as default when invoking this method.</param>
        /// <param name="x">The x-position of the dropdown menu, relative to the container's upper left corner.</param>
        /// <param name="y">The y-position of the dropdown menu, relative to the container's upper left corner.</param>
        /// <returns>The created dropdown menu.</returns>
        public static GameUISelectableDropdown AddDropdownToComponent(Component parent, string name, List<string> options, float x, float y)
        {
            GameUISelectableDropdown dropdown = GameObject.Instantiate(dropdownTemplate, parent.transform);
            dropdown.name = name;
            (dropdown.transform as RectTransform).anchorMin = new Vector2(0, 1);
            (dropdown.transform as RectTransform).anchorMax = new Vector2(0, 1);
            (dropdown.transform as RectTransform).offsetMin = new Vector2(x, -y - 50);
            (dropdown.transform as RectTransform).offsetMax = new Vector2(x + 250, -y);
            dropdown.SetOptions(options);
            dropdown.SetValue(options[0]);
            return dropdown;
        }

        /// <summary>
        /// Create a slider at the specified position, assign it to a parent <c>Component</c> and return it.
        /// </summary>
        /// <remarks>
        /// Uses <c>STANDARD_UI_ELEMENT_HEIGHT</c> to determine the
        /// width and height of this object. Using other values might lead to weird results.
        /// </remarks>
        /// <param name="parent">The sliders's supposed parent.</param>
        /// <param name="name">The Unity name of the slider.</param>
        /// <param name="value">The default value of the slider.</param>
        /// <param name="min">The minimum value the slider can be set to.</param>
        /// <param name="min">The maximum value the slider can be set to.</param>
        /// <param name="x">The x-position of the slider, relative to the container's upper left corner.</param>
        /// <param name="y">The y-position of the slider, relative to the container's upper left corner.</param>
        /// <param name="width">The width of the slider. Note that this includes the label at the side.</param>
        /// <returns>The created slider.</returns>
        public static GameUISelectableSlider AddSliderToComponent(Component parent, string name, int value, int min, int max, float x, float y, float width)
        {
            GameUISelectableSlider slider = GameObject.Instantiate(sliderTemplate, parent.transform);
            slider.Set(value, min, max);
            (slider.transform as RectTransform).anchorMin = new Vector2(0, 1);
            (slider.transform as RectTransform).anchorMax = new Vector2(0, 1);
            (slider.transform as RectTransform).offsetMin = new Vector2(x, -y - 50);
            (slider.transform as RectTransform).offsetMax = new Vector2(x + width, -y);
            return slider;
        }

        /// <summary>
        /// Assign a custom <c>ICustomDialogInputListener</c> to the specified <c>ScreenDialog</c>.
        /// </summary>
        /// <remarks>
        /// Each dialog can only have one associated listener. The listener method will be called
        /// before the main method and can stop the main method from executing if it returns <c>true</c>,
        /// Currently unused.
        /// </remarks>
        /// <param name="dialog">The key dialog.</param>
        /// <param name="listener">The assigned listener.</param>
        public static void SetCustomDialogInputListener(ScreenDialog dialog, ICustomDialogInputListener listener)
        {
            listeners.Add(dialog, listener);
        }

        /// <summary>
        /// Try to get the <c>ICustomDialogInputListener</c> assigned to the specified <c>ScreenDialog</c>.
        /// </summary>
        /// <remarks>
        /// Returns <c>null</c> if it cannot find an entry. Currently unused.
        /// </remarks>
        /// <param name="dialog">The key dialog.</param>
        /// <returns>The assigned <c>ICustomDialogInputListener</c> or <c>null</c> no key
        /// could be found</returns>
        public static ICustomDialogInputListener GetCustomDialogInputListener(ScreenDialog dialog)
        {
            ICustomDialogInputListener listener;
            if(listeners.TryGetValue(dialog, out listener))
            {
                return listener;
            }
            return null;
        }

    }
}
