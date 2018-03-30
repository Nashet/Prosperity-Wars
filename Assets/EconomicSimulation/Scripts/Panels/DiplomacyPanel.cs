﻿using System.Linq;
using System.Text;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Nashet.EconomicSimulation
{
    public class DiplomacyPanel : DragPanel
    {
        [SerializeField]
        private Text captionText, generalText, property;

        [SerializeField]
        private Button giveControlToAi, giveControlToPlayer;

        [SerializeField]
        private MainCamera mainCamera;

        private Country selectedCountry;
        private StringBuilder sb = new StringBuilder();

        // Use this for initialization
        private void Start()
        {
            MainCamera.diplomacyPanel = this;
            GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, MainCamera.topPanel.GetComponent<RectTransform>().rect.height * -1f);
            Hide();
        }

        // Update is called once per frame
        private void Update()
        {
            //refresh();
        }

        public override void Refresh()
        {
            setButtonsState();
            sb.Clear();
            sb.Append("Diplomacy of ").Append(selectedCountry);
            captionText.text = sb.ToString();

            sb.Clear();
            sb.Append("Population: ").Append(selectedCountry.getFamilyPopulation().ToString("N0")).Append("; rank: ").Append(selectedCountry.getPopulationRank());
            sb.Append(". Provinces: ").Append(selectedCountry.getSize()).Append("; rank: ").Append(selectedCountry.getSizeRank());
            //sb.Append(", str: ").Append(selectedCountry.getStregth(null));
            //.Get().ToString("N3")
            //.ToString("F3")
            sb.Append("\n\nGDP: ").Append(selectedCountry.getGDP()).Append("; rank: ").Append(selectedCountry.getGDPRank()).Append("; world share: ").Append(selectedCountry.getGDPShare());
            sb.Append("\n\nGDP per thousand men: ").Append(selectedCountry.getGDPPer1000()).Append("; rank: ").Append(selectedCountry.getGDPPer1000Rank());
            //sb.Append("\nAverage needs fulfilling: ").Append(selectedCountry.GetAveragePop(x=>x.needsFulfilled));
            sb.Append("\n\nPops average needs fulfilling: ").Append(selectedCountry.GetAllPopulation().GetAverageProcent(x => x.needsFulfilled));
            sb.Append(", loyalty: ").Append(selectedCountry.GetAllPopulation().GetAverageProcent(x => x.loyalty));
            sb.Append(", education: ").Append(selectedCountry.GetAllPopulation().GetAverageProcent(x => x.Education));
            sb.Append("\n\nReforms: ").Append(selectedCountry.government.getValue()).Append("; ").Append(selectedCountry.economy.getValue()).Append("; ").Append(selectedCountry.minorityPolicy.getValue());
            sb.AppendFormat("; {0}", selectedCountry.unemploymentSubsidies.getValue());
            sb.AppendFormat("; {0}", selectedCountry.minimalWage.getValue());
            sb.AppendFormat("; {0}", selectedCountry.taxationForPoor.getValue());
            sb.AppendFormat("; {0}", selectedCountry.taxationForRich.getValue());
            sb.Append("\n\nState culture: ").Append(selectedCountry.getCulture());
            sb.Append("\nCultures:\n\t").Append(selectedCountry.getCultures().getString("\n\t", 5));
            if (Game.devMode)
                sb.Append("\n\nArmy: ").Append(selectedCountry.getDefenceForces().getName());

            if (selectedCountry == Game.Player)
                sb.Append("\n\nOpinion of myself: I'm cool!");
            else
            {
                sb.Append("\n\n").Append(selectedCountry.FullName).Append("'s opinion of us: ").Append(selectedCountry.getRelationTo(Game.Player));
                string str;
                selectedCountry.modMyOpinionOfXCountry.getModifier(Game.Player, out str);
                sb.Append(" Dynamics: ").Append(str);
            }
            //sb.Append("\nInventions: ").Append(selectedCountry.inventions.getInvented(selectedCountry).ToString());
            //selectedCountry.inventions.getInvented(selectedCountry).ToString();
            generalText.text = sb.ToString();
            var found = World.GetAllShares(selectedCountry).OrderByDescending(x => x.Value.get());
            property.GetComponent<ToolTipHandler>().SetTextDynamic(() => "Owns:\n" + found.getString(", ", "\n"));
        }

        public Country getSelectedCountry()
        {
            return selectedCountry;
        }

        public void show(Country count)
        {
            selectedCountry = count;
            Show();
        }

        private void setButtonsState()
        {
            giveControlToPlayer.interactable = selectedCountry.isAI();
            giveControlToAi.interactable = !selectedCountry.isAI();
        }

        public void onSurrenderClick()
        {
            Game.GivePlayerControlToAI();
            setButtonsState();
        }

        public void onGoToClick()
        {
            if (selectedCountry != World.UncolonizedLand)
                mainCamera.FocusOnProvince(selectedCountry.Capital, true);
        }

        public void onRegainControlClick()
        {
            Game.GivePlayerControlOf(selectedCountry);
            setButtonsState();
        }
    }
}