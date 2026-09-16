// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;
using UnityEngine.UI;

namespace RedicionStudio
{
    public static class UIStuff
    {

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void TweenInit()
        {
            LeanTween.init();
        }

        public static bool useTween = true; // TMP: true

        public static void ModifyAlpha(this Image targetImage,
            float from, float to, float tweenTime, System.Action onComplete)
        {
            targetImage.gameObject.LeanCancel(false);

            Color tempColor = targetImage.color;

            if (!useTween)
            {
                tempColor.a = to;
                targetImage.color = tempColor;
                onComplete?.Invoke();
                return;
            }

            tempColor.a = from;
            targetImage.color = tempColor;
            _ = LeanTween.value(targetImage.gameObject, from, to, tweenTime).setOnUpdate((float val) => {
                tempColor.a = val;
                targetImage.color = tempColor;
            }).setOnComplete(() => onComplete?.Invoke());
        }

        public static void ModifyColor(this Image targetImage,
            Color from, Color to, float tweenTime)
        {
            targetImage.gameObject.LeanCancel(false);

            if (!useTween)
            {
                targetImage.color = to;
                return;
            }

            targetImage.color = from;
            _ = LeanTween.value(targetImage.gameObject, from, to, tweenTime).setOnUpdate((Color val) => {
                targetImage.color = val;
            });
        }

        public static void ModifyLocalScale(this Transform targetTransform,
            float from, float to, float tweenTime)
        {
            targetTransform.gameObject.LeanCancel(false);

            if (!useTween)
            {
                targetTransform.localScale = new Vector3(to, to);
                return;
            }

            targetTransform.localScale = new Vector3(from, from);
            _ = LeanTween.value(targetTransform.gameObject, from, to, tweenTime).setOnUpdate((float val) => {
                targetTransform.localScale = new Vector3(val, val);
            });
        }

        public static void ModifyLocalEulerAnglesZ(this Transform targetTransform,
            float from, float to, float tweenTime)
        {
            targetTransform.gameObject.LeanCancel(false);

            if (!useTween)
            {
                targetTransform.localEulerAngles = new Vector3(.0f, .0f, -to);
                return;
            }

            targetTransform.localEulerAngles = new Vector3(.0f, .0f, -from);
            _ = LeanTween.value(targetTransform.gameObject, from, to, tweenTime).setOnUpdate((float val) => {
                targetTransform.localEulerAngles = new Vector3(.0f, .0f, -val);
            });
        }

        public static void ModifyAnchoredPosX(this RectTransform targetTransform,
            float from, float to, float tweenTime)
        {
            targetTransform.gameObject.LeanCancel(false);

            if (!useTween)
            {
                targetTransform.anchoredPosition = new Vector2(to, targetTransform.anchoredPosition.y);
                return;
            }

            targetTransform.anchoredPosition = new Vector2(from, targetTransform.anchoredPosition.y);
            _ = LeanTween.value(targetTransform.gameObject, from, to, tweenTime).setOnUpdate((float val) => {
                targetTransform.anchoredPosition = new Vector2(val, targetTransform.anchoredPosition.y);
            });
        }
    }
}