using UnityEngine;

namespace Nashet.Map.GameplayControllers
{
	public interface IProvinceSelectionController
	{
		event OnProvinceSelected ProvinceSelected;

		void selectProvince(GameObject province, int? provinceId);
	}
}