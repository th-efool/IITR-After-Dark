// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;
using System;
using System.IO;
using System.Collections;

namespace RedicionStudio.MasterServer
{
    public class DatabaseBackup : MonoBehaviour
    {
        private RedicionStudio.Settings.TemplateSettings templateSettings;

        private string databasePath;

        public float backupIntervalSeconds = 86400f;

        void Start()
        {
            StartCoroutine(LoadTemplateSettings());
        }

        IEnumerator LoadTemplateSettings()
        {
            while (templateSettings == null)
            {
                templateSettings = Resources.Load<RedicionStudio.Settings.TemplateSettings>("TemplateSettings");

                yield return null;
            }

            if (templateSettings != null)
            {
                if (templateSettings.automaticPlayerDataBackupCreation)
                {
                    string projectPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/"));
                    databasePath = Path.Combine(projectPath, "MasterServer_Data/db.sqlite");

                    InvokeRepeating("BackupDatabase", 0f, backupIntervalSeconds);
                }
            }
        }

        void BackupDatabase()
        {
            string databaseDirectory = Path.GetDirectoryName(databasePath);
            string todayBackupFolder = Path.Combine(databaseDirectory, "Backup", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
            Directory.CreateDirectory(todayBackupFolder);

            string databaseName = Path.GetFileName(databasePath);
            string backupDatabasePath = Path.Combine(todayBackupFolder, databaseName);
            File.Copy(databasePath, backupDatabasePath, true);

            UnityEngine.Debug.Log("Database backup completed: " + backupDatabasePath);
        }
    }
}
