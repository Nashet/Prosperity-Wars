using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using Nashet.EconomicSimulation;
using Nashet.ValueSpace;
namespace Nashet.UnityUIUtils
{
    public class SampleButton : MonoBehaviour, IPointerDownHandler
    {

        public Button buttonComponent;
        public Text nameLabel;
        public Image iconImage;
        private object link;
        private DragPanel parent;


        // Use this for initialization
        void Start()
        {
            buttonComponent.onClick.AddListener(HandleClick);
        }

        //public void Setup(string text, PopUnit ipopUnit, MyTable currentScrollList)
        public void Setup(string text, object link)
        {
            this.link = link;
            nameLabel.text = text;
            parent = GetComponentInParent<DragPanel>();
        }
        public void OnPointerDown(PointerEventData data)
        {
            parent.OnPointerDown(data);
        }
        private void HandleClick()
        {
            if (link == null)
                return;
            if (link is Factory)
            {
                MainCamera.factoryPanel.Show((Factory)link);
                MainCamera.factoryPanel.refresh();
            }
            else if (link is PopUnit)
            {
                MainCamera.popUnitPanel.show((PopUnit)link);
                MainCamera.popUnitPanel.refresh();
            }
            else if (link is Product)
            {
                MainCamera.goodsPanel.Show((Product)link, true);
                MainCamera.goodsPanel.refresh();
            }
            else if (link is Storage)
            {
                var storage = link as Storage;
                if (!storage.isAbstractProduct())
                    MainCamera.tradeWindow.selectProduct((storage).getProduct());
            }
            else if (link is Invention)
            {
                //(Invention)link            
                MainCamera.inventionsPanel.refresh((Invention)link);
            }
            else if (link is FactoryType)
            {
                //MainCamera.buildPanel.selectedFactoryType = (FactoryType)link;
                MainCamera.buildPanel.refresh((FactoryType)link);
            }
            else if (link is AbstractReform)
            {
                //MainCamera.politicsPanel.selectedReform = (AbstractReform)link;
                MainCamera.politicsPanel.refresh(true, (AbstractReform)link);
                //MainCamera.politicsPanel.selectedReformValue = null;
            }
            else if (link is Province)
            {
                //MainCamera.politicsPanel.selectedReform = (AbstractReform)obj;
                //MainCamera.politicsPanel.refresh(true);
                //MainCamera.politicsPanel.selectedReformValue = null;
                Province temp = (Province)(link);
                MainCamera.SelectProvince(temp.getID());
            }
            else if (link is Country)
            {
                var country = link as Country;
                if (MainCamera.diplomacyPanel.isActiveAndEnabled)
                {
                    if (MainCamera.diplomacyPanel.getSelectedCountry() == country)

                        MainCamera.diplomacyPanel.hide();
                    else
                        MainCamera.diplomacyPanel.show(country);
                }
                else
                    MainCamera.diplomacyPanel.show(country);
            }


        }
        //internal void Setup(string text, UnityEngine.Events.UnityAction handleClick, ProductionWindowTable productionWindowTable, Factory stor)
        //{
        //    Setup( text,  handleClick, productionWindowTable);
        //}
    }
}