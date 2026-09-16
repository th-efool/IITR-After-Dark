// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio
{
    public class ShowFPS : MonoBehaviour
    {
        private const float UpdateInterval = 1.0f;
        private float _timeleft;
        private float _lastTime;
        private float _timeSpan;
        private int _lastFrame;
        private int _frames;
        private float _fps;

        void Update()
        {
            _timeleft += Time.deltaTime;
            if (_timeleft > UpdateInterval)
            {
                _timeleft -= UpdateInterval;
                _frames = Time.frameCount - _lastFrame;
                _lastFrame = Time.frameCount;
                _timeSpan = Time.realtimeSinceStartup - _lastTime;
                _lastTime = Time.realtimeSinceStartup;
                _fps = Mathf.RoundToInt(_frames / _timeSpan);
            }
        }

        void OnGUI()
        {
            int width = 70;
            int height = 25;
            int x = (Screen.width / 2) - (width / 2);
            int y = 10;

            GUI.Box(new Rect(x, y, width, height), string.Format("fps {0}", _fps));
        }
    }
}