using UnityEngine;
using UnityEngine.UI;

namespace Nashet.Map.Examples
{
	public class ProvincePanel : MonoBehaviour
	{
		[SerializeField] ProvinceSelectionHelper helper;
		[SerializeField] Text textMesh;

		// Start is called before the first frame update
		void Start()
		{
			helper.ProvinceSelected += Helper_ProvinceSelected;
		}

		private void Helper_ProvinceSelected(Province selectedProvince)
		{
			if (selectedProvince == null)
			{
				return;
			}
			textMesh.text = $"You selected {selectedProvince} province! Owner is {selectedProvince.Country ?? "No one"}";
		}
	}
}