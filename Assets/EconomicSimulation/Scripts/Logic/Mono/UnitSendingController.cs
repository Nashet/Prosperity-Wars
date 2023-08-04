using Nashet.GameplayControllers;
using Nashet.Map.GameplayControllers;
using Nashet.MapMeshes;
using Nashet.UnitSelection;
using QPathFinder;
using System;
using UnityEngine;

namespace Nashet.EconomicSimulation
{
	public class UnitSendingController : MonoBehaviour
	{
		private MouseClicksController clicksController;
		private new Camera camera;

		private void Start()
		{
			camera = Camera.main;
			clicksController = camera.GetComponent<MouseClicksController>();
			clicksController.MouseButtonReleased += ClicksController_MouseButtonReleased;
		}

		private void ClicksController_MouseButtonReleased(MouseCode mouseCode)
		{
			if (Game.selectedArmies.Count != 0 && (mouseCode == MouseCode.RightButton || Game.isInSendArmyMode && mouseCode == MouseCode.LeftButton))
			{
				//add message cant find de way?
				SendUnitsTo();
			}
		}

		private void SendUnitsTo()
		{
			var collider = UnitSelectionUtils.getRayCastMeshNumber(camera);
			if (collider != null)
			{
				Province sendToPovince = null;
				int? meshNumber = ProvinceMesh.GetIdByCollider(collider);
				if (meshNumber.HasValue) // send armies to another province
					sendToPovince = World.FindProvince(meshNumber.Value);
				else // better do here sort of collider layer, hitting provinces only
				{
					var unit = UnitSelectionController.GetUnit(collider);
					if (unit != null)
					{
						sendToPovince = unit.Province;
					}
				}

				if (sendToPovince == null)
					return;

				var addPath = Input.GetKey(LinksManager.Get.AdditionKey);

				foreach (var item in Game.selectedArmies)
				{
					if (addPath)
						item.AddToPath(sendToPovince);
					else
					{
						Predicate<IProvince> predicate = item.Province.Country == Game.Player && sendToPovince.Country == Game.Player ?
							x => x.Country == Game.Player : null;
						item.SetPathTo(sendToPovince, predicate);
					}
					Game.provincesToRedrawArmies.Add(item.Province);
				}
				//Unit.RedrawAll();
				Game.ChangeIsInSendArmyMode(false);
			}
		}
	}
}