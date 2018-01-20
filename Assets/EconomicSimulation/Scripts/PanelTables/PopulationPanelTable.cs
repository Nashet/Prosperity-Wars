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
    public class PopulationPanelTable : UITableNew<PopUnit>
    {
        private SortOrder needsFulfillmentOrder, unemploymentOrder, loyaltyOrder, populationOrder, cashOrder,
        movementOrder, provinceOrder, cultureOrder;

        private void Start()
        {
            needsFulfillmentOrder = new SortOrder(this, x => x.needsFullfilled.get());
            unemploymentOrder = new SortOrder(this, x => x.getUnemployedProcent().get());
            loyaltyOrder = new SortOrder(this, x => x.loyalty.get());
            populationOrder = new SortOrder(this, x => x.getPopulation());
            cashOrder = new SortOrder(this, x => x.getCash());

            
            provinceOrder = new SortOrder(this, x => x.getProvince().getSortRank());
            cultureOrder = new SortOrder(this, x => x.culture.getSortRank());
            movementOrder = new SortOrder(this, x =>
            {
                if (x.getMovement() == null)
                    return float.MinValue;
                else
                    return x.getMovement().GetHashCode();
            });
        }
        protected override IEnumerable<PopUnit> ContentSelector()
        {
            return Game.Player.getAllPopUnits();
            //var popsToShow = new List<PopUnit>();
            //foreach (Province province in Game.Player.ownedProvinces)
            //    foreach (PopUnit pop in province.allPopUnits)
            //        popsToShow.Add(pop);
            //return popsToShow;
        }
        protected override void AddRow(PopUnit pop, int number)
        {
            // Adding number
            //AddButton(Convert.ToString(i + offset), record);

            // Adding PopType
            AddCell(pop.popType.ToString(), pop);
            ////Adding province
            AddCell(pop.getProvince().ToString(), pop.getProvince(), () => "Click to select this province");
            ////Adding population
            AddCell(System.Convert.ToString(pop.getPopulation()), pop);
            ////Adding culture
            AddCell(pop.culture.ToString(), pop);

            ////Adding education
            AddCell(pop.education.ToString(), pop);

            ////Adding cash
            AddCell(pop.cash.ToString(), pop);

            ////Adding needs fulfilling

            //PopUnit ert = record;
            AddCell(pop.needsFullfilled.ToString(), pop,
                //() => ert.consumedTotal.ToStringWithLines()                        
                () => "Consumed:\n" + pop.getConsumed().getContainer().getString("\n")
                );

            ////Adding loyalty
            string accu;
            PopUnit.modifiersLoyaltyChange.getModifier(pop, out accu);
            AddCell(pop.loyalty.ToString(), pop, () => accu);

            //Adding Unemployment
            AddCell(pop.getUnemployedProcent().ToString(), pop);

            //Adding Movement
            if (pop.getMovement() == null)
                AddCell("", pop);
            else
                AddCell(pop.getMovement().getShortName(), pop, () => pop.getMovement().getName());
        }
        protected override void AddHeader()
        {
            // Adding number
            // AddButton("Number");

            // Adding PopType
            AddCell("Type");

            ////Adding province
            AddCell("Province"+ provinceOrder.getSymbol(), provinceOrder);

            ////Adding population
            AddCell("Population" + populationOrder.getSymbol(), populationOrder);

            ////Adding culture
            AddCell("Culture"+ cultureOrder.getSymbol(), cultureOrder);

            ////Adding education
            AddCell("Education");

            ////Adding storage
            //if (null.storage != null)
            AddCell("Cash" + cashOrder.getSymbol(), cashOrder);
            //else AddButton("Administration");

            ////Adding needs fulfilling
            AddCell("Needs fulfilled" + needsFulfillmentOrder.getSymbol(), needsFulfillmentOrder);

            ////Adding loyalty
            AddCell("Loyalty" + loyaltyOrder.getSymbol(), loyaltyOrder);

            ////Adding Unemployment
            AddCell("Unemployment" + unemploymentOrder.getSymbol(), unemploymentOrder);

            //Adding Movement
            AddCell("Movement" + movementOrder.getSymbol(), movementOrder);
        }
    }
}