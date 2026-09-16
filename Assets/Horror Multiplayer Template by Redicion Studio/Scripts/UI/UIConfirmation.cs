// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;
using TMPro;

namespace RedicionStudio
{
    public class UIConfirmation : UIBaseDialog
    {

        #region Instance
        public static UIConfirmation Instance { get; private set; }
        public UIConfirmation()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                throw new UnityException("Instance");
            }
        }
        #endregion

        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private UIButton _confirmButton;
        [SerializeField] private Vector2 _bSizeDelta;

        public void Show(string message, System.Action onConfirm)
        {
            base.Show();
            _confirmButton.onPressed = onConfirm;
            _confirmButton.onPressed += base.Hide;

            _messageText.SetText(message);
            _messageText.ForceMeshUpdate();
            ((RectTransform)transform).sizeDelta = new Vector2(_bSizeDelta.x, _bSizeDelta.y + _messageText.GetRenderedValues().y);
        }
    }
}
