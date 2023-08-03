using Nashet.EconomicSimulation;
using Nashet.Map.GameplayControllers;
using UnityEngine;

namespace Nashet.GameplayControllers
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
				selectedProvince = World.ProvincesById[provinceId.Value];
			}
			else
			{
				selectedProvince = null;
			}
			ProvinceSelected?.Invoke(selectedProvince);
		}
	}
}