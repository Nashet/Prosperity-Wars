using Nashet.Map.Utils;
using Nashet.MapMeshes;
using Nashet.UnitSelection;
using UnityEngine;

namespace Nashet.Map.GameplayControllers
{
	public delegate void OnProvinceSelected(int? provinceId);

	public class ProvinceSelectionController : MonoBehaviour, IProvinceSelectionController
	{
		public event OnProvinceSelected ProvinceSelected; // todo remove from project

		[SerializeField] private CameraController cameraController;
		[SerializeField] private Material provinceSelectionMaterial;

		public bool isInSendArmyMode { get; private set; }
		private GameObject previoslySelectedProvince;
		private GameObject selectedProvince;
		private ISelector provinceSelector;

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