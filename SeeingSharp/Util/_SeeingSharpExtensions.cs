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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using SharpDX;

    #endregion

    /// <summary>
    /// Some common extension methods used in most coding of SeeingSharp.
    /// </summary>
    internal static partial class CommonExtensions
    {
        private static readonly CultureInfo CULTURE_EN = new CultureInfo("en-GB");

        private static Dictionary<System.Threading.Timer, object> s_timerDict;
        private static object s_timerDictLock;

        /// <summary>
        /// "Forgets" the given task, but still tries to dispatch exception somewhere the user / developer
        /// can see them.
        /// </summary>
        /// <param name="asyncAction">The action to be fired.</param>
        public static async void FireAndForget(this Task asyncAction)
        {
            await asyncAction;
        }

        /// <summary>
        /// Initializes the <see cref="CommonExtensions" /> class.
        /// </summary>
        static CommonExtensions()
        {
            s_timerDict = new Dictionary<System.Threading.Timer, object>();
            s_timerDictLock = new object();
        }

        public static void CreateDummyObjectUntilCapacityReached<T>(this List<T> list, int targetCapacity)
        {
            if (list.Capacity < targetCapacity) { list.Capacity = targetCapacity; }
            while(list.Count < targetCapacity)
            {
                list.Add(default(T));
            }
        }

        /// <summary>
        /// Gets most exact value of elapsed milliseconds from the given stopwatch.
        /// </summary>
        /// <param name="stopwatch">The stopwatch to get elapsed milliseconds from.</param>
        public static double GetTrueElapsedMilliseconds(this Stopwatch stopwatch)
        {
            return stopwatch.Elapsed.TotalMilliseconds;
        }

        /// <summary>
        /// Gets most exact value of elapsed milliseconds from the given stopwatch.
        /// </summary>
        /// <param name="stopwatch">The stopwatch to get elapsed milliseconds from.</param>
        public static long GetTrueElapsedMillisecondsRounded(this Stopwatch stopwatch)
        {
            return (long)Math.Round(stopwatch.Elapsed.TotalMilliseconds);
        }

        /// <summary>
        /// Sets all values of the given array to the given value.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="newValue">The new value.</param>
        public static void SetAllValuesTo<T>(this T[] array, T newValue)
        {
            for(int loop=0 ; loop<array.Length; loop++)
            {
                array[loop] = newValue;
            }
        }

        /// <summary>
        /// Determines whether the specified collection contains the given string.
        /// </summary>
        /// <param name="collection">The collection to be searched for the gien string.</param>
        /// <param name="compareString">The string used for comparison.</param>
        /// <param name="comparison">The comparison mode.</param>
        public static bool ContainsString(this IEnumerable<string> collection, string compareString, StringComparison comparison = StringComparison.CurrentCulture)
        {
            foreach(string actString in collection)
            {
                if (string.Equals(actString, compareString, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Converts the given collection to a comma separated string (e. g. object1, object2, object3, ...).
        /// The ToString method is used to get the strings for each individual object.
        /// </summary>
        /// <typeparam name="T">Type of the items.</typeparam>
        /// <param name="collection">The collection to format.</param>
        public static string ToCommaSeparatedString<T>(this IEnumerable<T> collection)
        {
            return collection.ToCommaSeparatedString(", ");
        }

        /// <summary>
        /// Converts the given collection to a comma separated string (e. g. object1, object2, object3, ...).
        /// The ToString method is used to get the strings for each individual object.
        /// </summary>
        /// <typeparam name="T">Type of the items.</typeparam>
        /// <param name="collection">The collection to format.</param>
        /// <param name="separator">A custom separator string.</param>
        public static string ToCommaSeparatedString<T>(this IEnumerable<T> collection, string separator)
        {
            StringBuilder readMessageBuilder = new StringBuilder();
            foreach (T actObject in collection)
            {
                if (readMessageBuilder.Length > 0) { readMessageBuilder.Append(separator); }
                readMessageBuilder.Append("" + actObject);
            }
            return readMessageBuilder.ToString();
        }

        /// <summary>
        /// Converts the given collection to a comma separated string (e. g. object1, object2, object3, ...).
        /// The given toStringFunc method is used to get the strings for each individual object.
        /// </summary>
        /// <typeparam name="T">Type of the items.</typeparam>
        /// <param name="collection">The collection to format.</param>
        /// <param name="toStringFunc">To string function.</param>
        /// <returns></returns>
        public static string ToCommaSeparatedString<T>(this IEnumerable<T> collection, Func<T, string> toStringFunc)
        {
            return collection.ToCommaSeparatedString(toStringFunc, ", ");
        }

        /// <summary>
        /// Converts the given collection to a comma separated string (e. g. object1, object2, object3, ...).
        /// The given toStringFunc method is used to get the strings for each individual object.
        /// </summary>
        /// <typeparam name="T">Type of the items.</typeparam>
        /// <param name="collection">The collection to format.</param>
        /// <param name="toStringFunc">To string function.</param>
        /// <param name="separator">A custom separator string.</param>
        /// <returns></returns>
        public static string ToCommaSeparatedString<T>(this IEnumerable<T> collection, Func<T, string> toStringFunc, string separator)
        {
            StringBuilder readMessageBuilder = new StringBuilder();
            foreach (T actObject in collection)
            {
                if (readMessageBuilder.Length > 0) { readMessageBuilder.Append(separator); }
                readMessageBuilder.Append("" + toStringFunc(actObject));
            }
            return readMessageBuilder.ToString();
        }

        /// <summary>
        /// Gets the backing array of the given list.
        /// </summary>
        /// <param name="lst">The list from which to get the backing array for faster loop access.</param>
        public static T[] GetBackingArray<T>(this List<T> lst)
        {
            return SeeingSharpUtil.GetBackingArray(lst);
        }

        /// <summary>
        /// Gets the backing array of the given queue.
        /// </summary>
        /// <param name="queue">The queue from which to get the backing array for faster loop access.</param>
        public static T[] GetBackingArray<T>(this Queue<T> queue)
        {
            return SeeingSharpUtil.GetBackingArray(queue);
        }

        public static T[] Subset<T>(this T[] givenArray, int startIndex, int count)
        {
            T[] result = new T[count];
            for (int loop = 0; loop < count; loop++)
            {
                result[loop] = givenArray[startIndex + loop];
            }
            return result;
        }

        /// <summary>
        /// Raises the given event.
        /// </summary>
        /// <param name="eventHandler">The event to be raised.</param>
        /// <param name="sender">The sender parameter.</param>
        /// <param name="eventArgs">The event args parameter.</param>
        public static void Raise(this EventHandler eventHandler, object sender, EventArgs eventArgs)
        {
            if (eventHandler != null) { eventHandler(sender, eventArgs); }
        }

        /// <summary>
        /// Raises the given event.
        /// </summary>
        /// <param name="eventHandler">The event to be raised.</param>
        /// <param name="sender">The sender parameter.</param>
        /// <param name="eventArgs">The event args parameter.</param>
        public static void Raise<T>(this EventHandler<T> eventHandler, object sender, T eventArgs)
        {
            if (eventHandler != null) { eventHandler(sender, eventArgs); }
        }

        /// <summary>
        /// Reads all bytes from the given stream.
        /// </summary>
        /// <param name="inStream">The stream to read all the data from.</param>
        public static byte[] ReadAllBytes(this Stream inStream)
        {
            if (inStream.Length > Int32.MaxValue) { throw new NotSupportedException("Given stream is to big!"); }

            byte[] result = new byte[inStream.Length];
            inStream.Read(result, 0, (int)inStream.Length);
            return result;
        }

        /// <summary>
        /// Reads a vector from the given xml reader.
        /// </summary>
        /// <param name="xmlReader">The xml reader.</param>
        public static Vector3 ReadContentAsVector3(this XmlReader xmlReader)
        {
            return ReadContentAsVector3(xmlReader, CULTURE_EN.NumberFormat);
        }

        /// <summary>
        /// Reads a vector from the given xml reader.
        /// </summary>
        /// <param name="xmlReader">The xml reader.</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/> for parsing <see cref="System.Single"/> values.</param>
        public static Vector3 ReadContentAsVector3(this XmlReader xmlReader, IFormatProvider formatProvider)
        {
            string[] components = xmlReader.ReadContentAsString().Split(',');
            if (components.Length != 3) { throw new SeeingSharpException("Invalid vector3 format in xml file!"); }

            Vector3 result = new Vector3();
            result.X = float.Parse(components[0], formatProvider);
            result.Y = float.Parse(components[1], formatProvider);
            result.Z = float.Parse(components[2], formatProvider);

            return result;
        }

        /// <summary>
        /// Reads a vector from the given xml reader.
        /// </summary>
        /// <param name="xmlReader">The xml reader.</param>
        public static Vector2 ReadContentAsVector2(this XmlReader xmlReader)
        {
            return ReadContentAsVector2(xmlReader, CULTURE_EN.NumberFormat);
        }

        /// <summary>
        /// Reads a vector from the given xml reader.
        /// </summary>
        /// <param name="xmlReader">The xml reader.</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/> for parsing <see cref="System.Single"/> values.</param>
        public static Vector2 ReadContentAsVector2(this XmlReader xmlReader, IFormatProvider formatProvider)
        {
            string[] components = xmlReader.ReadContentAsString().Split(',');
            if (components.Length != 2) { throw new SeeingSharpException("Invalid vector2 format in xml file!"); }

            Vector2 result = new Vector2();
            result.X = float.Parse(components[0], formatProvider);
            result.Y = float.Parse(components[1], formatProvider);

            return result;
        }

        /// <summary>
        /// Reads a vector from the given xml reader.
        /// </summary>
        /// <param name="xmlReader">The xml reader.</param>
        public static Vector3 ReadElementContentAsVector3(this XmlReader xmlReader)
        {
            return ReadElementContentAsVector3(xmlReader, CULTURE_EN.NumberFormat);
        }

        /// <summary>
        /// Reads a vector from the given xml reader.
        /// </summary>
        /// <param name="xmlReader">The xml reader.</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/> for parsing <see cref="System.Single"/> values.</param>
        public static Vector3 ReadElementContentAsVector3(this XmlReader xmlReader, IFormatProvider formatProvider)
        {
            string[] components = xmlReader.ReadElementContentAsString().Split(',');
            if (components.Length != 3) { throw new SeeingSharpException("Invalid vector3 format in xml file!"); }

            Vector3 result = new Vector3();
            result.X = float.Parse(components[0], formatProvider);
            result.Y = float.Parse(components[1], formatProvider);
            result.Z = float.Parse(components[2], formatProvider);

            return result;
        }

        /// <summary>
        /// Clears and disposes all items of the given collection.
        /// </summary>
        /// <typeparam name="T">Type of the items.</typeparam>
        /// <param name="listOfDisposables">A collection containing all items to be disposed and removed.</param>
        public static void ClearAndDisposeAll<T>(this ICollection<T> listOfDisposables)
            where T : class, IDisposable
        {
            foreach (var actDisposeable in listOfDisposables)
            {
                SeeingSharpUtil.DisposeObject(actDisposeable);
            }
            listOfDisposables.Clear();
        }

        /// <summary>
        /// Performs the given Action for each element within the enumeration.
        /// </summary>
        /// <typeparam name="T">Type of the elements.</typeparam>
        /// <param name="enumeration">Enumeration to loop through.</param>
        /// <param name="actionPerElement">Action to perform for each element.</param>
        public static void ForEachInEnumeration<T>(this IEnumerable<T> enumeration, Action<T> actionPerElement)
        {
            foreach (T actElement in enumeration)
            {
                actionPerElement(actElement);
            }
        }

        /// <summary>
        /// Converts all items from the given source enumeration to the given target enumeration.
        /// </summary>
        public static IEnumerable<TTarget> Convert<TSource, TTarget>(this IEnumerable<TSource> enumeration, Func<TSource, TTarget> converter)
        {
            foreach (TSource actSourceItem in enumeration)
            {
                yield return converter(actSourceItem);
            }
        }

        public static void PostAlsoIfNull(this SynchronizationContext syncContext, Action postAction)
        {
            if (syncContext == null)
            {
                ThreadPool.QueueUserWorkItem(obj => postAction());
            }
            else
            {
                syncContext.Post(obj => postAction(), null);
            }
        }

#if WINRT || UNIVERSAL
        /// <summary>
        /// "Forgets" the given task, but still tries to dispatch exception somewhere the user / developer
        /// can see them.
        /// </summary>
        /// <param name="asyncAction">The action to be fired.</param>
        public static async void FireAndForget(this IAsyncAction asyncAction)
        {
            await asyncAction;
        }

        /// <summary>
        /// "Forgets" the given task, but still tries to dispatch exception somewhere the user / developer
        /// can see them.
        /// </summary>
        /// <param name="asyncAction">The action to be fired.</param>
        public static async void FireAndForget(this Task asyncAction)
        {
            await asyncAction;
        }
#endif

#if DESKTOP
        /// <summary>
        /// "Forgets" the given task, but still tries to dispatch exception somewhere the user / developer
        /// can see them.
        /// </summary>
        /// <param name="asyncAction">The action to be fired.</param>
        public static async void FireAndForget(this Task asyncAction)
        {
            await asyncAction;
        }

#endif

        /// <summary>
        /// Posts the given action to the given synchronization context also if it is null.
        /// If it is null, then a new task will be started.
        /// </summary>
        /// <param name="syncContext">The context to send the action to.</param>
        /// <param name="actionToSend">The action to send.</param>
        /// <param name="actionIfNull">What should we do if weg get no SyncContext?</param>
        public static void PostAlsoIfNull(this SynchronizationContext syncContext, Action actionToSend, ActionIfSyncContextIsNull actionIfNull)
        {
            if (syncContext != null) { syncContext.Post((arg) => actionToSend(), null); }
            else
            {
                switch (actionIfNull)
                {
                    case ActionIfSyncContextIsNull.InvokeSynchronous:
                        actionToSend();
                        break;

                    case ActionIfSyncContextIsNull.InvokeUsingNewTask:
                        Task.Factory.StartNew(actionToSend);
                        break;

                    case ActionIfSyncContextIsNull.DontInvoke:
                        break;

                    default:
                        throw new ArgumentException("actionIfNull", "Action " + actionIfNull + " unknown!");
                }
            }
        }

        /// <summary>
        /// Post the given action in an async manner to the given SynchronizationContext.
        /// </summary>
        /// <param name="syncContext">The target SynchronizationContext.</param>
        /// <param name="postAction">The action to be posted.</param>
        /// <param name="actionIfNull">What should we do if we get no SyncContext?</param>
        public static Task PostAlsoIfNullAsync(this SynchronizationContext syncContext, Action postAction, ActionIfSyncContextIsNull actionIfNull)
        {
            TaskCompletionSource<object> completionSource = new TaskCompletionSource<object>();
            syncContext.PostAlsoIfNull(() =>
                {
                    try
                    {
                        postAction();
                        completionSource.SetResult(null);
                    }
                    catch (Exception ex)
                    {
                        completionSource.SetException(ex);
                    }
                },
                actionIfNull);
            return completionSource.Task;
        }

        /// <summary>
        /// Post the given action in an async manner to the given SynchronizationContext.
        /// </summary>
        /// <param name="syncContext">The target SynchronizationContext.</param>
        /// <param name="postAction">The action to be posted.</param>
        public static Task PostAsync(this SynchronizationContext syncContext, Action postAction)
        {
            TaskCompletionSource<object> completionSource = new TaskCompletionSource<object>();
            syncContext.Post((arg) =>
            {
                try
                {
                    postAction();
                    completionSource.TrySetResult(null);
                }
                catch (Exception ex)
                {
                    completionSource.TrySetException(ex);
                }
            }, null);
            return completionSource.Task;
        }

        /// <summary>
        /// Executes a delayd post to the given synchronization context.
        /// </summary>
        /// <param name="syncContext">The synchronization context to post to.</param>
        /// <param name="callBack">The delegate to be called.</param>
        /// <param name="state">The parameter of the delegate.</param>
        /// <param name="delayTime">The total time to wait.</param>
        public static void PostDelayed(this SynchronizationContext syncContext, SendOrPostCallback callBack, object state, TimeSpan delayTime)
        {
            if (syncContext == null) { throw new ArgumentNullException("syncContext"); }
            if (callBack == null) { throw new ArgumentNullException("callBack"); }
            if (delayTime <= TimeSpan.Zero) { throw new ArgumentException("Delay time musst be greater than zero!", "delayTime"); }

            //Start and register timer in local timer store (ensures that no dispose gets called..)
            lock (s_timerDictLock)
            {
                System.Threading.Timer newTimer = null;
                newTimer = new System.Threading.Timer(
                    (arg) =>
                    {
                        lock (s_timerDictLock)
                        {
                            s_timerDict.Remove(newTimer);
                        }
                        syncContext.Post(callBack, state);
                    },
                    null,
                    (int)delayTime.TotalMilliseconds,
                    Timeout.Infinite);
                s_timerDict.Add(newTimer, null);
            }
        }
    }
}
