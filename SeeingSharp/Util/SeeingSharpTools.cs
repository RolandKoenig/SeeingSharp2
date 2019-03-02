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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SeeingSharp.Util
{
    public static class SeeingSharpTools
    {
        public static T TryExecute<T>(Func<T> funcToExec)
        {
            try
            {
                return funcToExec();
            }
            catch
            {
                return default;
            }
        }

        public static void TryExecute(Action actionToExecute)
        {
            try
            {
                actionToExecute();
            }
            catch { }
        }

        /// <summary>
        /// Calls the given function asynchronous.
        /// </summary>
        /// <param name="funcToExecute">The function to execute.</param>
        public static Task<T> CallAsync<T>(Func<T> funcToExecute)
        {
            return Task.Factory.StartNew(funcToExecute);
        }

        /// <summary>
        /// Copies memory from given source to given target pointer.
        /// </summary>
        /// <param name="sourcePointer">The source pointer.</param>
        /// <param name="targetPointer">The target pointer.</param>
        /// <param name="byteCount">The total count of bytes to be copied.</param>
        public static unsafe void CopyMemory(IntPtr sourcePointer, IntPtr targetPointer, ulong byteCount)
        {
            CopyMemory(sourcePointer.ToPointer(), targetPointer.ToPointer(), byteCount);
        }

        /// <summary>
        /// Copies memory from given source to given target pointer.
        /// </summary>
        /// <param name="sourcePointer">The source pointer.</param>
        /// <param name="targetPointer">The target pointer.</param>
        /// <param name="byteCount">The total count of bytes to be copied.</param>
        public static unsafe void CopyMemory(void* sourcePointer, void* targetPointer, ulong byteCount)
        {
            var longCount = byteCount / 8;
            var byteScrap = byteCount % 8;

            // Copy using long pointers
            var sourcePointerLong = (ulong*)sourcePointer;
            var targetPointerLong = (ulong*)targetPointer;
            for (ulong actIndexLong = 0; actIndexLong < longCount; actIndexLong++)
            {
                targetPointerLong[actIndexLong] = sourcePointerLong[actIndexLong];
            }

            // Copy remaining bytes
            if (byteScrap > 0)
            {
                var sourcePointerByte = (byte*)sourcePointer;
                var targetPointerByte = (byte*)targetPointer;
                for (var actIndexByte = byteCount - byteScrap; actIndexByte < byteCount; actIndexByte++)
                {
                    targetPointerByte[actIndexByte] = sourcePointerByte[actIndexByte];
                }
            }
        }

        /// <summary>
        /// Formats the given timespan to a compact string.
        /// </summary>
        /// <param name="timespan">The Tiemspan value to be formated.</param>
        public static string FormatTimespanCompact(TimeSpan timespan)
        {
            return
                Math.Floor(timespan.TotalHours).ToString("F0") + ":" +
                timespan.Minutes.ToString().PadLeft(2, '0') + ":" +
                timespan.Seconds.ToString().PadLeft(2, '0') + ":" +
                timespan.TotalMilliseconds.ToString().PadLeft(3, '0');
        }

        /// <summary>
        /// Performs a real delay (sleeps a shorter time and does a dummy loop after then).
        /// </summary>
        /// <param name="delayMilliseconds">Total delay time.</param>
        public static async Task MaximumDelayAsync(double delayMilliseconds)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // Ensure initial short delay
            if (delayMilliseconds > 5.0)
            {
                await Task.Delay(2);
            }

            // Do short delay pahses until we reach a point where we are "near" the target value
            while (stopwatch.GetTrueElapsedMilliseconds() < delayMilliseconds - 10.0)
            {
                await Task.Delay(2);
            }
        }

        /// <summary>
        /// Performs a real delay (uses a manual reset event for waiting..).
        /// </summary>
        /// <param name="delayMilliseconds">Total time for delay.</param>
        public static void MaximumDelay(double delayMilliseconds)
        {
            if (delayMilliseconds <= 0) { return; }

            using (var waitHandle = new ManualResetEvent(false))
            {
                waitHandle.WaitOne((int)delayMilliseconds);
            }
        }

        public static bool HasAnyElement<T>(IEnumerable<T> collection)
        {
            var readonlyCollection = collection as IReadOnlyCollection<T>;

            if (readonlyCollection != null)
            {
                return readonlyCollection.Count > 0;
            }
            var simpleCollection = collection as ICollection;

            if (simpleCollection != null)
            {
                return simpleCollection.Count > 0;
            }
            // Try to loop forward to the first element
            foreach (var actElement in collection)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the total count of items within the given collection.
        /// </summary>
        public static int GetCollectionCount<T>(IEnumerable<T> collection)
        {
            var readonlyCollection = collection as IReadOnlyCollection<T>;

            if (readonlyCollection != null)
            {
                return readonlyCollection.Count;
            }
            var simpleCollection = collection as ICollection;

            if (simpleCollection != null)
            {
                return simpleCollection.Count;
            }
            return collection.Count();
        }

        /// <summary>
        /// Inserts the given object using a binary search algorithm.
        /// </summary>
        /// <param name="targetList">The list to be modified.</param>
        /// <param name="newItem">The new item to be inserted.</param>
        public static int BinaryInsert<T>(List<T> targetList, T newItem)
        {
            var targetIndex = targetList.BinarySearch(newItem);
            if (targetIndex < 0)
            {
                targetList.Insert(~targetIndex, newItem);
                return ~targetIndex;
            }
            targetList.Insert(targetIndex, newItem);
            return targetIndex;
        }

        /// <summary>
        /// Inserts the given object using a binary search algorithm.
        /// </summary>
        /// <param name="targetList">The list to be modified.</param>
        /// <param name="newItem">The new item to be inserted.</param>
        /// <param name="comparer">The comparer which is used for the binary search method.</param>
        public static int BinaryInsert<T>(List<T> targetList, T newItem, IComparer<T> comparer)
        {
            var targetIndex = targetList.BinarySearch(newItem, comparer);
            if (targetIndex < 0)
            {
                targetList.Insert(~targetIndex, newItem);
                return ~targetIndex;
            }
            targetList.Insert(targetIndex, newItem);
            return targetIndex;
        }

        /// <summary>
        /// Removes the given object using binary search algorithm.
        /// </summary>
        /// <param name="targetList">The target list to be modified.</param>
        /// <param name="toRemove">The object to be removed.</param>
        public static void BinaryRemove<T>(List<T> targetList, T toRemove)
        {
            var targetIndex = targetList.BinarySearch(toRemove);
            if (targetIndex >= 0)
            {
                targetList.RemoveAt(targetIndex);
            }
        }

        /// <summary>
        /// Disposes the given object.
        /// </summary>
        public static void SafeDispose<T>(ref T toDispose)
            where T : class, IDisposable
        {
            toDispose = DisposeObject(toDispose);
        }

        /// <summary>
        /// Disposes the given object.
        /// </summary>
        public static void SafeDisposeLazy<T>(ref Lazy<T> toDispose)
            where T : class, IDisposable
        {
            toDispose = DisposeObjectLazy(toDispose);
        }

        /// <summary>
        /// Disposes the given object and returns null.
        /// </summary>
        public static T DisposeObject<T>(T objectToDispose)
            where T : class, IDisposable
        {
            if (objectToDispose == null) { return null; }

            try { objectToDispose.Dispose(); }
            catch (Exception)
            {
            }
            return null;
        }

        /// <summary>
        /// Disposes the given lazy object (if created already).
        /// </summary>
        /// <param name="objectToDispose">The object to be disposed.</param>
        public static Lazy<T> DisposeObjectLazy<T>(Lazy<T> objectToDispose)
            where T : class, IDisposable
        {
            if (objectToDispose == null) { return null; }
            if (!objectToDispose.IsValueCreated) { return null; }

            DisposeObject(objectToDispose.Value);
            return null;
        }

        /// <summary>
        /// Disposes all objects within the given enumeration.
        /// </summary>
        /// <param name="enumeration">Enumeration containing all disposable objects.</param>
        public static void DisposeObjects<T>(IEnumerable<T> enumeration)
            where T : class, IDisposable
        {
            if (enumeration == null)
            {
                throw new ArgumentNullException("enumeration");
            }

            foreach (var actItem in enumeration)
            {
                DisposeObject(actItem);
            }
        }
    }
}