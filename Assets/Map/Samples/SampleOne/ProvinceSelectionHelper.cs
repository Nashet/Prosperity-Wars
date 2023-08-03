using UnityEngine;
using Nashet.Map.GameplayControllers;

namespace Nashet.Map.Examples
{
	public delegate void OnProvinceSelected(Province province);

	public class ProvinceSelectionHelper : MonoBehaviour
	{
		public event OnProvinceSelected ProvinceSelected;

		[SerializeField] private ProvinceSelectionController provinceSelectionController;

		public Province selectedProvince { get; private set; }
		public bool ProvinceChangedFromLastTick => lastTickSelectedProvince != selectedProvince;

		private Province lastTickSelectedProvince;

		private void Start()
		{
			provinceSelectionController.ProvinceSelected += ProvinceSelectedHandler;
		}

		private void OnDestroy()
		{
			provinceSelectionController.ProvinceSelected -= ProvinceSelectedHandler;
		}

		private void Update()
		{
			lastTickSelectedProvince = selectedProvince;
		}

		private void ProvinceSelectedHandler(int? provinceId)
		{
			if (provinceId.HasValue)
			{
				selectedProvince = Province.AllProvinces[provinceId.Value];
				//Debug.LogError($"You selected {selectedProvince} province! Owner is {selectedProvince.Country}");
			}
			else
			{
				selectedProvince = null;
			}
			ProvinceSelected?.Invoke(selectedProvince);
		}
	}
}