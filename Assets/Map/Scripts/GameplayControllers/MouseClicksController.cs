using Nashet.Map.GameplayView;
using UnityEngine;

namespace Nashet.Map.GameplayControllers
{
	public enum MouseCode { LeftButton = 0, RightButton = 1 }
	public delegate void OnMouseButtonReleased(MouseCode mouseCode);

	/// <summary>
	/// Should be placed on Camera
	/// </summary>
	public class MouseClicksController : MonoBehaviour
	{

		public event OnMouseButtonReleased MouseButtonReleased;

		private MouseClicksView mouseClicksView;

		private void Awake()
		{
			mouseClicksView = GetComponent<MouseClicksView>();
			mouseClicksView.MouseButtonReleased += HandleMouseButtonReleased;
		}

		private void OnDestroy()
		{
			mouseClicksView.MouseButtonReleased -= HandleMouseButtonReleased;
		}

		private void HandleMouseButtonReleased(MouseCode buttonCode)
		{
			MouseButtonReleased?.Invoke(buttonCode);
		}
	}
}