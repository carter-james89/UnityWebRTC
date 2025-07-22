using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WindTurbine
{
    /// <summary>
    /// UI for <see cref="IWindTurbine"/>
    /// </summary>
    public class TurbineUI : MonoBehaviour
    {
        [SerializeField] private TextMeshPro _turbineIDText;
        [SerializeField] private TextMeshProUGUI _turbineSpeedText;
        [SerializeField] private Transform _panel;

        [SerializeField] private float _maxDist = 3;
        [SerializeField] private float _distFromUser = 1;
        [SerializeField]
        private float _distBelowUser = .2f;

         [SerializeField] private RectTransform _maxSpeedImage;
        [SerializeField] private RectTransform _speedBar;
        [SerializeField] float _maxRpm = 30f;


        private IWindTurbine _myTurbine;
        internal void Initialize(IWindTurbine turbine)
        {
            _myTurbine = turbine;
            _turbineIDText.text = turbine.GetTurbineID().ToString();
        }

        // Update is called once per frame
        void Update()
        {
            var tempPos = transform.position;
            tempPos.y = Camera.main.transform.position.y - _distBelowUser;
            transform.position = tempPos;

            transform.LookAt(Camera.main.transform.position);
            var tempEuler = transform.eulerAngles;
            tempEuler.z = 0;
            transform.eulerAngles = tempEuler;

            var dir = Camera.main.transform.position - transform.position;
            _panel.position = Camera.main.transform.position - (dir.normalized * _distFromUser);
            var dist = Vector3.Distance(transform.position, _panel.transform.position);


            if (dist > _maxDist)
            {
                _panel.position = transform.position + (dir.normalized * _maxDist);
            }
            UpdateSpeedVisual(_myTurbine.GetCurrentSpeed());

        }


        void UpdateSpeedVisual(float rpm)
        {
            float newHeight = Mathf.Clamp((rpm / _maxRpm) * _maxSpeedImage.sizeDelta.y, 0, _maxSpeedImage.sizeDelta.y);
            Vector2 size = _speedBar.sizeDelta;
            size.y = newHeight;
            _speedBar.sizeDelta = size;

            _turbineSpeedText.text = "CURRENT \n" + rpm.ToString() + " RPM";

            var tempPos = _turbineSpeedText.transform.localPosition;
            tempPos.y = newHeight;
            _turbineSpeedText.transform.localPosition = tempPos;

            if (rpm > 20)
            {
                _speedBar.GetComponent<Image>().color = Color.red;//not fast but out of time
            }
            else
            {
                _speedBar.GetComponent<Image>().color = Color.green;//not fast but out of time
            }
        }
    }

}