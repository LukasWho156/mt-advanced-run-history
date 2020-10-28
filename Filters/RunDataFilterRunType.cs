using System.Collections.Generic;

namespace AdvancedRunHistory.Filters
{
    class RunDataFilterRunType : IRunDataFilter
    {
        public const int TYPE_ALL = 0;
        public const int TYPE_STANDARD = 1;
        public const int TYPE_EXPERT = 2;
        public const int TYPE_DAILY = 3;
        public const int TYPE_HELLRUSH = 4;
        public const int TYPE_CUSTOM = 5;
        public const int TYPE_SHARE = 6;

        public readonly List<string> options = new List<string> { "Any", "Standard", "Expert", "Daily", "Hell Rush", "Custom", "Shared" };

        public int SelectedRunType { private set; get; }

        public RunDataFilterRunType(int runType = TYPE_ALL)
        {
            SetDesiredRunType(runType);
        }

        public void SetDesiredRunType(int runType)
        {
            SelectedRunType = runType;
        }

        public bool IsEgligible(RunAggregateData runData)
        {
            RunType runType = runData.GetRunTypeEnum();
            switch (SelectedRunType)
            {
                case TYPE_STANDARD:
                    return runType == RunType.Class && runData.GetSpChallengeId().IsNullOrEmpty();
                case TYPE_EXPERT:
                    return runType == RunType.Class && !runData.GetSpChallengeId().IsNullOrEmpty();
                case TYPE_DAILY:
                    return runType == RunType.Daily;
                case TYPE_HELLRUSH:
                    return runType == RunType.Matchmaker;
                case TYPE_CUSTOM:
                    return runType == RunType.Custom;
                case TYPE_SHARE:
                    return runType == RunType.Share;
                default:
                    return true;
            }
        }
    }
}
