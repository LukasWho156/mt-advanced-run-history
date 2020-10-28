namespace AdvancedRunHistory.Filters
{
    /// <summary>
    /// A filter that checks if the ring this run has made it to is within a certain range.
    /// </summary>
    class RunDataFilterProgress : RunDataFilterRanged
    {
        /// <summary>
        /// Create a new progress filter.
        /// </summary>
        public RunDataFilterProgress()
        {
            // Again, just hard-code this.
            SetRange(1, 9);
        }

        protected override int GetCheckedValue(RunAggregateData runData)
        {
            return runData.GetNumBattlesWon() + 1;
        }
    }
}
