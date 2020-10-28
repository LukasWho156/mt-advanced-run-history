using System.Collections.Generic;

namespace AdvancedRunHistory.Filters
{
    /// <summary>
    /// A manager class that keeps track of which filters to apply during a search operation.
    /// </summary>
    public class FilterManager
    {

        /// <summary>
        /// The filters that were added to this manager.
        /// </summary>
        public List<IRunDataFilter> Filters { get; private set; }
        /// <summary>
        /// Whether this manager is currently active.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Create a new, empty filter manager.
        /// </summary>
        public FilterManager()
        {
            Filters = new List<IRunDataFilter>();
        }

        /// <summary>
        /// Add a filter to this manager.
        /// </summary>
        /// <param name="filter">The filter to be added.</param>
        public void AddFilter(IRunDataFilter filter)
        {
            Filters.Add(filter);
        }

        /// <summary>
        /// Remove all filters from this manager.
        /// </summary>
        public void Clear()
        {
            Filters.Clear();
        }
    }
}
