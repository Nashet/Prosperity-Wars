﻿using System;
using System.Collections.Generic;
using Nashet.UnityUIUtils;

namespace Nashet.EconomicSimulation
{
    public class PopulationPanelTable : UITableNew<PopUnit>
    {
        private SortOrder needsFulfillmentOrder, unemploymentOrder, loyaltyOrder, populationOrder, cashOrder,
        movementOrder, provinceOrder, cultureOrder, popTypeOrder, educationOrder;

        private void Start()
        {
            popTypeOrder = new SortOrder(this, x => x.Type.GetNameWeight());
            needsFulfillmentOrder = new SortOrder(this, x => x.needsFulfilled.get());
            unemploymentOrder = new SortOrder(this, x => x.getUnemployment().get());
            loyaltyOrder = new SortOrder(this, x => x.loyalty.get());
            populationOrder = new SortOrder(this, x => x.getPopulation());
            cashOrder = new SortOrder(this, x => (float)x.Cash.Get());

            educationOrder = new SortOrder(this, x => x.Education.get());

            provinceOrder = new SortOrder(this, x => x.Province.GetNameWeight());
            cultureOrder = new SortOrder(this, x => x.culture.GetNameWeight());
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
            var selectedProvince = MainCamera.populationPanel.SelectedProvince;
            if (selectedProvince == null)
                return Game.Player.GetAllPopulation();
            else
                return selectedProvince.GetAllPopulation();
        }

        protected override void AddRow(PopUnit pop, int number)
        {
            // Adding number
            //AddButton(Convert.ToString(i + offset), record);

            // Adding PopType
            AddCell(pop.ShortName, pop);
            ////Adding province
            AddCell(pop.Province.ToString(), pop.Province, () => "Click to select this province");
            ////Adding population
            AddCell(Convert.ToString(pop.getPopulation()), pop);
            ////Adding culture
            AddCell(pop.culture.ToString(), pop);

            ////Adding education
            AddCell(pop.Education.ToString(), pop);

            ////Adding cash
            AddCell(pop.Cash.ToString(), pop);

            ////Adding needs fulfilling

            AddCell(pop.needsFulfilled.ToString(), pop,
                //() => ert.consumedTotal.ToStringWithLines()
                () => "Consumed:\n" + pop.getConsumed().GetString("\n")
                );

            ////Adding loyalty
            string accu;
            PopUnit.modifiersLoyaltyChange.getModifier(pop, out accu);
            AddCell(pop.loyalty.ToString(), pop, () => accu);

            //Adding Unemployment
            AddCell(pop.getUnemployment().ToString(), pop);

            //Adding Movement
            if (pop.getMovement() == null)
                AddCell("", pop);
            else
                AddCell(pop.getMovement().ShortName, pop, () => pop.getMovement().ToString());
        }

        protected override void AddHeader()
        {
            // Adding number
            // AddButton("Number");

            // Adding PopType
            AddCell("Type" + popTypeOrder.getSymbol(), popTypeOrder);

            ////Adding province
            AddCell("Province" + provinceOrder.getSymbol(), provinceOrder);

            ////Adding population
            AddCell("Population" + populationOrder.getSymbol(), populationOrder);

            ////Adding culture
            AddCell("Culture" + cultureOrder.getSymbol(), cultureOrder);

            ////Adding education
            AddCell("Education" + educationOrder.getSymbol(), educationOrder);

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