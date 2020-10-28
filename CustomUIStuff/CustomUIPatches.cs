using HarmonyLib;
using ShinyShoe;

namespace AdvancedRunHistory.CustomUIStuff
{

    /// <summary>
    /// This Patch initializes a bunch of UI templates for custom usage.
    /// </summary>
    /// <remarks>
    /// This is currently done pretty hackily: At the beginning of the run, load the settings screen to make
    /// sure its elements are loaded and then use a bunch of them as templates. The actual templating is done
    /// in <see cref="CustomUIManager.InitalizeTemplates"/>
    /// </remarks>
    [HarmonyPatch(typeof(MainMenuScreen), "Initialize")]
    public class InitCustomUIElementsPatch
    {
        public static void Postfix(ref MainMenuScreen __instance)
        {
            // We only need to do this once. In fact, not checking this probably results in the game freezing due
            // to an infinite recursion.
            if (!CustomUIManager.AreTemplatesInitalized())
            {
                // Find the current ScreenManager to access the Settigns Screen and Settings Dialog, as the Settings
                // Dialog contains a lot of UI elements and is helpful in creating templates.
                ScreenManager screenManager = Traverse.Create(Traverse.Create(__instance).Field("gameStateManager").GetValue<GameStateManager>()).Field("screenManager").GetValue<ScreenManager>();
                SettingsScreen settingsScreen = (SettingsScreen)screenManager.GetScreen(ScreenName.Settings);
                SettingsDialog settingsDialog = Traverse.Create(settingsScreen).Field("settingsDialog").GetValue<SettingsDialog>();
                // We need to actually open the Settings Dialog in order for the UI elements to initalize correctly.
                screenManager.ShowScreen(ScreenName.Settings);
                settingsDialog.Open();
                settingsDialog.Close();
                // Use the Patch Notes Dialog as a template for custom dialogs
                PatchNotesUI patch = Traverse.Create(__instance).Field("patchNotesDialog").GetValue<PatchNotesUI>();
                ScreenDialog dialogTemplate = Traverse.Create(patch).Field("dialog").GetValue<ScreenDialog>();
                CustomUIManager.InitalizeTemplates(settingsScreen, dialogTemplate, settingsDialog);
                // Pretend that nothing has happend and return to the main menu.
                screenManager.ReturnToMainMenu();
            }
        }
    }

    /// <summary>
    /// This Patch overrides the <c>ApplyScreenInput</c> method of a Screen Dialog so that it calls the
    /// <c>ApplyScreenInput</c> method of an <c>ICustomDialogInputListener</c> first if there is one
    /// assigned to the dialog.
    /// </summary>
    /// <remarks>
    /// Currently, this patch is unused as I found it more practical to just have a container dialog class and
    /// invoke its <c>ApplyScreenInput</c> method directly. I'll leave it in the codebase for now
    /// in case there's ever a use for it, since there are no apparent breaks from leaving it in.
    /// </remarks>
    [HarmonyPatch(typeof(ScreenDialog), "ApplyScreenInput")]
    public class HandleCustomDialogInputPatch
    {
        public static void Postfix(ref bool __result, ref ScreenDialog __instance, ref CoreInputControlMapping mapping, ref IGameUIComponent triggeredUI, ref InputManager.Controls triggeredMappingID)
        {
            // Check if there's a custom input listener assigned to this dialog. If that is the case, invoke its
            // ApplyScreenInput.
            ICustomDialogInputListener listener = CustomUIManager.GetCustomDialogInputListener(__instance);
            if(listener != null)
            {
                __result = listener.ApplyScreenInput(mapping, triggeredUI, triggeredMappingID);
            }
        }
    }
}
