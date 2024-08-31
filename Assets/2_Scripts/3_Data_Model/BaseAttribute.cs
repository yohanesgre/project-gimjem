using System;
using System.Collections.Generic;
using UnityEngine;

namespace GimJem.DataModel
{
    [Serializable]
    public abstract class BaseAttribute<T> : IAttribute<T> where T : struct
    {
        [SerializeField] protected T baseValue;
        [SerializeField] protected T value;

        public T BaseValue { get { return baseValue; } }

        public T Value { get { return value; } }

        private List<Action<T>> onAttributeUpdatedListeners = new List<Action<T>>();

        public virtual void InitializeAttribute(T value)
        {
            baseValue = value;
            this.value = value;
            NotifyListeners();
        }

        public virtual void UpdateAttribute(T value)
        {
            this.value = value;
            NotifyListeners();
        }

        public virtual void ResetAttribute()
        {
            value = baseValue;
            NotifyListeners();
        }

        public void AddOnAttributeUpdatedListener(Action<T> OnAttributeUpdated)
        {
            onAttributeUpdatedListeners.Add(OnAttributeUpdated);
        }

        public void RemoveOnAttributeUpdatedListener(Action<T> OnAttributeUpdated)
        {
            onAttributeUpdatedListeners.Remove(OnAttributeUpdated);
        }

        public void RemoveAllOnAttributeUpdatedListener()
        {
            onAttributeUpdatedListeners.Clear();
        }

        protected void NotifyListeners()
        {
            foreach (var listener in onAttributeUpdatedListeners)
            {
                listener.Invoke(value);
            }
        }
    }

}
