// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;
using TMPro;

namespace RedicionStudio
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class UIBuildInfoText : MonoBehaviour
    {

        private void Awake()
        {
            GetComponent<TextMeshProUGUI>().text = "Horror Multiplayer Template by Redicion Studio (v" + Application.version + ')';
        }
    }
}
