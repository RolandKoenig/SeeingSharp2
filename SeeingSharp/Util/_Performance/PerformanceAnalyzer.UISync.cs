#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/
#endregion

namespace SeeingSharp.Util
{
    #region using

    using System.Collections.ObjectModel;

    #endregion

    public partial class PerformanceAnalyzer
    {
        /// <summary>
        /// Handles the given result object for the UI layer.
        /// </summary>
        /// <param name="actResult">The result to be processed.</param>
        private void HandleResultForUI(PerformanceAnalyzeResultBase actResult)
        {
            // Handle duration kpi results
            var actDurationResult = actResult as DurationPerformanceResult;

            if (actDurationResult != null)
            {
                HandleResultForUIForFlowRateKpi(
                    actDurationResult,
                    m_uiDurationKpisHistorical, m_uiDurationKpisCurrents);
                return;
            }

            // Put here other result handlers
            var actFlowRateResult = actResult as FlowRatePerformanceResult;

            if (actFlowRateResult != null)
            {
                HandleResultForUIForFlowRateKpi(
                    actFlowRateResult,
                    m_uiFlowRateKpisHistorical, m_uiFlowRateKpisCurrents);
                return;
            }
        }

        /// <summary>
        /// Handles the given result object for the UI layer.
        /// </summary>
        private void HandleResultForUIForFlowRateKpi<T>(
            T kpiResult,
            ObservableCollection<T> kpisHistorical,
            ObservableCollection<T> kpisCurrents)
            where T : PerformanceAnalyzeResultBase
        {
            // Handle historical entries
            if (m_generateHistoricalCollection)
            {
                var actResultCount = 1;
                kpisHistorical.Add(kpiResult);
                for (var loop = kpisHistorical.Count - 1; loop >= 0; loop--)
                {
                    if (kpisHistorical[loop].Calculator == kpiResult.Calculator)
                    {
                        actResultCount++;
                    }

                    if (actResultCount > m_maxCountHistoricalEntries)
                    {
                        kpisHistorical.RemoveAt(loop);
                    }
                }
            }

            // Handle most current entires
            if (m_generateCurrentValueCollection)
            {
                kpisCurrents.Add(kpiResult);
                for (var loop = kpisCurrents.Count - 1; loop >= 0; loop--)
                {
                    if ((kpisCurrents[loop] != kpiResult) &&
                        (kpisCurrents[loop].Calculator == kpiResult.Calculator))
                    {
                        kpisCurrents.RemoveAt(loop);
                    }
                }
            }
        }
    }
}