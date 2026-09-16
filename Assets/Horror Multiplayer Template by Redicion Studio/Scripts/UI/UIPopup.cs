// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;
using TMPro;

namespace RedicionStudio
{
    public class UIPopup : UIBaseDialog
    {

        public static UIPopup instance;
        public UIPopup()
        {
            if (instance == null)
            {
                instance = this;
            }
        }

        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Vector2 _bSizeDelta;

        public void Show(string message)
        {
            base.Show();
            _messageText.SetText(message);
            _messageText.ForceMeshUpdate();
            ((RectTransform)transform).sizeDelta = new Vector2(_bSizeDelta.x, _bSizeDelta.y + _messageText.GetRenderedValues().y);
        }
    }
}
