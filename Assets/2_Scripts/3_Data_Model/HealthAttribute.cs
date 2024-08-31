
using System;
using UnityEditor;
using UnityEngine;

namespace GimJem.DataModel
{
    [Serializable]
    public class HealthAttribute : BaseAttribute<float>
    {
        public bool IsDead { get => value <= 0; }
    }

}
