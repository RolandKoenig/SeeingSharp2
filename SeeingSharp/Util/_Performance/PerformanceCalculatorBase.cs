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

    using System;

    #endregion

    public abstract class PerformanceCalculatorBase
    {
        private PerformanceAnalyzer m_parent;
        private string m_calculatorName;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceCalculatorBase"/> class.
        /// </summary>
        /// <param name="calculatorName">Name of the calculator.</param>
        internal PerformanceCalculatorBase(string calculatorName)
        {
            m_calculatorName = calculatorName;
        }

        /// <summary>
        /// Calculates a new kpi value based on given timestamp parameters.
        /// </summary>
        /// <param name="keyTimeStamp">The timestamp which is used for the result object.</param>
        /// <param name="minTimeStamp">The timestamp which is the minimum for current calculation step.</param>
        /// <param name="maxTimeStamp">The maximum timestamp up to which to calculate the next kpi.</param>
        /// <param name="calculationInterval">The interval from which to take all values from.</param>
        internal virtual PerformanceAnalyzeResultBase Calculate(
            DateTime keyTimeStamp, 
            DateTime minTimeStamp, DateTime maxTimeStamp,
            TimeSpan calculationInterval)
        {
            return null;
        }

        /// <summary>
        /// Gets the parent kpi container.
        /// </summary>
        public PerformanceAnalyzer Parent
        {
            get { return m_parent; }
            internal set { m_parent = value; }
        }
        
        /// <summary>
        /// Gets the name of this calculator.
        /// </summary>
        public string CalculatorName
        {
            get { return m_calculatorName; }
        }
    }
}