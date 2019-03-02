/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
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
using System;

namespace SeeingSharp.Util
{
    public abstract class PerformanceCalculatorBase
    {
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
        /// Initializes a new instance of the <see cref="PerformanceCalculatorBase"/> class.
        /// </summary>
        /// <param name="calculatorName">Name of the calculator.</param>
        internal PerformanceCalculatorBase(string calculatorName)
        {
            CalculatorName = calculatorName;
        }

        /// <summary>
        /// Gets the parent kpi container.
        /// </summary>
        public PerformanceAnalyzer Parent { get; internal set; }

        /// <summary>
        /// Gets the name of this calculator.
        /// </summary>
        public string CalculatorName { get; }
    }
}