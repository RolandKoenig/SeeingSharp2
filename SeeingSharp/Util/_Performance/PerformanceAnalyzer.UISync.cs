#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeeingSharp.Util
{
    public partial class PerformanceAnalyzer
    {
        /// <summary>
        /// Handles the given result object for the UI layer.
        /// </summary>
        /// <param name="actResult">The result to be processed.</param>
        private void HandleResultForUI(PerformanceAnalyzeResultBase actResult)
        {
            // Handle duration kpi results
            DurationPerformanceResult actDurationResult = actResult as DurationPerformanceResult;
            if (actDurationResult != null)
            {
                HandleResultForUIForFlowRateKpi(
                    actDurationResult,
                    m_uiDurationKpisHistorical, m_uiDurationKpisCurrents);
                return;
            }

            // Put here other result handlers
            FlowRatePerformanceResult actFlowRateResult = actResult as FlowRatePerformanceResult;
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
                int actResultCount = 1;
                kpisHistorical.Add(kpiResult);
                for (int loop = kpisHistorical.Count - 1; loop >= 0; loop--)
                {
                    if (kpisHistorical[loop].Calculator == kpiResult.Calculator) { actResultCount++; }
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
                for (int loop = kpisCurrents.Count - 1; loop >= 0; loop--)
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