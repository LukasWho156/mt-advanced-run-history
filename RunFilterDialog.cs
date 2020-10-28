using HarmonyLib;
using AdvancedRunHistory.CustomUIStuff;
using AdvancedRunHistory.Filters;
using ShinyShoe;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace AdvancedRunHistory
{
    /// <summary>
    /// A hard-coded dialog for editing run data filters.
    /// </summary>
    /// <remarks>
    /// There's a lot of manual positioning and other hard-coded stuff right now. I should probably change that
    /// one of these days. Maybe figure out how Containers in Unity work or something. Anyway, it's working for
    /// now, but some refactoring might do this class good.
    /// </remarks>
    public class RunFilterDialog
    {
        // Layout stuff.
        private const int firstColumn = 150;
        private const int firstLine = 100;
        private const int columnWidth = 1130 - firstColumn * 2 - 250;
        private const int lineHeight = 70;
        
        // Some needed things.
        private ScreenDialog dialog;
        private Component content;
        private FilterManager filterManager;
        
        // A flag to keep track if a filter has been changed.
        private bool updated;

        // Keep a list of dropdowns and sliders to make calling their ApplyScreenInputs easier.
        private List<GameUISelectableDropdown> dropdowns;
        private List<GameUISelectableSlider> sliders;

        // A lot of filters and corresponding UI elements.
        private RunDataFilterOutcome outcomeFilter;
        private GameUISelectableDropdown outcomeFilterDropdown;

        private RunDataFilterRunType runTypeFilter;
        private GameUISelectableDropdown runTypeFilterDropdown;

        private RunDataFilterCovenant covenantFilter;
        private GameUISelectableSlider covenantMinSlider;
        private GameUISelectableSlider covenantMaxSlider;

        private RunDataFilterProgress progressFilter;
        private GameUISelectableSlider progressMinSlider;
        private GameUISelectableSlider progressMaxSlider;

        private RunDataFilterClan clanFilter1;
        private RunDataFilterClan clanFilter2;
        private GameUISelectableDropdown clanRoleDropdown;
        private GameUISelectableDropdown clanDropdown1;
        private GameUISelectableDropdown clanDropdown2;
        private TextMeshProUGUI secondClanLabel;

        /// <summary>
        /// Create a new filter dialog.
        /// </summary>
        /// <param name="parent">The container for this dialog. This probably doesn't need to be the RunHistoryScreen,
        /// but it seems reasonable to be. Should contain a child named "Content"</param>
        /// <param name="filterManager">The filter manager. Should be empty when this constructor is called.</param>
        public RunFilterDialog(RunHistoryScreen parent, FilterManager filterManager)
        {
            this.filterManager = filterManager;
            // (a little fail-safe to avoid duplicate filters in the filter manager if the constructor is called again)
            filterManager.Clear();
            Reinit(parent);
        }

        /// <summary>
        /// Reinitalize the dialog. Use this if the dialog has already been created, but the UI elements have been
        /// destroyed due to a screen transition.
        /// </summary>
        /// <param name="parent">The container for this dialog, see <see cref="RunFilterDialog.RunFilterDialog"/></param>
        public void Reinit(RunHistoryScreen parent)
        {
            this.dialog = CustomUIManager.CreateCustomDialog(parent, "FilterDialog", "Run Data Filters");
            content = dialog.transform.Find("Content");
            dropdowns = new List<GameUISelectableDropdown>();
            sliders = new List<GameUISelectableSlider>();
            MakeOutcomeFilter(firstColumn, firstLine);
            MakeRunTypeFilter(firstColumn, firstLine + lineHeight);
            MakeCovenantFilter(firstColumn, firstLine + lineHeight * 2);
            MakeClanFilter(firstColumn, firstLine + lineHeight * 4);
            MakeProgressFilter(firstColumn, firstLine + lineHeight * 6);
        }

        // A lot of methods to initalize filters and UI elements, some very similar code, not gonna comment them all.
        private void MakeOutcomeFilter(float xOffset, float yOffset)
        {
            // If the filter has not yet been created, do so and add it to the filter manager
            if (outcomeFilter == null)
            {
                outcomeFilter = new RunDataFilterOutcome();
                filterManager.AddFilter(outcomeFilter);
            }
            // UI stuff
            CustomUIManager.AddLabelToComponent(content, "Run Outcome:", xOffset, yOffset, 240, 50);
            outcomeFilterDropdown = CustomUIManager.AddDropdownToComponent(content, "OutcomeDropdown", outcomeFilter.options, xOffset + columnWidth, yOffset);
            outcomeFilterDropdown.optionChosenSignal.AddListener(HandleOutcome);
            // Manually set the dropdown menu to the current option chosen. There might be a less ugly way to do this.
            outcomeFilterDropdown.SetValue(outcomeFilter.options[outcomeFilter.Outcome]);
            dropdowns.Add(outcomeFilterDropdown);
        }

        private void MakeRunTypeFilter(float xOffset, float yOffset)
        {
            if (runTypeFilter == null)
            {
                runTypeFilter = new RunDataFilterRunType();
                filterManager.AddFilter(runTypeFilter);
            }
            CustomUIManager.AddLabelToComponent(content, "Run Type:", xOffset, yOffset, 240, 50);
            runTypeFilterDropdown = CustomUIManager.AddDropdownToComponent(content, "RunTypeDropdown", runTypeFilter.options, xOffset + columnWidth, yOffset);
            runTypeFilterDropdown.optionChosenSignal.AddListener(HandleRunType);
            runTypeFilterDropdown.SetValue(runTypeFilter.options[runTypeFilter.SelectedRunType]);
            dropdowns.Add(runTypeFilterDropdown);
        }

        private void MakeCovenantFilter(float xOffset, float yOffset)
        {
            if (covenantFilter == null)
            {
                covenantFilter = new RunDataFilterCovenant();
                filterManager.AddFilter(covenantFilter);
            }
            CustomUIManager.AddLabelToComponent(content, "Min Covenant:", xOffset, yOffset, 240, 50);
            CustomUIManager.AddLabelToComponent(content, "Max Covenant:", xOffset, yOffset + 70, 240, 50);
            covenantMinSlider = CustomUIManager.AddSliderToComponent(content, "MinCovenantSlider", 0, 0, 25, xOffset + columnWidth, yOffset, 240);
            covenantMaxSlider = CustomUIManager.AddSliderToComponent(content, "MaxCovenantSlider", 25, 0, 25, xOffset + columnWidth, yOffset + lineHeight, 240);
            covenantMinSlider.ValueSetSignal.AddListener(HandleMinCovenant);
            covenantMaxSlider.ValueSetSignal.AddListener(HandleMaxCovenant);
            UpdateCovenantFilter();
            sliders.Add(covenantMinSlider);
            sliders.Add(covenantMaxSlider);
        }

        // If UI elements can be changed by other UI elements, implement Update functions (in this case,
        // make sure that the max value is always >= the min value).
        private void UpdateCovenantFilter()
        {
            covenantMinSlider.Set(covenantFilter.MinValue, 0, 25);
            covenantMaxSlider.Set(covenantFilter.MaxValue, 0, 25);
        }

        // This is probably the weirdest one, so some comments here.
        private void MakeClanFilter(float xOffset, float yOffset)
        {
            // As usual, start with initalizing the filters if they aren't yet.
            // (Probably should use two if-statements as a fail-safe, but there should be no way only one of them
            // is ever set and not the other)
            if(clanFilter1 == null || clanFilter2 == null)
            {
                clanFilter1 = new RunDataFilterClan(RunDataFilterClan.CLAN_ANY, RunDataFilterClan.AS_PRIMARY);
                filterManager.AddFilter(clanFilter1);
                clanFilter2 = new RunDataFilterClan(RunDataFilterClan.CLAN_ANY, RunDataFilterClan.AS_PRIMARY);
                filterManager.AddFilter(clanFilter2);
            }
            // First clan: Choose the clan as well as the role it should play.
            clanRoleDropdown = CustomUIManager.AddDropdownToComponent(content, "ClanRoleDropdown", RunDataFilterClan.roleOptions, xOffset - 10, yOffset);
            CustomUIManager.AddLabelToComponent(content, "Clan:", xOffset + 260, yOffset, 100, 50);
            clanDropdown1 = CustomUIManager.AddDropdownToComponent(content, "ClanDropdown1", RunDataFilterClan.clanOptions, xOffset + columnWidth, yOffset);
            // For the second clan, the role directly results from the first clan's role, so we can use a simple label.
            // However, we need to update that label accordingly if the first clan's role is changed, see below.
            secondClanLabel = CustomUIManager.AddLabelToComponent(content, "Secondary Clan:", xOffset, yOffset + 70, 350, 50);
            clanDropdown2 = CustomUIManager.AddDropdownToComponent(content, "ClanDropdown1", RunDataFilterClan.clanOptions, xOffset + columnWidth, yOffset + lineHeight);
            // Add listeners.
            clanRoleDropdown.optionChosenSignal.AddListener(HandleClanRole);
            clanDropdown1.optionChosenSignal.AddListener(HandleClan1);
            clanDropdown2.optionChosenSignal.AddListener(HandleClan2);
            // If the current filter settings are not the default ones, we need to update the UI elements. As we also
            // need to do this whenever the first clan's role is changed, put it in a seperate method.
            UpdateClanFilter();
            // "Register" the UI elements.
            dropdowns.Add(clanRoleDropdown);
            dropdowns.Add(clanDropdown1);
            dropdowns.Add(clanDropdown2);
        }

        private void UpdateClanFilter()
        {
            clanRoleDropdown.SetValue(RunDataFilterClan.roleOptions[clanFilter1.Role]);
            // NOTE: 2 - clanFilter1.Role only works with the current constants. This will break if I ever change
            // how they work.
            secondClanLabel.text = RunDataFilterClan.roleOptions[2 - clanFilter1.Role] + " Clan:";
            clanDropdown1.SetValue(RunDataFilterClan.clanOptions[clanFilter1.Clan]);
            clanDropdown2.SetValue(RunDataFilterClan.clanOptions[clanFilter2.Clan]);
        }

        private void MakeProgressFilter(float xOffset, float yOffset)
        {
            if (progressFilter == null)
            {
                progressFilter = new RunDataFilterProgress();
                filterManager.AddFilter(progressFilter);
            }
            CustomUIManager.AddLabelToComponent(content, "Min Ring:", xOffset, yOffset, 240, 50);
            CustomUIManager.AddLabelToComponent(content, "Max Ring:", xOffset, yOffset + 70, 240, 50);
            progressMinSlider = CustomUIManager.AddSliderToComponent(content, "MinProgressSlider", 0, 0, 25, xOffset + columnWidth, yOffset, 240);
            progressMaxSlider = CustomUIManager.AddSliderToComponent(content, "MaxProgressSlider", 25, 0, 25, xOffset + columnWidth, yOffset + lineHeight, 240);
            progressMinSlider.ValueSetSignal.AddListener(HandleMinProgress);
            progressMaxSlider.ValueSetSignal.AddListener(HandleMaxProgress);
            UpdateProgressFilter();
            sliders.Add(progressMinSlider);
            sliders.Add(progressMaxSlider);
        }

        private void UpdateProgressFilter()
        {
            progressMinSlider.Set(progressFilter.MinValue, 1, 9);
            progressMaxSlider.Set(progressFilter.MaxValue, 1, 9);
        }

        /// <summary>
        /// Open the associated <c>ScreenDialog</c>.
        /// </summary>
        public void Open()
        {
            dialog.Open();
        }

        /// <summary>
        /// Is this dialog currently active?
        /// </summary>
        /// <returns><c>true</c> if active, otherwise <c>false</c>.</returns>
        public bool IsActive()
        {
            return dialog.Active;
        }

        // A few listener methods. Most of them are pretty straightforward: If a UI element is updated, also update
        // the associated filter. Set a flag that a filter has been updated.
        private void HandleOutcome(int index, string value)
        {
            outcomeFilter.SetDesiredOutcome(index);
            Update();
        }

        private void HandleRunType(int index, string value)
        {
            runTypeFilter.SetDesiredRunType(index);
            Update();
        }

        private void HandleMinCovenant(int value)
        {
            covenantFilter.MinValue = value;
            UpdateCovenantFilter();
            Update();
        }

        private void HandleMaxCovenant(int value)
        {
            covenantFilter.MaxValue = value;
            UpdateCovenantFilter();
            Update();
        }

        private void HandleMinProgress(int value)
        {
            progressFilter.MinValue = value;
            UpdateProgressFilter();
            Update();
        }

        private void HandleMaxProgress(int value)
        {
            progressFilter.MaxValue = value;
            UpdateProgressFilter();
            Update();
        }

        // The only tricky one, as this also changes the second clan's role.
        private void HandleClanRole(int index, string value)
        {
            clanFilter1.SetRole(index);
            clanFilter2.SetRole(2 - index);
            UpdateClanFilter();
            Update();
        }

        private void HandleClan1(int index, string value)
        {
            clanFilter1.SetClan(index);
            Update();
        }

        private void HandleClan2(int index, string value)
        {
            clanFilter2.SetClan(index);
            Update();
        }

        /// <summary>
        /// Check whether any of the filter settings were updated since the last time this was called.
        /// </summary>
        /// <remarks>
        /// Currently, that's not exactly what it does. It only checks if a flag has been set and clears it
        /// if that's the case. As long as every time a UI element is updated, it calls the <c>Update()</c> method,
        /// it's fine, but I guess it's not best practise.
        /// </remarks>
        /// <returns></returns>
        public bool WasUpdated()
        {
            if(updated)
            {
                updated = false;
                return true;
            }
            return false;
        }

        // Set the updated flag to <c>true</c>.
        private void Update()
        {
            updated = true;
        }

        /// <summary>
        /// Evaluate the user input. For params, see <see cref="ICustomDialogInputListener.ApplyScreenInput"/>
        /// </summary>
        public bool ApplyScreenInput(CoreInputControlMapping mapping, IGameUIComponent triggeredUI, InputManager.Controls triggeredMappingID)
        {
            // Invoke the ScreenDialog's ApplyScreenInput method. This is mostly to handle correct closing.
            dialog.ApplyScreenInput(mapping, triggeredUI, triggeredMappingID);
            // Evaluate all dropdown menus.
            foreach(GameUISelectableDropdown dropdown in dropdowns)
            {
                if(dropdown.TryClose(triggeredUI, triggeredMappingID) || dropdown.ApplyScreenInput(mapping, triggeredUI, triggeredMappingID))
                {
                    return true;
                }
            }
            // Evaluate all sliders.
            foreach (GameUISelectableSlider slider in sliders)
            {
                if (slider.ApplyScreenInput(mapping, triggeredUI, triggeredMappingID))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
