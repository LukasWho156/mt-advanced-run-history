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
        // Clan types
        public const int CLAN_ANY = 0;
        public const int CLAN_HELLHORNED = 1;
        public const int CLAN_AWOKEN = 2;
        public const int CLAN_STYGIAN = 3;
        public const int CLAN_UMBRA = 4;
        public const int CLAN_MELTING = 5;

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
        public static readonly List<string> clanOptions = new List<String> { "Any", "Hellhorned", "Awoken", "Stygian", "Umbra", "Melting" };
        /// <summary>
        /// Options for the clan role menu.
        /// </summary>
        public static readonly List<string> roleOptions = new List<String> { "Primary", "Either", "Secondary" };

        /// <summary>
        /// Creates a new clan filter.
        /// </summary>
        /// <param name="clan">Which clan to look for.</param>
        /// <param name="role">Which role it should play.</param>
        public RunDataFilterClan(int clan = CLAN_ANY, int role = AS_ANY)
        {
            // I got these keys from printing them to the console. I sure hope they are generally applicable.
            // There might also be a nicer way to find them in-game, but eh.
            clanKeys.Add("c595c344-d323-4cf1-9ad6-41edc2aebbd0", CLAN_HELLHORNED);
            clanKeys.Add("fd119fcf-c2cf-469e-8a5a-e9b0f265560d", CLAN_AWOKEN);
            clanKeys.Add("9317cf9a-04ec-49da-be29-0e4ed61eb8ba", CLAN_STYGIAN);
            clanKeys.Add("4fe56363-b1d9-46b7-9a09-bd2df1a5329f", CLAN_UMBRA);
            clanKeys.Add("fda62ada-520e-42f3-aa88-e4a78549c4a2", CLAN_MELTING);
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
            // Otherwise check if the run has the specified clan in the specified role.
            switch(Role)
            {
                case AS_ANY:
                    return clanKeys[runData.GetMainClassID()] == Clan || clanKeys[runData.GetSubClassID()] == Clan;
                case AS_PRIMARY:
                    return clanKeys[runData.GetMainClassID()] == Clan;
                case AS_SECONDARY:
                    return clanKeys[runData.GetSubClassID()] == Clan;
                default:
                    return false;
            }
        }
    }
}
