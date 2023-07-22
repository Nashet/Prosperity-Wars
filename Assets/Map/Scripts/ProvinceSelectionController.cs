using Nashet.MapMeshes;
using Nashet.UnitSelection;
using Nashet.Utils;
using UnityEngine;

namespace Nashet.GameplayControllers
{
	public delegate void OnProvinceSelected(int? provinceId);
		public class ProvinceSelectionController : MonoBehaviour
	{
		public event OnProvinceSelected ProvinceSelected;

		[SerializeField] private CameraController cameraController;
		[SerializeField] private Material provinceSelectionMaterial;

		public bool isInSendArmyMode { get; private set; }
		public GameObject previoslySelectedProvince;
		public GameObject selectedProvince;
		public ISelector provinceSelector;

		private SelectionComponent selector;		

		private void Start()
		{
			selector = GetComponent<SelectionComponent>();
			selector.OnProvinceClicked += ProvinceClickedHandler;
			provinceSelector = TimedSelectorWithMaterial.AddTo(gameObject, provinceSelectionMaterial, 0);
		}

		private void OnDestroy()
		{
			selector.OnProvinceClicked -= ProvinceClickedHandler;
		}

		private void ProvinceClickedHandler(SelectionData selected)
		{
			if (isInSendArmyMode)
				return;

			if (selected == null)
				selectProvince(null, null);
			else
			{
				int? provinceId = ProvinceMesh.GetIdByCollider(selected.SingleSelection);
				var obj = selected.SingleSelection.gameObject;

				if (provinceId != null)
				{
					selectProvince(obj, provinceId);					
				}
			}
		}

		private void Update()
		{
			previoslySelectedProvince = selectedProvince;
		}

		public void selectProvince(GameObject province, int? provinceId)
		{
			if (province == null || province == selectedProvince)// same province clicked, hide selection
			{
				var lastSelected = selectedProvince;
				selectedProvince = null;

				if (lastSelected != null)
				{
					provinceSelector.Deselect(lastSelected);
				}
				ProvinceSelected?.Invoke(null);
			}
			else // new province selected
			{
				if (selectedProvince != null)//deal with previous selection
				{
					provinceSelector.Deselect(selectedProvince);
				}
				// freshly selected province
				selectedProvince = province;
				provinceSelector.Select(selectedProvince);
				ProvinceSelected?.Invoke(provinceId);

			}
		}
	}
}