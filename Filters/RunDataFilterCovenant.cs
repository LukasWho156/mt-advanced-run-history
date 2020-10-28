namespace AdvancedRunHistory.Filters
{
    /// <summary>
    /// A filter that checks whether the run was at a covenant level within a certain range.
    /// </summary>
    class RunDataFilterCovenant : RunDataFilterRanged
    {
        /// <summary>
        /// Create a new covenant filter. Works automatically.
        /// </summary>
        public RunDataFilterCovenant()
        {
            // Hard-code the available covenants, because why not.
            SetRange(0, 25);
        }

        protected override int GetCheckedValue(RunAggregateData runData)
        {
            return runData.GetAscensionLevel();
        }
    }
}
