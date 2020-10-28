using BepInEx;
using HarmonyLib;
using ShinyShoe;
using BepInEx.Logging;
using AdvancedRunHistory.CustomUIStuff;
using System.Reflection;
using AdvancedRunHistory.Filters;

namespace AdvancedRunHistory
{
    /// <summary>
    /// The main class of the Advanced Run History mod.
    /// </summary>
    /// <remarks>
    /// This mod aims to add a few options to the Run History screen, which I find to be a bit lacking in the
    /// base game. At the moment, it implements the possibility to filter runs so that only runs that fulfill
    /// certain criteria are returned by the search function.
    /// A downside of this is that the current implementation requires <c>RunAggregateData</c> objects to be
    /// instantiated from database entries, which slows down the search process by a bit.
    /// Right now, a lot of information like cards, relics, etc. cannot be retrieved from the run data objects
    /// during the filtering process. This is because only minimal run data is returned while the game searches
    /// for runs. There might be a workaround for this, but it would probably slow down the game significantly.
    /// This class itself is only used for logging and holding a few variables that are globally needed.
    /// Everything else has been moved to some other place.
    /// </remarks>
    [BepInPlugin(MOD_ID, MOD_NAME, MOD_VERSION)]
    [BepInProcess("MonsterTrain.exe")]
    [BepInProcess("MtLinkHandler.exe")]
    public class AdvancedRunHistory : BaseUnityPlugin
    {
        // Just some constants
        public const string MOD_ID = "luc.mods.runfilters";
        public const string MOD_NAME = "Advanced Run History";
        public const string MOD_VERSION = "1.0.0";

        private static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource(MOD_NAME);
        public static FilterManager filterManager = new FilterManager();
        public static RunFilterDialog filterDialog;

        public static GameUISelectableButton openFilterDialogButton;
        public static GameUISelectableToggle applyFiltersToggle;

        private void Awake()
        {
            // Apply patches.
            var harmony = new Harmony(MOD_ID);
            harmony.PatchAll();
        }

        /// <summary>
        /// Use BepInEx to log stuff.
        /// </summary>
        /// <param name="message">The message you want to log.</param>
        /// <param name="level">The logging level. Default: Debug.</param>
        public static void Log(string message, LogLevel level = LogLevel.Debug)
        {
            logger.Log(level, message);
        }
    }

    /// <summary>
    /// This patch adds a few custom UI elements to the Run History screen and initalized the Run Filter dialog.
    /// </summary>
    [HarmonyPatch(typeof(RunHistoryScreen), "Setup")]
    public class RunHistoryUIPatch
    {
        public static void Postfix(ref RunHistoryScreen __instance)
        {
            // Add a few UI elements. TODO: Currenty, these are added directly to the screen. It might be nicer to add
            // them to the screen's content, but I'm too lazy for it right now.
            CustomUIManager.AddLabelToComponent(__instance, "Apply Filters:", 170, 145, 200, 50);
            AdvancedRunHistory.applyFiltersToggle = CustomUIManager.AddToggleToComponent(__instance, "ApplyFiltersToggle", 310, 146);
            AdvancedRunHistory.applyFiltersToggle.isOn = AdvancedRunHistory.filterManager.Active;
            AdvancedRunHistory.openFilterDialogButton = CustomUIManager.AddButtonToComponent(__instance, "OpenFilterScreenButton", "Edit Filters", 570, 140, 250, 60);
            // If the Filter dialog has not yet been created, do so. Otherwise, reinitalize it.
            if (AdvancedRunHistory.filterDialog == null)
            {
                AdvancedRunHistory.filterDialog = new RunFilterDialog(__instance, AdvancedRunHistory.filterManager);
            } else
            {
                AdvancedRunHistory.filterDialog.Reinit(__instance);
            }
        }
    }


    /// <summary>
    /// This patch adds handling for the custom UI elements added to the Run History screen.
    /// </summary>
    [HarmonyPatch(typeof(RunHistoryScreen), "ApplyScreenInput")]
    public class RunHistoryInputPatch
    {
        public static bool Prefix(ref RunHistoryScreen __instance, ref bool __result, ref CoreInputControlMapping mapping, ref IGameUIComponent triggeredUI, ref InputManager.Controls triggeredMappingID)
        {
            // If the filter dialog is open, handle it first.
            if(AdvancedRunHistory.filterDialog.IsActive())
            {
                if(AdvancedRunHistory.filterDialog.ApplyScreenInput(mapping, triggeredUI, triggeredMappingID))
                {
                    // If one of the filters was changed, re-fetch runs.
                    if(AdvancedRunHistory.filterDialog.WasUpdated() && AdvancedRunHistory.filterManager.Active)
                    {
                        UpdateHistoryUI(__instance);
                    }
                    __result = true;
                }
                return false;
            }
            // "Edit Filters" button clicked: Open the filter dialog.
            if (triggeredMappingID == InputManager.Controls.Clicked && triggeredUI.IsGameUIComponent(AdvancedRunHistory.openFilterDialogButton))
            {
                AdvancedRunHistory.filterDialog.Open();
                __result = true;
                return false;
            }
            // "Apply Filters" toggle clicked: Toggle the filter manager's active state and re-fetch runs.
            if(triggeredMappingID == InputManager.Controls.Clicked && triggeredUI.IsGameUIComponent(AdvancedRunHistory.applyFiltersToggle)) {
                AdvancedRunHistory.Log("Hi!");
                AdvancedRunHistory.filterManager.Active = AdvancedRunHistory.applyFiltersToggle.Toggle();
                UpdateHistoryUI(__instance);
                return false;
            }
            // If nothing else has been found, run the original method.
            return true;
        }

        // A quick method to make the Run History UI re-fetch runs.
        private static void UpdateHistoryUI(RunHistoryScreen screen)
        {
            RunHistoryUI ui = Traverse.Create(screen).Field("runHistoryUI").GetValue<RunHistoryUI>();
            MethodInfo fetch = ui.GetType().GetMethod("FetchGameRuns", BindingFlags.NonPublic | BindingFlags.Instance);
            fetch.Invoke(ui, new object[] { 1 });
        }
    }

}
