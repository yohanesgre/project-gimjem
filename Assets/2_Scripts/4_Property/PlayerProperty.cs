using System;
using System.Collections;
using System.Collections.Generic;
using GimJem.DataModel;
using Unity.VisualScripting;
using UnityEngine;

namespace GimJem.Property
{
    public class PlayerProperties : MonoBehaviour
    {
        public HealthAttribute health;
        public SpeedAttribute speed;

        void Awake()
        {
            health.InitializeAttribute(100f);
            speed.InitializeAttribute(5f);

            health.AddOnAttributeUpdatedListener(OnHealthUpdated);
            speed.AddOnAttributeUpdatedListener(OnSpeedUpdated);
        }

        void OnHealthUpdated(float newHealth)
        {
            Debug.Log($"Health updated to: {newHealth}");
        }

        void OnSpeedUpdated(float newSpeed)
        {
            Debug.Log($"Speed updated to: {newSpeed}");
        }

    }

}
