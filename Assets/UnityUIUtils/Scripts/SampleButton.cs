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
        [SerializeField]
        private Button buttonComponent;

        [SerializeField]
        private Text nameLabel;

        [SerializeField]
        private Image iconImage;
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
                MainCamera.factoryPanel.show((Factory)link);
                MainCamera.factoryPanel.Refresh();
            }
            else if (link is PopUnit)
            {
                MainCamera.popUnitPanel.show((PopUnit)link);
                MainCamera.popUnitPanel.Refresh();
            }
            else if (link is Product)
            {
                MainCamera.goodsPanel.show((Product)link, true);
                MainCamera.goodsPanel.Refresh();
            }
            else if (link is Storage)
            {
                var storage = link as Storage;
                if (!storage.isAbstractProduct())
                    MainCamera.tradeWindow.selectProduct((storage).getProduct());
            }
            else if (link is Invention)
            {                
                MainCamera.inventionsPanel.selectInvention((Invention)link);
                MainCamera.inventionsPanel.Refresh();
            }
            else if (link is FactoryType)
            {
                MainCamera.buildPanel.selectFactoryType((FactoryType)link);
                MainCamera.buildPanel.Refresh();
            }
            else if (link is AbstractReform)
            {
                MainCamera.politicsPanel.selectReform((AbstractReform)link);
                MainCamera.politicsPanel.Refresh();                
            }
            else if (link is Province)
            {
                //MainCamera.politicsPanel.selectedReform = (AbstractReform)obj;
                //MainCamera.politicsPanel.refresh(true);
                //MainCamera.politicsPanel.selectedReformValue = null;
                Province temp = (Province)(link);
                MainCamera.selectProvince(temp.getID());
            }
            else if (link is Country)
            {
                var country = link as Country;
                if (MainCamera.diplomacyPanel.isActiveAndEnabled)
                {
                    if (MainCamera.diplomacyPanel.getSelectedCountry() == country)

                        MainCamera.diplomacyPanel.Hide();
                    else
                        MainCamera.diplomacyPanel.show(country);
                }
                else
                    MainCamera.diplomacyPanel.show(country);
            }
        }        
    }
}