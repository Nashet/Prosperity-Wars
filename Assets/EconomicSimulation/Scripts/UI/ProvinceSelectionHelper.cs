using Nashet.EconomicSimulation;
using UnityEngine;

namespace Nashet.GameplayControllers
{
	public class ProvinceSelectionHelper : MonoBehaviour
	{
		[SerializeField]
		private ProvinceSelectionController provinceSelectionController;

		public Province selectedProvince { get; private set; }
		private void Start()
		{
			provinceSelectionController.ProvinceSelected += ProvinceSelected;
		}

		private void OnDestroy()
		{
			provinceSelectionController.ProvinceSelected -= ProvinceSelected;
		}
		private void ProvinceSelected(int? provinceId)
		{
			if (provinceId.HasValue)
			{
				selectedProvince = World.ProvincesById[provinceId.Value];
			}
			else
			{
				selectedProvince = null;
			}
		}
	}
}