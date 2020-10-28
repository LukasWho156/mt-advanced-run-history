using System;

namespace AdvancedRunHistory.Filters
{
    /// <summary>
    /// Base class for filters that check whether a certain value is within a specified range.
    /// </summary>
    /// <remarks>
    /// Note that if you change either the max or the min value of the range, this class automatically
    /// adjusts the range so that maxValue >= minValue holds true.
    /// </remarks>
    public abstract class RunDataFilterRanged : IRunDataFilter
    {
        // possible range
        private int rangeStart;
        private int rangeEnd;
        // specified range
        private int minValue;
        private int maxValue;

        /// <summary>
        /// The lower bound of the specified range. If you set this to a value higher than the current upper bound,
        /// the upper bound is set to the same value.
        /// </summary>
        public int MinValue { get { return minValue; }
            set
            {
                minValue = Math.Max(rangeStart, value);
                maxValue = Math.Max(value, maxValue);
            } }
        /// <summary>
        /// The upper bound of the specified range. If you set this to a value lower than the current lower bound,
        /// the lower bound is set to the same value.
        /// </summary>
        public int MaxValue { get { return maxValue; }
            set
            {
                maxValue = Math.Min(rangeEnd, value);
                minValue = Math.Min(value, minValue);
            }
        }

        /// <summary>
        /// Extract the value from the run data that shall be checked against the specified range.
        /// </summary>
        /// <param name="runData">The run data</param>
        /// <returns>The value that shall be checked against the specified range.</returns>
        protected abstract int GetCheckedValue(RunAggregateData runData);

        protected void SetRange(int start, int end)
        {
            rangeStart = start;
            rangeEnd = end;
            minValue = start;
            maxValue = end;
        }

        public bool IsEgligible(RunAggregateData runData)
        {
            int check = GetCheckedValue(runData);
            return check >= minValue && check <= maxValue;
        }
    }
}
