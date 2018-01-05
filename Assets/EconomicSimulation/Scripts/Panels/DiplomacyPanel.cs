using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;
using Nashet.UnityUIUtils;
using Nashet.Utils;

namespace Nashet.EconomicSimulation
{
    public class DiplomacyPanel : DragPanel
    {
        [SerializeField]
        private Text captionText, generalText;
        [SerializeField]
        private Button giveControlToAi, giveControlToPlayer;
        [SerializeField]
        private MainCamera mainCamera;
        private Country selectedCountry;
        StringBuilder sb = new StringBuilder();
        // Use this for initialization
        void Start()
        {
            MainCamera.diplomacyPanel = this;
            GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, MainCamera.topPanel.GetComponent<RectTransform>().rect.height * -1f);
            Hide();
        }
        // Update is called once per frame
        void Update()
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

            sb.Append("\nGDP: ").Append(selectedCountry.getGDP().get().ToString("N3")).Append("; rank: ").Append(selectedCountry.getGDPRank()).Append("; world share: ").Append(selectedCountry.getGDPShare());
            sb.Append("\nGDP per thousand men: ").Append(selectedCountry.getGDPPer1000().ToString("F3")).Append("; rank: ").Append(selectedCountry.getGDPPer1000Rank());
            sb.Append("\nAverage needs fulfilling: ").Append(selectedCountry.getAverageNeedsFulfilling());
            sb.Append("\nReforms: ").Append(selectedCountry.government.getValue()).Append("; ").Append(selectedCountry.economy.getValue()).Append("; ").Append(selectedCountry.minorityPolicy.getValue());
            sb.AppendFormat("; {0}", selectedCountry.unemploymentSubsidies.getValue());
            sb.AppendFormat("; {0}", selectedCountry.minimalWage.getValue());
            sb.AppendFormat("; {0}", selectedCountry.taxationForPoor.getValue());
            sb.AppendFormat("; {0}", selectedCountry.taxationForRich.getValue());
            sb.Append("\nState culture: ").Append(selectedCountry.getCulture());
            sb.Append("\nCultures:\n\t").Append(selectedCountry.getCultures().getString("\n\t", 5));
            sb.Append("\n\nArmy: ").Append(selectedCountry.getDefenceForces().getName());
            if (selectedCountry == Game.Player)
                sb.Append("\n\nOpinion of myself: I'm cool!");
            else
            {
                sb.Append("\n\n").Append(selectedCountry.getDescription()).Append("'s opinion of us: ").Append(selectedCountry.getRelationTo(Game.Player));
                string str;
                selectedCountry.modMyOpinionOfXCountry.getModifier(Game.Player, out str);
                sb.Append(" Dynamics: ").Append(str);
            }
            //sb.Append("\nInventions: ").Append(selectedCountry.inventions.getInvented(selectedCountry).ToString());
            //selectedCountry.inventions.getInvented(selectedCountry).ToString();
            generalText.text = sb.ToString();
        }
        public Country getSelectedCountry()
        {
            return selectedCountry;
        }
        public void show(Country count)
        {
            gameObject.SetActive(true);
            panelRectTransform.SetAsLastSibling();
            selectedCountry = count;
            Refresh();
        }
        private void setButtonsState()
        {
            giveControlToPlayer.interactable = selectedCountry.isAI();
            giveControlToAi.interactable = !selectedCountry.isAI();
        }
        public void onSurrenderClick()
        {
            Game.givePlayerControlToAI();
            setButtonsState();
        }
        public void onGoToClick()
        {
            if (selectedCountry != Country.NullCountry)
                mainCamera.focusCamera(selectedCountry.getCapital());
        }
        public void onRegainControlClick()
        {
            Game.takePlayerControlOfThatCountry(selectedCountry);
            setButtonsState();
        }
    }
}