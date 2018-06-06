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
using SeeingSharp.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SeeingSharp.Checking
{
    public static partial class Ensure
    {
        [Conditional("DEBUG")]
        public static void EnsureDoesNotContain<T>(
            this IEnumerable<T> collection, T element, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            // Check result
            if (collection.Contains(element))
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Collection {0} within method {1} must not contain element {2}!",
                    checkedVariableName, callerMethod, element));
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureMoreThanZeroElements<T>(
            this IEnumerable<T> collection, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            // Get the collection count
            bool hasAnyElement = CommonTools.HasAnyElement(collection);

            // Check result
            if(!hasAnyElement)
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Collection {0} within method {1} musst have more than zero elements!",
                    checkedVariableName, callerMethod));
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureCountInRange<T>(
            this IEnumerable<T> collection, int countMin, int countMax, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            // Get the collection count
            int collectionCount = -1;
            collectionCount = CommonTools.GetCollectionCount<T>(collection);

            // Check result
            if ((collectionCount < countMin) ||
                (collectionCount > countMax))
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Collection {0} within method {1} does not have the expected count of elements (expected min {2} to max {3}, current count is {4})!",
                    checkedVariableName, callerMethod, countMin, countMax, collectionCount));
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureCountEquals<T>(
            this IEnumerable<T> collection, int count, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            // Get the collection count
            int collectionCount = -1;
            collectionCount = CommonTools.GetCollectionCount<T>(collection);

            // Check result
            if (collectionCount != count)
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Collection {0} within method {1} does not have the expected count of elements (expected {2})!",
                    checkedVariableName, callerMethod, count, collectionCount));
            }
        }
    }
}
