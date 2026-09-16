// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RedicionStudio
{
    public class EmoteWheelArea : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Dependencies")]
        public EmoteWheel _emoteWheel;
        [Header("Area")]
        [Tooltip("0 = Top, 1 = Down, 2 = Right, 3 = Left, 4 = Middle")]
        public int AreaID;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (AreaID == 0)
            {
                _emoteWheel.currentEmoteName.gameObject.SetActive(true);
                _emoteWheel.currentEmoteInfo.gameObject.SetActive(true);
                _emoteWheel.lineUiElement.gameObject.SetActive(true);
                _emoteWheel.Left = false;
                _emoteWheel.Right = false;
                _emoteWheel.Down = false;
                _emoteWheel.Top = true;
            }
            else if (AreaID == 1)
            {
                _emoteWheel.currentEmoteName.gameObject.SetActive(true);
                _emoteWheel.currentEmoteInfo.gameObject.SetActive(true);
                _emoteWheel.lineUiElement.gameObject.SetActive(true);
                _emoteWheel.Left = false;
                _emoteWheel.Right = false;
                _emoteWheel.Top = false;
                _emoteWheel.Down = true;
            }
            else if (AreaID == 2)
            {
                _emoteWheel.currentEmoteName.gameObject.SetActive(true);
                _emoteWheel.currentEmoteInfo.gameObject.SetActive(true);
                _emoteWheel.lineUiElement.gameObject.SetActive(true);
                _emoteWheel.Left = false;
                _emoteWheel.Down = false;
                _emoteWheel.Top = false;
                _emoteWheel.Right = true;
            }
            else if (AreaID == 3)
            {
                _emoteWheel.currentEmoteName.gameObject.SetActive(true);
                _emoteWheel.currentEmoteInfo.gameObject.SetActive(true);
                _emoteWheel.lineUiElement.gameObject.SetActive(true);
                _emoteWheel.Right = false;
                _emoteWheel.Down = false;
                _emoteWheel.Top = false;
                _emoteWheel.Left = true;
            }
            else if (AreaID == 4)
            {
                _emoteWheel.currentEmoteName.gameObject.SetActive(false);
                _emoteWheel.currentEmoteInfo.gameObject.SetActive(false);
                _emoteWheel.lineUiElement.gameObject.SetActive(false);
                _emoteWheel.Right = false;
                _emoteWheel.Down = false;
                _emoteWheel.Top = false;
                _emoteWheel.Left = false;
                foreach (EmoteWheelItem _emoteItem in _emoteWheel.emotes)
                {
                    _emoteItem.Deselect();
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {

        }
    }
}