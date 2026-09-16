// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;

namespace RedicionStudio.InventorySystem {

	[CreateAssetMenu(fileName = "Configuration", menuName = "Tools/Horror Multiplayer Template by Redicion Studio/Inventory System/Create new configuration file")]
	public class ConfigurationSO : ScriptableObject {

		public static ConfigurationSO Instance { get; private set; }

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void Load() {
			ConfigurationSO[] configurationSOs = Resources.LoadAll<ConfigurationSO>(string.Empty);

			if (configurationSOs.Length < 1) {
				throw new UnityException("Configuration scriptable object file could not be found.");
			}

			Instance = configurationSOs[0];

			if (configurationSOs.Length > 1) {
				Debug.LogWarning("Multiple configuration scriptable objects have been found.");
			}
		}

		public GameObject itemDropPrefab;

		[Header("Rarity")]
		public Color commonColor;
		public Color rareColor;
		public Color uniqueColor;
	}
}
