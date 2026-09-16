// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class UIWaypoint : MonoBehaviour
    {

#if !UNITY_SERVER || UNITY_EDITOR
        [SerializeField] private Image _image;
        [SerializeField] private TextMeshProUGUI _text;

        [Space]
        [SerializeField] private Vector3 _offset;

        private static Camera _camera;

        private void Awake()
        {
            //_camera = FindObjectOfType<Camera>();
            _camera = GameObject.Find("MainCamera").GetComponent<Camera>();
        }

        private Vector2 _worldToScreenPoint;
        private void Update()
        {
            if (Player.localPlayer == null || PropertyArea.myIndex == -1 || PropertyArea.properties.Count < 1)
            {
                _image.enabled = false;
                _text.enabled = false;
                return;
            }

            Vector3 propertyPosition = PropertyArea.properties[PropertyArea.myIndex].transform.position;
            int distance = (int)Vector3.Distance(Player.localPlayer.transform.position, propertyPosition);

            if (distance <= 10)
            {
                _image.enabled = false;
                _text.enabled = false;
                return;
            }

            _image.enabled = true;
            _text.enabled = true;

            float minX = _image.GetPixelAdjustedRect().width / 2f;
            float maxX = Screen.width - minX;

            float minY = _image.GetPixelAdjustedRect().height / 2f;
            float maxY = Screen.height - minY;

            _worldToScreenPoint = _camera.WorldToScreenPoint(propertyPosition + _offset);

            if (Vector3.Dot(propertyPosition - Player.localPlayer.transform.position, _camera.transform.forward) < 0f)
            {
                if (_worldToScreenPoint.x < Screen.width / 2f)
                {
                    _worldToScreenPoint.x = maxX;
                }
                else
                {
                    _worldToScreenPoint.x = minX;
                }
            }

            _worldToScreenPoint.x = Mathf.Clamp(_worldToScreenPoint.x, minX, maxX);
            _worldToScreenPoint.y = Mathf.Clamp(_worldToScreenPoint.y, minY, maxY);

            transform.position = _worldToScreenPoint;

            _text.text = "Your Property\n<color=#aaa>[" + ((int)Vector3.Distance(Player.localPlayer.transform.position, propertyPosition)).ToString() + "m]</color>";
        }

        // Refactor ALL
#endif
    }
}