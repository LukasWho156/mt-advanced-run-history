using HarmonyLib;
using Mono.Data.Sqlite;
using RunHistory;
using ShinyShoe;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AdvancedRunHistory.Filters
{
	/// <summary>
	/// A patch that makes it possible to filter your run history with a variety of filters.
	/// </summary>
	/// <remarks>
	/// This is pretty ugly right now, unfortunately. Since RunMinDbEntry already returns only the requested page of results,
	/// there doesn't seem to be a way to filter out results any later than during this method. Also, the changes to the
	/// code need to be applied in the middle of the method, and unless I learn how Transpilers work, that means I have
	/// to rewrite the entire method.Oh well.At least it sort of works.
	/// Also, going through all the results and applying filters is probably pretty slow, especially if you have a lot of
	/// runs. I'm not sure how well filtering using SQL works, but it might be preferable in the long run.
	/// </remarks>
	[HarmonyPatch(typeof(RunMinDbEntry), "GetSortedPage")]
	public class DbGetPagePatch
	{
		public static bool Prefix(ref RunAggregateDatas __result,
			ref string database,
			ref string userId,
			ref string altUserId,
			ref int page,
			ref int numPerPage,
			ref HadesNetworkCaller.SortFieldName sortFieldName,
			ref HadesNetworkCaller.SortDirection sortDirection)
		{
			// Get the filter manager. I don't think there's a way to pass this as an argument, so we'll have to
			// set it manually.
			FilterManager manager = AdvancedRunHistory.filterManager;
			// This is just vanilla code.
			altUserId = altUserId ?? string.Empty;
			int environment = (int)AppManager.EnvironmentConfig.Environment;
			string commandText = string.Format("SELECT {0} FROM '{1}' WHERE ({2} = '{3}' OR {4} = '{5}') AND {6} = {7} ORDER BY {8} {9}", "runId", "runHistoryTable", "userId", userId, "userId", altUserId, "env", environment, sortFieldName, sortDirection);
			List<string> runIds = new List<string>();
			if (!SqlUtil.ExecuteReader(database, commandText, ErrorFile.RunMinDbEntry, 3, delegate (SqliteDataReader reader)
			{
				while (reader.Read())
				{
					runIds.Add(reader.GetString(0));
				}
				return true;
			}, exactlyOneRead: false))
			{
				__result = null;
				return false;
			}
			// The actual patch; start by assuming we don't want to filter.
			List<string> filteredRunIds = runIds;
			// Only filter runs if the manager is active-
			if(manager.Active)
            {
				// Make a new list of runIds.
				filteredRunIds = new List<String>();
				// Go through all runIds in the original list.
				foreach (string runId in runIds)
				{
					// Try to read the run from the DB.
					RunMinDbEntry runMinDbEntry = RunMinDbEntry.ReadFromDb(database, runId);
					if (runMinDbEntry != null)
					{
						// Create a RunAggregateData. Assume it's eglibile.
						RunAggregateData runAggregateData = runMinDbEntry.minimalRunData.CreateRunAggregateData();
						bool isEgligible = true;
						// Go through all filters and make the run uneglibile if it doesn't pass.
						foreach (IRunDataFilter filter in manager.Filters)
						{
							if (!filter.IsEgligible(runAggregateData))
							{
								isEgligible = false;
								break;
							}
						}
						// The run has passed all filters, add it to the filtered list.
						if (isEgligible)
						{
							filteredRunIds.Add(runId);
						}
					}
				}
			}
			// Back to vanilla code, but changing runIds --> filteredRunIds.
			List<string> list = new List<string>();
			List<RunAggregateData> list2 = new List<RunAggregateData>();
			for (int i = (page - 1) * numPerPage; i < filteredRunIds.Count; i++)
			{
				if (list2.Count >= numPerPage)
				{
					break;
				}
				RunMinDbEntry runMinDbEntry = RunMinDbEntry.ReadFromDb(database, filteredRunIds[i]);
				if (runMinDbEntry != null)
				{
					RunAggregateData runAggregateData = runMinDbEntry.minimalRunData.CreateRunAggregateData();
					string item = JsonUtility.ToJson(runAggregateData);
					list.Add(item);
					list2.Add(runAggregateData);
				}
			}
			int pageCount = filteredRunIds.Count / numPerPage + Math.Min(filteredRunIds.Count % numPerPage, 1);
			RunAggregateDatas runAggregateDatas = new RunAggregateDatas(list.ToArray(), pageCount, page);
			runAggregateDatas.SetRuns(list2.ToArray());
			__result = runAggregateDatas;
			return false;
		}
    }
}
