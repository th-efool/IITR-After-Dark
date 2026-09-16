// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;
using UnityEngine.UI;

namespace RedicionStudio
{
    public class UIFadeImage : MonoBehaviour
    {

        #region instance
        public static UIFadeImage instance;
        public UIFadeImage()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                throw new UnityException("instance");
            }
        }
        #endregion

        [SerializeField] private Image _fadeImage;
        [SerializeField] private float _tweenTime;

        private static int _queue;

        public void Show(float targetAlpha, System.Action onComplete)
        {
            _queue++;
            _fadeImage.gameObject.SetActive(true);
            _fadeImage.ModifyAlpha(_fadeImage.color.a, targetAlpha, Mathf.Abs(_tweenTime * (targetAlpha - _fadeImage.color.a)), onComplete);
        }

        public void Hide()
        {
            _queue--;
            if (!_fadeImage.gameObject.activeSelf || _queue > 0)
            {
                return;
            }
            _fadeImage.ModifyAlpha(_fadeImage.color.a, .0f, _tweenTime * _fadeImage.color.a, () => _fadeImage.gameObject.SetActive(false));
        }
    }
}
