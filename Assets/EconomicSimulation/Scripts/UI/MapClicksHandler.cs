using Nashet.UnitSelection;
using Nashet.Utils;
using System.Linq;
using UnityEngine;

namespace Nashet.EconomicSimulation
{
    public class MapClicksHandler : MonoBehaviour
    {
        private SelectionComponent selector;
        private static int nextArmyToSelect = 0;

        private void Start()
        {
            selector = GetComponent<SelectionComponent>();
            selector.OnEntitySelected += EntitySelectedHandler;
            selector.OnProvinceSelected += ProvinceSelectedHandler;
        }

        private void OnDestroy()
        {
            selector.OnEntitySelected -= EntitySelectedHandler;
            selector.OnProvinceSelected -= ProvinceSelectedHandler;
        }

        private void ProvinceSelectedHandler(SelectionData selected)
        {
            if (Game.isInSendArmyMode)
                return;

            if (selected == null)
                MainCamera.selectProvince(-1);
            else
            {
                int provinceNumber = Province.FindByCollider(selected.SingleSelection);
                if (provinceNumber > 0)
                {
                    MainCamera.selectProvince(provinceNumber);
                    if (!Input.GetKey(LinksManager.Get.AdditionKey)) // don't de select units if AdditionKey is pressed
                    {
                        Game.selectedArmies.ToList().PerformAction(x => x.Deselect());
                        EntitySelectedHandler(null);
                    }
                }
            }
        }

        private void EntitySelectedHandler(SelectionData selected)
        {
            if (selected == null)
            {
                var c = Game.selectedArmies.ToList(); //to avoid collection change
                c.PerformAction(x => x.Deselect());
            }
            else if (selected.MultipleSelection != null)
            {
                HandleMultipleSelection(selected);
            }
            else
            {
                HandleSingleSelection(selected);
            }
        }

        private static void HandleSingleSelection(SelectionData selected)
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

        private void Update()
        {
            Game.previoslySelectedProvince = Game.selectedProvince;
        }

        public static Unit GetUnit(Collider collider)
        {
            return collider.transform.parent.GetComponent<Unit>();
        }
    }
}