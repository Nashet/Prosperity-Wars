using Nashet.Map.GameplayControllers;
using UnityEngine;

namespace Nashet.Map.GameplayView
{
	public class MouseClicksView : MonoBehaviour
	{
		public event OnMouseButtonReleased MouseButtonReleased;

		private void Update()
		{
			if (Input.GetMouseButtonUp(1))
				MouseButtonReleased?.Invoke(MouseCode.RightButton);
			if (Input.GetMouseButtonUp(0))
				MouseButtonReleased?.Invoke(MouseCode.LeftButton);
		}
	}
}
