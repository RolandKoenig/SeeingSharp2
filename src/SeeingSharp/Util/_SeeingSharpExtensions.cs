﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SeeingSharp.Util
{
    /// <summary>
    /// Some common extension methods used in most coding of SeeingSharp.
    /// </summary>
    internal static class CommonExtensions
    {
        private static readonly CultureInfo s_cultureEn = new("en-GB");

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
            for (var loop = 0; loop < array.Length; loop++)
            {
                array[loop] = newValue;
            }
        }

        /// <summary>
        /// Determines whether the specified collection contains the given string.
        /// </summary>
        /// <param name="collection">The collection to be searched for the given string.</param>
        /// <param name="compareString">The string used for comparison.</param>
        /// <param name="comparison">The comparison mode.</param>
        public static bool ContainsString(this IEnumerable<string> collection, string compareString, StringComparison comparison = StringComparison.CurrentCulture)
        {
            foreach (var actString in collection)
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
            var readMessageBuilder = new StringBuilder();

            foreach (var actObject in collection)
            {
                if (readMessageBuilder.Length > 0)
                {
                    readMessageBuilder.Append(separator);
                }

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
            var readMessageBuilder = new StringBuilder();

            foreach (var actObject in collection)
            {
                if (readMessageBuilder.Length > 0)
                {
                    readMessageBuilder.Append(separator);
                }

                readMessageBuilder.Append("" + toStringFunc(actObject));
            }

            return readMessageBuilder.ToString();
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
            var result = new T[count];
            for (var loop = 0; loop < count; loop++)
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
        public static void Raise(this EventHandler? eventHandler, object sender, EventArgs eventArgs)
        {
            eventHandler?.Invoke(sender, eventArgs);
        }

        /// <summary>
        /// Raises the given event.
        /// </summary>
        /// <param name="eventHandler">The event to be raised.</param>
        /// <param name="sender">The sender parameter.</param>
        /// <param name="eventArgs">The event args parameter.</param>
        public static void Raise<T>(this EventHandler<T>? eventHandler, object sender, T eventArgs)
        {
            eventHandler?.Invoke(sender, eventArgs);
        }

        /// <summary>
        /// Reads all bytes from the given stream.
        /// </summary>
        /// <param name="inStream">The stream to read all the data from.</param>
        public static byte[] ReadAllBytes(this Stream inStream)
        {
            if (inStream.Length > int.MaxValue) { throw new NotSupportedException("Given stream is to big!"); }

            var result = new byte[inStream.Length];
            inStream.Read(result, 0, (int)inStream.Length);
            return result;
        }

        /// <summary>
        /// Reads a vector from the given xml reader.
        /// </summary>
        /// <param name="xmlReader">The xml reader.</param>
        public static Vector3 ReadContentAsVector3(this XmlReader xmlReader)
        {
            return ReadContentAsVector3(xmlReader, s_cultureEn.NumberFormat);
        }

        /// <summary>
        /// Reads a vector from the given xml reader.
        /// </summary>
        /// <param name="xmlReader">The xml reader.</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/> for parsing <see cref="System.Single"/> values.</param>
        public static Vector3 ReadContentAsVector3(this XmlReader xmlReader, IFormatProvider formatProvider)
        {
            var components = xmlReader.ReadContentAsString().Split(',');

            if (components.Length != 3)
            {
                throw new SeeingSharpException("Invalid vector3 format in xml file!");
            }

            var result = new Vector3
            {
                X = float.Parse(components[0], formatProvider),
                Y = float.Parse(components[1], formatProvider),
                Z = float.Parse(components[2], formatProvider)
            };

            return result;
        }

        /// <summary>
        /// Reads a vector from the given xml reader.
        /// </summary>
        /// <param name="xmlReader">The xml reader.</param>
        public static Vector2 ReadContentAsVector2(this XmlReader xmlReader)
        {
            return ReadContentAsVector2(xmlReader, s_cultureEn.NumberFormat);
        }

        /// <summary>
        /// Reads a vector from the given xml reader.
        /// </summary>
        /// <param name="xmlReader">The xml reader.</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/> for parsing <see cref="System.Single"/> values.</param>
        public static Vector2 ReadContentAsVector2(this XmlReader xmlReader, IFormatProvider formatProvider)
        {
            var components = xmlReader.ReadContentAsString().Split(',');

            if (components.Length != 2)
            {
                throw new SeeingSharpException("Invalid vector2 format in xml file!");
            }

            var result = new Vector2
            {
                X = float.Parse(components[0], formatProvider),
                Y = float.Parse(components[1], formatProvider)
            };

            return result;
        }

        /// <summary>
        /// Reads a vector from the given xml reader.
        /// </summary>
        /// <param name="xmlReader">The xml reader.</param>
        public static Vector3 ReadElementContentAsVector3(this XmlReader xmlReader)
        {
            return ReadElementContentAsVector3(xmlReader, s_cultureEn.NumberFormat);
        }

        /// <summary>
        /// Reads a vector from the given xml reader.
        /// </summary>
        /// <param name="xmlReader">The xml reader.</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/> for parsing <see cref="System.Single"/> values.</param>
        public static Vector3 ReadElementContentAsVector3(this XmlReader xmlReader, IFormatProvider formatProvider)
        {
            var components = xmlReader.ReadElementContentAsString().Split(',');

            if (components.Length != 3)
            {
                throw new SeeingSharpException("Invalid vector3 format in xml file!");
            }

            var result = new Vector3
            {
                X = float.Parse(components[0], formatProvider),
                Y = float.Parse(components[1], formatProvider),
                Z = float.Parse(components[2], formatProvider)
            };

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
            foreach (var actDisposable in listOfDisposables)
            {
                SeeingSharpUtil.DisposeObject(actDisposable);
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
            foreach (var actElement in enumeration)
            {
                actionPerElement(actElement);
            }
        }

        /// <summary>
        /// Converts all items from the given source enumeration to the given target enumeration.
        /// </summary>
        public static IEnumerable<TTarget> Convert<TSource, TTarget>(this IEnumerable<TSource> enumeration, Func<TSource, TTarget> converter)
        {
            foreach (var actSourceItem in enumeration)
            {
                yield return converter(actSourceItem);
            }
        }
    }
}
