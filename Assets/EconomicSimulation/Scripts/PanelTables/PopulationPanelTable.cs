using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using System.Linq;

namespace Nashet.EconomicSimulation
{
    public class PopulationPanelTable : UITableNew
    {
        private SortOrder needsFulfillmentOrder, unemploymentOrder, loyaltyOrder;
        private void Start()
        {
            needsFulfillmentOrder = new NeedsFulfillmentOrder(this);
            unemploymentOrder = new UnemploymentOrder(this);
            loyaltyOrder = new LoyaltyOrder(this);
        }
        public override void Refresh()
        {
            StartUpdate();
            //lock (gameObject)
            {
                RemoveButtons();
                var howMuchRowsShow = CalcSize(Game.popsToShowInPopulationPanel.Count);
                AddHeader();
                if (Game.popsToShowInPopulationPanel.Count > 0)
                {
                    for (int i = 0; i < howMuchRowsShow; i++)
                    //foreach (PopUnit record in Game.popsToShowInPopulationPanel)
                    {
                        PopUnit pop = Game.popsToShowInPopulationPanel[i + GetRowOffset()];

                        // Adding number
                        //AddButton(Convert.ToString(i + offset), record);

                        // Adding PopType
                        AddButton(pop.popType.ToString(), pop);
                        ////Adding province
                        AddButton(pop.getProvince().ToString(), pop.getProvince(), () => "Click to select this province");
                        ////Adding population
                        AddButton(System.Convert.ToString(pop.getPopulation()), pop);
                        ////Adding culture
                        AddButton(pop.culture.ToString(), pop);

                        ////Adding education
                        AddButton(pop.education.ToString(), pop);

                        ////Adding cash
                        AddButton(pop.cash.ToString(), pop);

                        ////Adding needs fulfilling

                        //PopUnit ert = record;
                        AddButton(pop.needsFullfilled.ToString(), pop,
                            //() => ert.consumedTotal.ToStringWithLines()                        
                            () => "Consumed:\n" + pop.getConsumed().getContainer().getString("\n")
                            );

                        ////Adding loyalty
                        string accu;
                        PopUnit.modifiersLoyaltyChange.getModifier(pop, out accu);
                        AddButton(pop.loyalty.ToString(), pop, () => accu);

                        //Adding Unemployment
                        AddButton(pop.getUnemployedProcent().ToString(), pop);

                        //Adding Movement
                        if (pop.getMovement() == null)
                            AddButton("", pop);
                        else
                            AddButton(pop.getMovement().getShortName(), pop, () => pop.getMovement().getName());
                    }
                }
            }
            EndUpdate();
        }        
        protected override void AddHeader()
        {
            // Adding number
            // AddButton("Number");

            // Adding PopType
            AddButton("Type");

            ////Adding province
            AddButton("Province");

            ////Adding population
            AddButton("Population");

            ////Adding culture
            AddButton("Culture");

            ////Adding education
            AddButton("Education");

            ////Adding storage
            //if (null.storage != null)
            AddButton("Cash");
            //else AddButton("Administration");

            ////Adding needs fulfilling
            AddButton("Needs fulfilled " + needsFulfillmentOrder.getSymbol(), needsFulfillmentOrder);

            ////Adding loyalty
            AddButton("Loyalty "+loyaltyOrder.getSymbol(), loyaltyOrder);

            ////Adding Unemployment
            AddButton("Unemployment " + unemploymentOrder.getSymbol(), unemploymentOrder);

            //Adding Movement
            AddButton("Movement");
        }
        private class NeedsFulfillmentOrder : SortOrder
        {
            public NeedsFulfillmentOrder(UITableNew parent) : base(parent) { }
            public override void OnClickedCell()
            {
                base.OnClickedCell();
                Game.popsToShowInPopulationPanel = DoSorting(Game.popsToShowInPopulationPanel, x => x.needsFullfilled.get());
                //MainCamera.populationPanel.Refresh();
                getParent().Refresh();
            }
            private void makeDefaultList()
            {
                if (MainCamera.populationPanel.showingProvince == null)
                    MainCamera.populationPanel.SetAllPopsToShow();
            }
        }
        private class LoyaltyOrder : SortOrder
        {
            public LoyaltyOrder(UITableNew parent) : base(parent) { }
            public override void OnClickedCell()
            {
                base.OnClickedCell();
                Game.popsToShowInPopulationPanel = DoSorting(Game.popsToShowInPopulationPanel, x => x.loyalty.get());                
                getParent().Refresh();
            }         
        }
        private class UnemploymentOrder : SortOrder
        {
            public UnemploymentOrder(UITableNew parent) : base(parent) { }
            public override void OnClickedCell()
            {
                base.OnClickedCell();
                Game.popsToShowInPopulationPanel = DoSorting(Game.popsToShowInPopulationPanel, x => x.getUnemployedProcent().get());
                getParent().Refresh();
            }
        }
    }
}