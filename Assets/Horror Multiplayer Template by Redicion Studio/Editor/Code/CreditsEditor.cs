using UnityEditor;
using UnityEngine;

namespace RedicionStudio
{
    [CustomEditor(typeof(CreditsManager))]
    public class CreditsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            CreditsManager credits = (CreditsManager)target;

            EditorGUILayout.LabelField("Credits Entries", EditorStyles.boldLabel);

            for (int i = 0; i < credits.creditEntries.Count; i++)
            {
                CreditsEntry entry = credits.creditEntries[i];

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"Entry {i + 1}", EditorStyles.boldLabel);

                entry.title = EditorGUILayout.TextField("Title", entry.title);

                EditorGUILayout.LabelField("Names:");
                for (int j = 0; j < entry.names.Count; j++)
                {
                    EditorGUILayout.BeginHorizontal();
                    entry.names[j] = EditorGUILayout.TextField($"Name {j + 1}", entry.names[j]);

                    if (GUILayout.Button("Remove Name", GUILayout.Width(100)))
                    {
                        entry.names.RemoveAt(j);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (GUILayout.Button("Add Name"))
                {
                    entry.names.Add("");
                }

                if (GUILayout.Button("Remove Entry"))
                {
                    credits.creditEntries.RemoveAt(i);
                    break;
                }

                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add Entry"))
            {
                credits.creditEntries.Add(new CreditsEntry());
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
}
