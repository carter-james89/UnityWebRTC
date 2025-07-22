using UnityEngine;
using System.Collections.Generic;
using System;

namespace WindTurbine
{
    public class TurbineData
    {
        public int TurbineID { get; private set; }
        public DateTime? FirstTimeStamp;
        public Dictionary<float, float> SpeedData;

        public TurbineData(int ID)
        {
            TurbineID = ID;
            SpeedData = new Dictionary<float, float>();
        }
    }

    /// <summary>
    /// Parses datasets and initializes <see cref="IWindTurbine"/>
    /// </summary>
    public class WindTurbineController : MonoBehaviour
    {
        [SerializeField] private List<TextAsset> _dataSets;
        private IWindTurbine[] _turbines;
        private Dictionary<int, TurbineData> _turbineSpeedData = new Dictionary<int, TurbineData>();
     
        void Start()
        {
            _turbines = GetComponentsInChildren<IWindTurbine>();
            for (int i = 0; i < _turbines.Length; i++)
            {
                int turbineID = i + 1;
                Debug.Log("Create Turbine Speed Dataset : " + turbineID);
                _turbineSpeedData.Add(turbineID, new TurbineData(turbineID));
            }
            foreach (var dataSet in _dataSets)
            {
                LoadCSVFromTextAsset(dataSet);
            }

            for (int i = 0; i < _turbines.Length; i++)
            {
                int turbineID = i + 1;
                _turbines[i].Initialize(_turbineSpeedData[turbineID]);
            }
        }
        void LoadCSVFromTextAsset(TextAsset file)
        {
            string[] lines = file.text.Split('\n');

            for (int i = 1; i < lines.Length; i++) // Skip header
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                string[] parts = lines[i].Split(',');

                if (parts.Length < 3)
                {
                    Debug.LogWarning($"Invalid line at {i}: {lines[i]}");
                    continue;
                }

                int.TryParse(parts[0], out int turbineId);

                if (_turbineSpeedData.ContainsKey(turbineId))
                {
                    DateTime.TryParse(parts[1], out DateTime timestamp);

                    if (_turbineSpeedData[turbineId].FirstTimeStamp == null)
                    {
                        _turbineSpeedData[turbineId].FirstTimeStamp = timestamp;
                    }
                    float.TryParse(parts[2], out float rpm);

                    TimeSpan timeSinceStart = (TimeSpan)(timestamp - _turbineSpeedData[turbineId].FirstTimeStamp);
                    float minutesSinceStart = (float)timeSinceStart.TotalMinutes;

                    _turbineSpeedData[turbineId].SpeedData.Add(minutesSinceStart, rpm);
                }
            }
        }
    }
}