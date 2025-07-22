using UnityEngine;
using System.Collections.Generic;

namespace WindTurbine
{
    /// <summary>
    /// Represents Wind Turbine with spinning blade and UI
    /// </summary>
    public class Turbine : MonoBehaviour, IWindTurbine
    {
        [SerializeField]private Transform _blades;

        [SerializeField] private TurbineUI _turbineUI;

        private Dictionary<float, float> _timeToRpm = new Dictionary<float, float>();

        private int _id;

        public void Initialize(TurbineData data)
        {
            Debug.Log("Initialize Turbine : " + data.TurbineID);
            _id = data.TurbineID;
            _timeToRpm = data.SpeedData;

            _turbineUI.Initialize(this);
        }
        private void FixedUpdate()
        {

            var rpm = GetCurrentSpeed();
            float degreesPerSecond = (rpm / 60f) * 360f;
            _blades.Rotate(Vector3.forward, degreesPerSecond * Time.fixedDeltaTime);
        }

        public float GetCurrentSpeed()
        {
            float currentTime = Time.time;
            float currentMinutes = Mathf.Floor(currentTime / 60f); // Convert game time to minutes
            _timeToRpm.TryGetValue(currentMinutes, out float rpm);
            return rpm;
        }

        public int GetTurbineID()
        {
            return _id;
        }
    }
}
