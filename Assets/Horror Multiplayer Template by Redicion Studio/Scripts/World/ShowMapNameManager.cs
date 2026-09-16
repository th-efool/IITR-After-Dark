using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowMapNameManager : MonoBehaviour
{
    public TMPro.TMP_Text mapNameText;
    public TMPro.TMP_Text mapDescriptionText;

    public void ShowMapName(string mapName, string mapDescription)
    {
        mapNameText.text = mapName;
        mapDescriptionText.text = mapDescription;
    }
}
