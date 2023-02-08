using Nashet.UnitSelection;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nashet.EconomicSimulation
{
    public class UnitPanel : Hideable//MonoBehaviour
    {
        //[SerializeField]
        //private Unit unit;

        [SerializeField]
        private Text panelText;

        [SerializeField]
        private RawImage flag;


        public void SetFlag(Texture2D flag)
        {
            this.flag.texture = flag;
        }
        public void SetText(string text)
        {
            panelText.text = text;
        }
        //public void Move(Province where)
        //{
        //    var position = where.getPosition();
        //    position.y += unitPanelYOffset;
        //    transform.position = position;
        //}

        private void Update()
        {
            HandleSendUnitTo();
        }

        private void HandleSendUnitTo()
        {
            // MOUSE RIGHT BUTTON clicked or Left clicked after SendButon clicked
            if (Game.selectedArmies.Count != 0 && (Input.GetMouseButtonUp(1) || Game.isInSendArmyMode && Input.GetMouseButtonUp(0)))
            {                
                SendUnitTo();
            }
        }

        private void SendUnitTo()
        {
            var collider = SelectionComponent.getRayCastMeshNumber();
            if (collider != null)
            {
                Province sendToPovince = null;
                int meshNumber = Province.FindByCollider(collider);
                if (meshNumber > 0) // send armies to another province
                    sendToPovince = World.FindProvince(meshNumber);
                else // better do here sort of collider layer, hitting provinces only
                {
                    var unit = SelectionComponent.GetUnit(collider);
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
                        item.SetPathTo(sendToPovince);
                    Game.provincesToRedrawArmies.Add(item.Province);
                }
                //Unit.RedrawAll();
                Game.ChangeIsInSendArmyMode(false);
            }
        }
    }
}