// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;

namespace RedicionStudio
{
    public abstract class UIBaseDialog : MonoBehaviour
    {

        [SerializeField] private UIButton _cancelButton;

        public virtual void Show()
        {
            UIFadeImage.instance.Show(.98f, null);
            transform.ModifyLocalScale(.0f, 1f, .18f);
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
            UIFadeImage.instance.Hide();
        }

        protected virtual void Start()
        {
            _cancelButton.onPressed = Hide;
        }
    }
}
