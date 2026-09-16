using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio
{
    [System.Serializable]
    public class CreditsEntry
    {
        public string title;
        [Tooltip("Enter names for this credit entry")]
        public List<string> names = new List<string>();
    }

    public class CreditsManager : MonoBehaviour
    {
        [Tooltip("List of credit entries")]
        public List<CreditsEntry> creditEntries = new List<CreditsEntry>();
    }
}
