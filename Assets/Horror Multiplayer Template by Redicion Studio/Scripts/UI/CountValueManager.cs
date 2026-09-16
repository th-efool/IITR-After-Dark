// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio
{
    public class CountValueManager : MonoBehaviour
    {
        public float countDuration = 1;
        public TMPro.TMP_Text numberText;
        float currentValue = 0, targetValue = 0;
        public string additionalText;
        Coroutine _C2T;

        void Start()
        {
            currentValue = float.Parse(numberText.text);
            targetValue = currentValue;
        }

        IEnumerator CountTo(float targetValue)
        {
            var rate = Mathf.Abs(targetValue - currentValue) / countDuration;
            while (currentValue != targetValue)
            {
                currentValue = Mathf.MoveTowards(currentValue, targetValue, rate * Time.deltaTime);
                numberText.text = ((int)currentValue).ToString() + additionalText;
                yield return null;
            }
        }

        public void AddValue(float value)
        {
            targetValue += value;
            if (_C2T != null)
                StopCoroutine(_C2T);
            _C2T = StartCoroutine(CountTo(targetValue));
        }

        public void SetTarget(float target)
        {
            targetValue = target;
            if (_C2T != null)
                StopCoroutine(_C2T);
            _C2T = StartCoroutine(CountTo(targetValue));
        }
    }
}
