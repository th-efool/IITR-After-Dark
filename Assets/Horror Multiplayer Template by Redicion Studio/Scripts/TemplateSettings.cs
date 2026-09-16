// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;

namespace RedicionStudio.Settings
{
    //[CreateAssetMenu(fileName = "New Setting SO", menuName = "Template Settings")]
    public class TemplateSettings : ScriptableObject
    {
        [Header("Settings")]

        [Space]
        [Tooltip("When hideSpecificWorldElements is set to 'true', all the world elements specified in the TemplateSettingsManager are hidden during runtime.")]
        public bool hideSpecificWorldElements = false;

        [Space]
        [Tooltip("If 'automaticPlayerDataBackupCreation' is set to 'true', the master server will create a backup of player data every time it boots up and every 24 hours thereafter.")]
        public bool automaticPlayerDataBackupCreation = false;
    }
}
