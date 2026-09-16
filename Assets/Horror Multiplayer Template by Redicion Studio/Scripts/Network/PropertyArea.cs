// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    [RequireComponent(typeof(BoxCollider))]
    public class PropertyArea : MonoBehaviour
    {

        private BoxCollider _boxCollider;

        [HideInInspector] public int index;

        public static int myIndex = -1;

        private void Awake()
        {
            _boxCollider = GetComponent<BoxCollider>();
        }

        public bool Contains(Bounds bounds)
        {
            return _boxCollider.bounds.Intersects(bounds);
        }

        public int ownerId;

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.tag == "Player")
                other.GetComponent<PlayerInteractionModule>().playerInventory.inPropertyArea = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.transform.tag == "Player")
                other.GetComponent<PlayerInteractionModule>().playerInventory.inPropertyArea = false;
        }

#if UNITY_SERVER// || UNITY_EDITOR // (Server)
	public static Dictionary<string, List<PropertyArea>> _properties = new Dictionary<string, List<PropertyArea>>();

	private string _instanceUniqueName;

	private void Start() {
		_instanceUniqueName = GetComponentInParent<Instance>().uniqueName;

		if (!_properties.ContainsKey(_instanceUniqueName)) {
			_properties[_instanceUniqueName] = new List<PropertyArea>();
		}
		_properties[_instanceUniqueName].Add(this);
		_properties[_instanceUniqueName] = _properties[_instanceUniqueName].OrderBy(value => value.transform.GetSiblingIndex()).ToList(); // ?
		index = _properties[_instanceUniqueName].IndexOf(this);
	}

	public static PropertyArea GetPropertyArea(string instanceUniqueName, int ownerId) {
		for (int i = 0; i < _properties[instanceUniqueName].Count; i++) {
			if (_properties[instanceUniqueName][i].ownerId == ownerId) {
				return _properties[instanceUniqueName][i];
			}
		}
		throw new UnityException("All property areas are occupied.");
	}

	public void AssignTo(int ownerId) {
		this.ownerId = ownerId;

		if (ownerId == 0) {
			return;
		}
	}

	public static int Assign(string instanceUniqueName, int ownerId) {
		PropertyArea propertyArea = GetPropertyArea(instanceUniqueName, 0);
		propertyArea.AssignTo(ownerId);
		return propertyArea.index;
	}

	private void OnDestroy() {
		_properties[_instanceUniqueName].Remove(this);
		if (_properties[_instanceUniqueName].Count < 1) {
			_properties.Remove(_instanceUniqueName);
		}
	}
#else
        public static List<PropertyArea> properties = new List<PropertyArea>();

        private void Start()
        {
            properties.Add(this);
            properties = properties.OrderBy(value => value.transform.GetSiblingIndex()).ToList(); // ?
            index = properties.IndexOf(this);
        }

        private void OnDestroy()
        {
            properties.Remove(this);
        }
#endif
#if UNITY_EDITOR
        [SerializeField] private Color _gizmoColor = new Color(0f, 1f, 1f, .24f);
        [SerializeField] private Color _gizmoWireColor = new Color(1f, 1f, 1f, .64f);

        private void OnDrawGizmos()
        {
            if (_boxCollider == null)
            {
                _boxCollider = GetComponent<BoxCollider>();
            }

            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.color = _gizmoColor;
            Gizmos.DrawCube(_boxCollider.center, _boxCollider.size);
            Gizmos.color = index == myIndex ? Color.red : _gizmoWireColor;
            Gizmos.DrawWireCube(_boxCollider.center, _boxCollider.size);
            Gizmos.matrix = Matrix4x4.identity; // ?
        }
#endif
    }
}