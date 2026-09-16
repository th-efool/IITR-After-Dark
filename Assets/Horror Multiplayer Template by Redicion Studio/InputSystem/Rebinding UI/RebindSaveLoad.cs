using UnityEngine;
using UnityEngine.InputSystem;

namespace RedicionStudio
{
    public class RebindSaveLoad : MonoBehaviour
    {
        public InputActionAsset actions;

        private void OnEnable()
        {
            LoadRebindings();
        }

        private void OnDisable()
        {
            SaveRebindings();
        }

        public void LoadRebindings()
        {
            var rebinds = PlayerPrefs.GetString("rebinds");
            if (!string.IsNullOrEmpty(rebinds))
            {
                actions.LoadBindingOverridesFromJson(rebinds);
            }
            else
            {
                // No rebindings found, using defaults.
            }
        }

        public void SaveRebindings()
        {
            var rebinds = actions.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString("rebinds", rebinds);
            PlayerPrefs.Save();
        }

        public void ResetBindings()
        {
            actions.RemoveAllBindingOverrides();
            PlayerPrefs.DeleteKey("rebinds");
            PlayerPrefs.Save();
        }
    }
}