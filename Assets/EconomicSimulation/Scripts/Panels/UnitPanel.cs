using Nashet.UnitSelection;
using Nashet.UnityUIUtils;
using System;
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
        private new Camera camera;

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

        private void Start()
        {
            camera = Camera.main;
        }
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
            var collider = UnitSelection.Utils.getRayCastMeshNumber(camera);
            if (collider != null)
            {
                Province sendToPovince = null;
                int meshNumber = Province.FindByCollider(collider);
                if (meshNumber > 0) // send armies to another province
                    sendToPovince = World.FindProvince(meshNumber);
                else // better do here sort of collider layer, hitting provinces only
                {
                    var unit = MapClicksHandler.GetUnit(collider);
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
                        Predicate<Province> predicate = item.Province.Country == Game.Player && sendToPovince.Country == Game.Player ?
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