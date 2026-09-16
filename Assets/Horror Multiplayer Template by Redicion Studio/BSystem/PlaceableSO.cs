// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio.BSystem {

	//[CreateAssetMenu(fileName = "New Placeable SO", menuName = "BSystem/Placeable SO")]
	public class PlaceableSO : ScriptableObject {

		public class Category {

			public string name;
			public List<PlaceableSO> placeableSOs;
			public List<Category> subcategories;
		}

		public static List<Category> globalCategories;

		public static Category GetCategory(ref List<Category> categories, string name) {
			for (int i = 0; i < categories.Count; i++) {
				if (categories[i].name == name) {
					return categories[i];
				}
			}
			return null;
		}

		private static Category GetCreateCategory(ref List<Category> categories, string name) {
			Category category = GetCategory(ref categories, name);
			if (category == null) {
				category = new Category {
					name = name,
					placeableSOs = new List<PlaceableSO>(),
					subcategories = new List<Category>()
				};
				categories.Add(category);
			}
			return category;
		}

		public static Category GetCategory(string categoryStr) {
			string[] array = categoryStr.Split('/');
			Category category = GetCreateCategory(ref globalCategories, array[0]);
			for (int i = 1; i < array.Length; i++) {
				category = GetCreateCategory(ref category.subcategories, array[i]);
			}
			return category;
		}

		private static void Create(PlaceableSO placeableSO) {
			if (string.IsNullOrWhiteSpace(placeableSO.category)) { // ?
				return;
			}

			GetCategory(placeableSO.category).placeableSOs.Add(placeableSO);
		}

		private static PlaceableSO[] _placeableSOs;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void LoadAll() {
			_placeableSOs = Resources.LoadAll<PlaceableSO>(string.Empty);

#if !UNITY_SERVER || UNITY_EDITOR // (Client)
			globalCategories = new List<Category>();
			for (int i = 0; i < _placeableSOs.Length; i++) {
				Create(_placeableSOs[i]);
			}
			Debug.Log(globalCategories.Count + " global categories have been loaded");
#endif
		}

		public string uniqueName;

		[Space]
		public string category;

		public static PlaceableSO GetPlaceableSO(string uniqueName) {
			for (int i = 0; i < _placeableSOs.Length; i++) {
				if (_placeableSOs[i].uniqueName == uniqueName) {
					return _placeableSOs[i];
				}
			}
			return null;
		}

		public enum Type : byte {
			Object,
			Floor,
			Wall
		}

		[Space]
		public Type type;
		public Vector3 size;

		public const float Size = 4f;
		public const float Thickness = .16f;

		public static Vector3 GetSize(Type type) {
			switch (type) {
				default:
					throw new UnityException("type");
				case Type.Floor:
					return new Vector3(Size, Thickness, Size);
				case Type.Wall:
					return new Vector3(Size, Size, Thickness);
			}
		}

		public Vector3 GetSize() {
			if (type == Type.Object) {
				return size;
			}
			return GetSize(type);
		}

		public bool CanBePlacedOnGround => type == Type.Floor || type == Type.Object;

		[Space]
		public GameObject modelPrefab;
		public Sprite sprite;

		[Space]
		public int price;
		public int sellPrice;

		//[Space]
		//public int maxPerPlayer;

		public enum Direction : byte {
			Backward,
			Left,
			Forward,
			Right
		}

		public static Vector3 GetDirectionEulerAngleY(Direction direction) {
			switch (direction) {
				default:
					throw new UnityException("direction");
				case Direction.Backward:
					return Vector3.zero;
				case Direction.Left:
					return new Vector3(0f, 90f, 0f);
				case Direction.Forward:
					return new Vector3(0f, 180f, 0f);
				case Direction.Right:
					return new Vector3(0f, 270f, 0f);
			}
		}

		private static readonly Vector3 _scalarAdjustment = new Vector3(.18f, .18f, .18f);

		private static Vector3 _scalarCachedSize;

		private Vector3 Scalar() {
			_scalarCachedSize = GetSize();
			return type == Type.Object ? _scalarCachedSize - _scalarAdjustment : new Vector3(Mathf.Max(_scalarCachedSize.x - Thickness, Thickness), // Thickness * 2f ?
				Mathf.Max(_scalarCachedSize.y - Thickness, Thickness),
				Mathf.Max(_scalarCachedSize.z - Thickness, Thickness)) - _scalarAdjustment; // ?
		}

		public bool CheckBox(Vector3 position, Quaternion rotation, LayerMask mask) {
			return Physics.CheckBox(position, Scalar() / 2f, rotation, mask, QueryTriggerInteraction.Ignore);
		}
	}
}
