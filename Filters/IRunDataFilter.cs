namespace AdvancedRunHistory.Filters
{
    /// <summary>
    /// An interface for Run Data Filters, i.e. filters that check if a given run fullfils certain requirements.
    /// </summary>
    public interface IRunDataFilter
    {
        /// <summary>
        /// Check whether a given run defined by a <c>RunAggregateData</c> object fulfills the requirements
        /// of this filter.
        /// </summary>
        /// <param name="runData">The run. Note that the patch that uses this calls this method with a "minimal run
        /// data" object, meaning that it lacks certain informaiton like cards, relics and which battles were
        /// fought. Changing would probably both be tricky and very slow.</param>
        /// <returns><c>true</c> if the run fulfills the filter's requirements, <c>false</c>
        /// otherwise.</returns>
        bool IsEgligible(RunAggregateData runData);
    }
}
