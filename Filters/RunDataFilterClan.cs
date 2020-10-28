using BepInEx.Logging;
using System;
using System.Collections.Generic;

namespace AdvancedRunHistory.Filters
{
    /// <summary>
    /// A filter that checks whether a certain clan fought during that run and what role (primary, secondary, either)
    /// it played.
    /// </summary>
    class RunDataFilterClan : IRunDataFilter
    {
        // Clan type ints are now created automatically, so no more consts.
        public const int CLAN_ANY = 0;

        // Clan roles, i.e. primary, secondary or either
        public const int AS_PRIMARY = 0;
        public const int AS_ANY = 1;
        public const int AS_SECONDARY = 2;

        // RunAggregateData objects seem to store clans only as string ids, so let's set up a dictionary.
        private readonly Dictionary<string, int> clanKeys = new Dictionary<string, int>();

        /// <summary>
        /// Which clan to look for, e.g. Hellhorned, Awoken, ...
        /// </summary>
        public int Clan { private set; get; }
        /// <summary>
        /// Which role the clan should have played in that run, i.e. primary, secondary or either.
        /// </summary>
        public int Role { private set; get; }

        /// <summary>
        /// Options for the clan dropdown menu.
        /// </summary>
        public readonly List<string> clanOptions = new List<string> { "Any" };
        /// <summary>
        /// Options for the clan role menu.
        /// </summary>
        public static readonly List<string> roleOptions = new List<String> { "Primary", "Either", "Secondary" };

        /// <summary>
        /// Creates a new clan filter.
        /// </summary>
        /// <param name="clan">Which clan to look for.</param>
        /// <param name="role">Which role it should play.</param>
        public RunDataFilterClan(SaveManager saveManager, int clan = CLAN_ANY, int role = AS_ANY)
        {
            // Yeah, hard-coding ran into problems with custom clans. Let's do this properly.
            List<ClassData> allClans = saveManager.GetAllGameData().GetAllClassDatas();
            int i = 1;
            foreach(ClassData data in allClans)
            {
                clanKeys.Add(data.GetID(), i);
                clanOptions.Add(data.GetTitle().Split(' ')[0]);
                i++;
            }
            SetClan(clan);
            SetRole(role);
        }

        /// <summary>
        /// Set the filter's desired clan.
        /// </summary>
        /// <param name="clan">The clan's integer id, see constants at the top of this file</param>
        public void SetClan(int clan)
        {
            this.Clan = clan;
        }

        /// <summary>
        /// Set the filter's desired clan role.
        /// </summary>
        /// <param name="role">The role's integer id, see constants at the top this file</param>
        public void SetRole(int role)
        {
            this.Role = role;
        }

        public bool IsEgligible(RunAggregateData runData)
        {
            // If no clan is defined, it doesn't matter which role it shall play.
            if(Clan == CLAN_ANY)
            {
                return true;
            }
            // Otherwise, try to find both clan ids ...
            int primary = 0; int secondary = 0;
            if(!clanKeys.TryGetValue(runData.GetMainClassID(), out primary))
            {
                AdvancedRunHistory.Log("Can't find primary clan ID " + runData.GetMainClassID(), LogLevel.Warning);
            }
            if (!clanKeys.TryGetValue(runData.GetSubClassID(), out secondary))
            {
                AdvancedRunHistory.Log("Can't find secondary clan ID " + runData.GetSubClassID(), LogLevel.Warning);
            }
            // ... and check whether they are the desired ones.
            switch (Role)
            {
                case AS_ANY:
                    return primary == Clan || secondary == Clan;
                case AS_PRIMARY:
                    return primary == Clan;
                case AS_SECONDARY:
                    return secondary == Clan;
                default:
                    return false;
            }
        }
    }
}
