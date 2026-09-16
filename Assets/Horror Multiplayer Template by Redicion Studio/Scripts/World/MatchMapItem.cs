// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RedicionStudio
{
    public class MatchMapItem : MonoBehaviour
    {
        public int mapId;
        public TMPro.TMP_Text mapNameText;
        public Image mapImage;

        public GameObject selectedUIElement;

        public void SetUpMatchMapItem(string mapName, Sprite mapSprite, int matchMapId)
        {
            mapId = matchMapId;
            mapNameText.text = mapName;
            mapImage.sprite = mapSprite;
        }

        public void SetCurrentMatchMap()
        {
            OfflineMainMenuManager._instance.currentSelectedMapId = mapId;
            foreach(MatchMapItem mapItem in OfflineMainMenuManager._instance.matchMapItemList)
            {
                mapItem.selectedUIElement.SetActive(false);
            }
            selectedUIElement.SetActive(true);
        }
    }
}
