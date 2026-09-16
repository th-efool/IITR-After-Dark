// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;

namespace RedicionStudio.NetworkUtils {

	public abstract class SONAlloc : ScriptableObject { // ?

		private string nameCache;
#pragma warning disable IDE1006
		public new string name {
#pragma warning restore IDE1006
			get {
				if (nameCache == null) {
					nameCache = base.name;
				}
				return nameCache;
			}
		}
	}
}
