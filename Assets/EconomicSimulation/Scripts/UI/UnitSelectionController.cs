using Nashet.EconomicSimulation;
using Nashet.UnitSelection;
using Nashet.Map.Utils;
using System;
using System.Linq;
using UnityEngine;

namespace Nashet.GameplayControllers
{
	public class UnitSelectionController : MonoBehaviour
	{
		private SelectionComponent selector;
		private int nextArmyToSelect = 0;

		private void Start()
		{
			selector = GetComponent<SelectionComponent>();
			selector.OnEntitySelected += EntitySelectedHandler;
			selector.OnMultipleEntitiesSelected += MultipleEntitiesSelectedHandler;
		}
		
		private void OnDestroy()
		{
			selector.OnEntitySelected -= EntitySelectedHandler;
			selector.OnMultipleEntitiesSelected -= EntitySelectedHandler;
		}

		private void EntitySelectedHandler(SelectionData selected, int buttonNumber)
		{
			if (selected == null)
			{
				var c = Game.selectedArmies.ToList(); //to avoid collection change
				c.PerformAction(x => x.Deselect());
			}
			else
			{
				HandleSingleSelection(selected);
			}
		}

		private void MultipleEntitiesSelectedHandler(SelectionData data, int buttonNumber)
		{
			HandleMultipleSelection(data);
		}

		private static void HandleMultipleSelection(SelectionData selected)
		{
			if (!Input.GetKey(LinksManager.Get.AdditionKey))
			{
				Game.selectedArmies.ToList().PerformAction(x => x.Deselect());
			}
			foreach (var item in selected.MultipleSelection)
			{
				var unit = GetUnit(item);
				var army = Game.Player.AllArmies().FirstOrDefault(x => x.unit.Collider == item);

				army.Select();
			}
		}

		private void HandleSingleSelection(SelectionData selected)
		{
			if (Game.isInSendArmyMode)
				return;

			var unit = GetUnit(selected.SingleSelection);

			if (unit == null)
			{
				return;
			}

			var army = unit.Province.AllStandingArmies().Where(x => x.getOwner() == Game.Player).Next(ref nextArmyToSelect);
			if (army != null)
			{
				if (Input.GetKey(LinksManager.Get.AdditionKey))
				{
					if (Game.selectedArmies.Contains(army))
						army.Deselect();
					else
						army.Select();
				}
				else
				{
					if (Game.selectedArmies.Count > 0)
					{
						Game.selectedArmies.ToList().PerformAction(x => x.Deselect());
					}
					army.Select();
				}
			}
		}

		public static Unit GetUnit(Collider collider)
		{
			return collider.transform.parent.GetComponent<Unit>();
		}
	}
}