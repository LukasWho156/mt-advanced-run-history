using System.Collections.Generic;

namespace AdvancedRunHistory.Filters
{
    /// <summary>
    /// A filter that checks whether a run was won or lost.
    /// </summary>
    public class RunDataFilterOutcome : IRunDataFilter
    {
        // One day I'll figure out how enums work. Until then, I'm gonna use constants.
        public const int OUTCOME_ALL = 0;
        public const int OUTCOME_VICTORY = 1;
        public const int OUTCOME_DEFEAT = 2;

        /// <summary>
        /// Options for an associated dropdown menu.
        /// </summary>
        public readonly List<string> options = new List<string> { "Any", "Victory", "Defeat" };
        /// <summary>
        /// The desired outcome of the run.
        /// </summary>
        public int Outcome { private set; get; }

        /// <summary>
        /// Create a new outcome filter.
        /// </summary>
        /// <param name="outcome">The desired outcome, any by default.</param>
        public RunDataFilterOutcome(int outcome = OUTCOME_ALL)
        {
            SetDesiredOutcome(outcome);
        }

        /// <summary>
        /// Set the desired outcome.
        /// </summary>
        /// <param name="outcome">The desired outcome id, see constants at the top of this file.</param>
        public void SetDesiredOutcome(int outcome)
        {
            this.Outcome = outcome;
        }

        public bool IsEgligible(RunAggregateData runData)
        {
            switch(Outcome)
            {
                case OUTCOME_VICTORY:
                    return runData.GetVictory();
                case OUTCOME_DEFEAT:
                    return !runData.GetVictory();
                default:
                    return true;
            }
        }
    }
}
