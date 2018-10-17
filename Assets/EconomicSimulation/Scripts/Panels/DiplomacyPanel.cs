using Nashet.EconomicSimulation;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using System;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Nashet.UISystem
{
    class DiplomacyPanel : DragPanel
    {
        [SerializeField]
        protected Text captionText, generalText, property;

        [SerializeField]
        protected Button giveControlToAi, giveControlToPlayer, declareWar;

        // [SerializeField]
        //private MainCamera mainCamera;

        [SerializeField]
        protected RawImage flag;

        protected Country selectedCountry;
        protected StringBuilder sb = new StringBuilder();

        // Use this for initialization
        protected void Start()
        {
            //MainCamera.diplomacyPanel = this;
            GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 45);
            Hide();
            //Game.Player.events.WantedToSeeDiplomacy += WantedToSeeDiplomacy;
        }

        // Update is called once per frame
        protected void Update()
        {
            //if (Game.Player != null)
            //    Game.Player.events.WantedToSeeDiplomacy += WantedToSeeDiplomacy;
            //refresh();
        }

        public override void Refresh()
        {
            setButtonsState();
            sb.Clear();
            sb.Append("Diplomacy of ").Append(selectedCountry);
            captionText.text = sb.ToString();

            sb.Clear();
            sb.Append("Population: ").Append(selectedCountry.Provinces.getFamilyPopulation().ToString("N0")).Append("; rank: ").Append(selectedCountry.getPopulationRank());
            sb.Append(". Provinces: ").Append(selectedCountry.Provinces.Count).Append("; rank: ").Append(selectedCountry.getSizeRank());
            //sb.Append(", str: ").Append(selectedCountry.getStregth(null));
            //.Get().ToString("N3")
            //.ToString("F3")
            sb.Append("\n\nGDP: ").Append(selectedCountry.getGDP()).Append("; rank: ").Append(selectedCountry.getGDPRank()).Append("; world share: ").Append(selectedCountry.getGDPShare());
            sb.Append("\n\nGDP per thousand men: ").Append(selectedCountry.getGDPPer1000()).Append("; rank: ").Append(selectedCountry.getGDPPer1000Rank());
            //sb.Append("\nAverage needs fulfilling: ").Append(selectedCountry.GetAveragePop(x=>x.needsFulfilled));
            sb.Append("\n\nPops average needs fulfilling: ").Append(selectedCountry.Provinces.AllPops.GetAverageProcent(x => x.needsFulfilled));
            sb.Append(", loyalty: ").Append(selectedCountry.Provinces.AllPops.GetAverageProcent(x => x.loyalty));
            sb.Append(", education: ").Append(selectedCountry.Provinces.AllPops.GetAverageProcent(x => x.Education));
            sb.Append("\n\nReforms: ").Append(selectedCountry.government).Append("; ")
                .Append(selectedCountry.economy).Append("; ")
                .Append(selectedCountry.minorityPolicy);
            sb.AppendFormat("; {0}", selectedCountry.unemploymentSubsidies);
            sb.AppendFormat("; {0}", selectedCountry.UBI);
            sb.AppendFormat("; {0}", selectedCountry.PovertyAid);
            sb.AppendFormat("; {0}", selectedCountry.minimalWage);
            sb.AppendFormat("; {0}", selectedCountry.taxationForPoor);
            sb.AppendFormat("; {0}", selectedCountry.taxationForRich);

            sb.Append("\n\nState culture: ").Append(selectedCountry.Culture);
            sb.Append("\nCultures: ").Append(selectedCountry.Provinces.AllPops.Group(x => x.culture, y => y.population.Get())
                .OrderByDescending(x => x.Value.get()).ToString(", ", 5));
            sb.Append("\nClasses: ").Append(selectedCountry.Provinces.AllPops.Group(x => x.Type, y => y.population.Get())
                .OrderByDescending(x => x.Value.get()).ToString(", ", 0));
            if (Game.devMode)
                sb.Append("\n\nArmy: ").Append(selectedCountry.getDefenceForces());

            if (selectedCountry == Game.Player)
                sb.Append("\n\nOpinion of myself: I'm cool!");
            else
            {
                sb.Append("\n\n").Append(selectedCountry.FullName).Append("'s opinion of us: ").Append(selectedCountry.Diplomacy.GetRelationTo(Game.Player));
                string str;
                selectedCountry.modMyOpinionOfXCountry.getModifier(Game.Player, out str);
                sb.Append(" Dynamics: ").Append(str);
            }
            //sb.Append("\nInventions: ").Append(selectedCountry.inventions.getInvented(selectedCountry).ToString());
            //selectedCountry.inventions.getInvented(selectedCountry).ToString();
            generalText.text = sb.ToString();
            var found = World.GetAllShares(selectedCountry).OrderByDescending(x => x.Value.get());
            property.GetComponent<ToolTipHandler>().SetTextDynamic(() => "Owns:\n" + found.ToString(", ", "\n"));
        }

        protected void show(Country country)
        {
            selectedCountry = country;
            Show();
            flag.texture = country.Flag;
        }

        protected void setButtonsState()
        {
            giveControlToPlayer.interactable = selectedCountry.isAI();
            giveControlToAi.interactable = !selectedCountry.isAI();

            declareWar.interactable = !Diplomacy.IsInWar(Game.Player, this.selectedCountry);
        }

        public void onSurrenderClick()
        {
            Game.GivePlayerControlToAI();
            setButtonsState();
        }

        public void OnDeclareWar()
        {
            if (Game.Player != this.selectedCountry && !Diplomacy.IsInWar(Game.Player, this.selectedCountry))
            {
                Diplomacy.DeclareWar(Game.Player, this.selectedCountry);
                Refresh();
            }
        }

        public void onGoToClick()
        {
            //if (selectedCountry != World.UncolonizedLand)
            //mainCamera.FocusOnProvince(selectedCountry.Capital, true);
        }

        public void onRegainControlClick()
        {
            Game.GivePlayerControlOf(selectedCountry);
            setButtonsState();
        }

        //todo Instance
        protected static DiplomacyPanel Instance;
        protected void Awake()
        {
            base.Awake();
            Instance = this;
        }

        public static void WantedToSeeDiplomacyHandler(object sender, EventArgs e)
        {
            var isCountryArguments = e as CountryEventArgs;
            if (isCountryArguments != null)
            {
                if (Instance.isActiveAndEnabled)
                {
                    if (Instance.selectedCountry == isCountryArguments.Country)
                        Instance.Hide();
                    else
                        Instance.show(isCountryArguments.Country);
                }
                else
                    Instance.show(isCountryArguments.Country);
            }
            //if (MainCamera.diplomacyPanel.isActiveAndEnabled)
            //{
            //    if (MainCamera.diplomacyPanel.getSelectedCountry() == Game.selectedProvince.Country)

            //        MainCamera.diplomacyPanel.Hide();
            //    else
            //        MainCamera.diplomacyPanel.show(Game.selectedProvince.Country);
            //}
            //else
            //    MainCamera.diplomacyPanel.show(Game.selectedProvince.Country);

            //if (MainCamera.diplomacyPanel.isActiveAndEnabled)
            //{
            //    if (MainCamera.diplomacyPanel.getSelectedCountry() == this)
            //        MainCamera.diplomacyPanel.Hide();
            //    else
            //        MainCamera.diplomacyPanel.show(this);
            //}
            //else
            //    MainCamera.diplomacyPanel.show(this);
        }
    }
}