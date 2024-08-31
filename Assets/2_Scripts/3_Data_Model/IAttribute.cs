using System;

namespace GimJem.DataModel
{
    public interface IAttribute<T>
    {
        void InitializeAttribute(T value);
        void UpdateAttribute(T value);
        void ResetAttribute();
        void AddOnAttributeUpdatedListener(Action<T> OnAttributeUpdated);
        void RemoveOnAttributeUpdatedListener(Action<T> OnAttributeUpdated);
        void RemoveAllOnAttributeUpdatedListener();
    }

}
