using System;
using System.Collections.Generic;
using UnityEngine;

namespace AppStructure.Utils
{
    public class RootByGenericTypes<TValue>
    {
        private readonly Dictionary<Type, TValue> _valuesByTypes = new();

        protected IReadOnlyDictionary<Type, TValue> ValuesByTypes => _valuesByTypes;
        
        public void RegisterModel<T>(T model) where T : TValue
        {
            try
            {
                _valuesByTypes.Add(typeof(T), model);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public T Model<T>() where T : class, TValue
        {
            if (!_valuesByTypes.TryGetValue(typeof(T), out var modelBase))
                return null;

            return modelBase as T;
        }
    }
}