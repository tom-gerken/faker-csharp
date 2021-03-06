﻿using System;
using System.Collections.Generic;
using System.Linq;
using Faker.Helpers;
using Faker.Selectors;

namespace Faker
{
    public enum SelectorPosition
    {
        First,
        Last
    }

    /// <summary>
    /// The repository which determines the order in which selectors are picked for any given type
    /// </summary>
    public sealed class TypeTable
    {
        //Our internal map for all types and their selectors used within Faker
        private readonly Dictionary<Type, LinkedList<ITypeSelector>> _typeMap;

        /// <summary>
        /// Default constructor
        /// </summary>
        public TypeTable(bool useDefaults = true):this(useDefaults, new Dictionary<Type, LinkedList<ITypeSelector>>()){}

        /// <summary>
        /// Private constructor which accepts an inbound typemap as an argument
        /// </summary>
        /// <param name="useDefaults">Determines if we should load all of our default selectors or not</param>
        /// <param name="typeMap">A typemap instance</param>
        private TypeTable(bool useDefaults, Dictionary<Type, LinkedList<ITypeSelector>> typeMap)
        {
            _typeMap = typeMap;
            if(useDefaults) //Load defaults if the user requested it
                LoadDefaults();
        }

        /// <summary>
        /// Private internal method for loading all of the default TypeSelectors into the TypeTable
        /// </summary>
        private void LoadDefaults()
        {
            /* NUMERIC DEFAULT SELECTORS */
            AddSelector(new IntSelector());
            AddSelector(new LongSelector());
            AddSelector(new TimeStampSelector()); //TimeStamp selector should get used before the long selector
            AddSelector(new DoubleSelector());
            AddSelector(new FloatSelector());
            AddSelector(new DecimalSelector());

            /* DATETIME SELECTORS */
            AddSelector(new DateTimeSelector());
            AddSelector(new DateTimeOffsetSelector());

            /* STRING SELECTORS */
            AddSelector(new StringSelector()); //String selector is the very last one we want to try and use
            AddSelector(new FirstNameSelector());
            AddSelector(new LastNameSelector());
            AddSelector(new FullNameSelector());
            AddSelector(new EmailSelector());

            /* GUID SELECTORS */
            AddSelector(new GuidSelector());
        }

        /// <summary>
        /// Internal method for securely creating type rows where needed
        /// </summary>
        /// <param name="incomingType">The type to which we're mapping this object</param>
        private void CreateTypeIfNotExists(Type incomingType)
        {
            //If we already have a collection in our dictionary that corresponds to this type, bail out
            if (_typeMap.ContainsKey(incomingType))
                return;

            //Otherwise, create the row in our dictionary
            _typeMap.Add(incomingType, new LinkedList<ITypeSelector>());
        }

        /// <summary>
        /// Add a strongly typed selector to the 
        /// </summary>
        /// <typeparam name="T">The type for which this selector is used</typeparam>
        /// <param name="selector">A TypeSelectorBase implementation for Type T</param>
        /// <param name="position">Optional parameter - indicates whether you want this type selector to be used first or last in the sequence of available selectors for this type</param>
        public void AddSelector<T>(TypeSelectorBase<T> selector, SelectorPosition position = SelectorPosition.First)
        {
            var activeType = typeof (T);
            CreateTypeIfNotExists(activeType);

            if (position == SelectorPosition.First) //If the user wants to add this selector to the front of the list (default), do that
                _typeMap[activeType].AddFirst(selector);
            else //Otherwise, add this type to the back of the list
                _typeMap[activeType].AddLast(selector);
        }

        /// <summary>
        /// Remove all of the selectors for a given type
        /// </summary>
        /// <typeparam name="T">The type for which we want to clear all of the selectors</typeparam>
        public void ClearSelectors<T>()
        {
            var activeType = typeof (T);
            if (_typeMap.ContainsKey(activeType)) //Reassign the value to a new list
                _typeMap[activeType] = new LinkedList<ITypeSelector>();
        }

        /// <summary>
        /// Count all of the selectors for a given type
        /// </summary>
        /// <typeparam name="T">The type for which we need value selectors</typeparam>
        /// <returns>The number of selectors we have available for this type</returns>
        public int CountSelectors<T>()
        {
            var activeType = typeof(T);
            return CountSelectors(activeType);
        }

        /// <summary>
        /// Count all of the selectors for a given type
        /// </summary>
        /// <param name="t">A type object for which we want to know the number of available selectors</param>
        /// <returns>The number of selectors we have available for this type</returns>
        public int CountSelectors(Type t)
        {
            CreateTypeIfNotExists(t);
            return _typeMap[t].Count;
        }

        /// <summary>
        /// Get all of the selectors for a given type
        /// </summary>
        /// <typeparam name="T">The type for which we need value selectors</typeparam>
        /// <returns>An enumerable list of selectors</returns>
        public IEnumerable<TypeSelectorBase<T>> GetSelectors<T>()
        {
            var activeType = typeof(T);
            var selectors = GetSelectors(activeType);
            return selectors.Cast<TypeSelectorBase<T>>();
        }

        /// <summary>
        /// Get all of the selectors for a given type
        /// </summary>
        /// <returns>An enumerable list of selectors</returns>
        public IEnumerable<ITypeSelector> GetSelectors(Type t)
        {
            CreateTypeIfNotExists(t);
            return _typeMap[t];
        }

        /// <summary>
        /// Gets the base selector for a given datatype
        /// </summary>
        /// <param name="t">The type that we need to inject</param>
        /// <returns>A matching selector, null otherwise</returns>
        public ITypeSelector GetBaseSelector(Type t)
        {
            CreateTypeIfNotExists(t);
            var baseType = GenericHelper.GetGenericType(typeof (PrimitiveSelectorBase<>), t);
            return _typeMap[t].FirstOrDefault(x => baseType.IsAssignableFrom(x.GetType()));
        }
    }
}
