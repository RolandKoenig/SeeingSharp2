#region License information
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
#endregion

using System;

namespace SeeingSharp.Util
{
    #region using
    #endregion

    public abstract class PerformanceAnalyzeResultBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceAnalyzeResultBase"/> class.
        /// </summary>
        /// <param name="calculator">The Calculator this result object belongs to.</param>
        /// <param name="keyTimestamp">The key of this result object..</param>
        internal PerformanceAnalyzeResultBase(PerformanceCalculatorBase calculator, DateTime keyTimestamp)
        {
            Calculator = calculator;
            KeyTimestamp = keyTimestamp;
        }

        /// <summary>
        /// Gets the calculator this value was generated on.
        /// </summary>
        public PerformanceCalculatorBase Calculator { get; }

        /// <summary>
        /// Gets the name of the calculator.
        /// </summary>
        public string CalculatorName => Calculator.CalculatorName;

        /// <summary>
        /// Gets the key of this kpi.
        /// </summary>
        public DateTime KeyTimestamp { get; }
    }
}