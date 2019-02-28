#region License information
/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwhise.
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

    public class FlowRatePerformanceResult : PerformanceAnalyzeResultBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlowRatePerformanceResult"/> class.
        /// </summary>
        /// <param name="calculator">The calculator.</param>
        /// <param name="keyTimestamp">The key timestamp.</param>
        /// <param name="value">The value.</param>
        public FlowRatePerformanceResult(FlowRatePerformanceCalculator calculator, DateTime keyTimestamp, double value)
            : base(calculator, keyTimestamp)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the value that was calculated.
        /// </summary>
        public double Value { get; }
    }
}