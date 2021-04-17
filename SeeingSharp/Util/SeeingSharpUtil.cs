/*
    SeeingSharp and all applications distributed together with it. 
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
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Core;

namespace SeeingSharp.Util
{
    public static class SeeingSharpUtil
    {
        /// <summary>
        /// Gets the backing array of the given queue.
        /// </summary>
        /// <param name="queue">The queue from which to get the backing array for faster loop access.</param>
        public static T[] GetBackingArray<T>(Queue<T> queue)
        {
            var fInfo = queue.GetType().GetTypeInfo().GetField("_array", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fInfo != null)
            {
                return fInfo.GetValue(queue) as T[];
            }
            throw new SeeingSharpException("Unable to get backing array from Queue<T>!");
        }

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
            catch
            {
                // ignored
            }
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
        public static unsafe void CopyMemory(IntPtr sourcePointer, IntPtr targetPointer, uint byteCount)
        {
            CopyMemory(sourcePointer.ToPointer(), targetPointer.ToPointer(), byteCount);
        }

        /// <summary>
        /// Copies memory from given source to given target pointer.
        /// </summary>
        /// <param name="sourcePointer">The source pointer.</param>
        /// <param name="targetPointer">The target pointer.</param>
        /// <param name="byteCount">The total count of bytes to be copied.</param>
        public static unsafe void CopyMemory(void* sourcePointer, void* targetPointer, uint byteCount)
        {
            Unsafe.CopyBlock(targetPointer, sourcePointer, byteCount);
        }

        /// <summary>
        /// Formats the given timespan to a compact string.
        /// </summary>
        /// <param name="timespan">The TimeSpan value to be formatted.</param>
        public static string FormatTimespanCompact(TimeSpan timespan)
        {
            return
                Math.Floor(timespan.TotalHours).ToString("F0") + ":" +
                timespan.Minutes.ToString().PadLeft(2, '0') + ":" +
                timespan.Seconds.ToString().PadLeft(2, '0') + ":" +
                timespan.TotalMilliseconds.ToString(CultureInfo.InvariantCulture).PadLeft(3, '0');
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

            // Do short delay phases until we reach a point where we are "near" the target value
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
            switch (collection)
            {
                case IReadOnlyCollection<T> readonlyCollection:
                    return readonlyCollection.Count > 0;
                case ICollection simpleCollection:
                    return simpleCollection.Count > 0;

                default:
                    // ReSharper disable once UnusedVariable
                    foreach (var actElement in collection)
                    {
                        return true;
                    }
                    return false;
            }
        }

        /// <summary>
        /// Gets the total count of items within the given collection.
        /// </summary>
        public static int GetCollectionCount<T>(IEnumerable<T> collection)
        {
            switch (collection)
            {
                case IReadOnlyCollection<T> readonlyCollection:
                    return readonlyCollection.Count;
                case ICollection simpleCollection:
                    return simpleCollection.Count;

                default:
                    return collection.Count();
            }
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
            catch (Exception ex)
            {
                // Publish exception info
                GraphicsCore.PublishInternalExceptionInfo(ex, InternalExceptionLocation.DisposeObject);
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
            enumeration.EnsureNotNull(nameof(enumeration));

            foreach (var actItem in enumeration)
            {
                DisposeObject(actItem);
            }
        }
    }
}