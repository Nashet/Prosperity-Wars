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

        private Country _selectedCountry;
        protected Country SelectedCountry {
            get { return _selectedCountry; }
            set {
                _selectedCountry = value;
                flag.texture = _selectedCountry.Flag;
            }
        }
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
            sb.Append("Diplomacy of ").Append(SelectedCountry);
            captionText.text = sb.ToString();

            sb.Clear();
            sb.Append("Population: ").Append(SelectedCountry.Provinces.getFamilyPopulation().ToString("N0")).Append("; rank: ").Append(SelectedCountry.getPopulationRank());
            sb.Append(". Provinces: ").Append(SelectedCountry.Provinces.Count).Append("; rank: ").Append(SelectedCountry.getSizeRank());
            //sb.Append(", str: ").Append(selectedCountry.getStregth(null));
            //.Get().ToString("N3")
            //.ToString("F3")
            sb.Append("\n\nGDP: ").Append(SelectedCountry.getGDP()).Append("; rank: ").Append(SelectedCountry.getGDPRank()).Append("; world share: ").Append(SelectedCountry.getGDPShare());
            sb.Append("\n\nGDP per thousand men: ").Append(SelectedCountry.getGDPPer1000()).Append("; rank: ").Append(SelectedCountry.getGDPPer1000Rank());
            //sb.Append("\nAverage needs fulfilling: ").Append(selectedCountry.GetAveragePop(x=>x.needsFulfilled));
            sb.Append("\n\nPops average needs fulfilling: ").Append(SelectedCountry.Provinces.AllPops.GetAverageProcent(x => x.needsFulfilled));
            sb.Append(", loyalty: ").Append(SelectedCountry.Provinces.AllPops.GetAverageProcent(x => x.loyalty));
            sb.Append(", education: ").Append(SelectedCountry.Provinces.AllPops.GetAverageProcent(x => x.Education));
            sb.Append("\n\nReforms: ").Append(SelectedCountry.government).Append("; ")
                .Append(SelectedCountry.economy).Append("; ")
                .Append(SelectedCountry.minorityPolicy);
            sb.AppendFormat("; {0}", SelectedCountry.unemploymentSubsidies);
            sb.AppendFormat("; {0}", SelectedCountry.UBI);
            sb.AppendFormat("; {0}", SelectedCountry.PovertyAid);
            sb.AppendFormat("; {0}", SelectedCountry.minimalWage);
            sb.AppendFormat("; {0}", SelectedCountry.taxationForPoor);
            sb.AppendFormat("; {0}", SelectedCountry.taxationForRich);

            sb.Append("\n\nState culture: ").Append(SelectedCountry.Culture);
            sb.Append("\nCultures: ").Append(SelectedCountry.Provinces.AllPops.Group(x => x.culture, y => y.population.Get())
                .OrderByDescending(x => x.Value.get()).ToString(", ", 5));
            sb.Append("\nClasses: ").Append(SelectedCountry.Provinces.AllPops.Group(x => x.Type, y => y.population.Get())
                .OrderByDescending(x => x.Value.get()).ToString(", ", 0));
            if (Game.devMode)
                sb.Append("\n\nArmy: ").Append(SelectedCountry.getDefenceForces());

            if (SelectedCountry == Game.Player)
                sb.Append("\n\nOpinion of myself: I'm cool!");
            else
            {
                sb.Append("\n\n").Append(SelectedCountry.FullName).Append("'s opinion of us: ").Append(SelectedCountry.Diplomacy.GetRelationTo(Game.Player));
                string str;
                SelectedCountry.modMyOpinionOfXCountry.getModifier(Game.Player, out str);
                sb.Append(" Dynamics: ").Append(str);
            }
            //sb.Append("\nInventions: ").Append(selectedCountry.inventions.getInvented(selectedCountry).ToString());
            //selectedCountry.inventions.getInvented(selectedCountry).ToString();
            generalText.text = sb.ToString();
            var found = World.GetAllShares(SelectedCountry).OrderByDescending(x => x.Value.get());
            property.GetComponent<ToolTipHandler>().SetTextDynamic(() => "Owns:\n" + found.ToString(", ", "\n"));
        }
        
        protected void setButtonsState()
        {
            giveControlToPlayer.interactable = SelectedCountry.isAI();
            giveControlToAi.interactable = !SelectedCountry.isAI();

            declareWar.interactable = !Diplomacy.IsInWar(Game.Player, this.SelectedCountry);
        }

        public void onSurrenderClick()
        {
            Game.GivePlayerControlToAI();
            setButtonsState();
        }

        public void OnDeclareWar()
        {
            if (Game.Player != this.SelectedCountry && !Diplomacy.IsInWar(Game.Player, this.SelectedCountry))
            {
                Diplomacy.DeclareWar(Game.Player, this.SelectedCountry);
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
            Game.GivePlayerControlOf(SelectedCountry);
            setButtonsState();
        }

        //todo temporal? Instance
        protected static DiplomacyPanel Instance;

        new protected void Awake()
        {
            base.Awake();
            Instance = this;
            UIEvents.ClickedOn += OnClickedOn;
        }

        protected void OnClickedOn(object sender, EventArgs e)
        {
            var isCountryArguments = e as CountryEventArgs;
            if (isCountryArguments != null)
            {
                if (Instance.isActiveAndEnabled && Instance.SelectedCountry == isCountryArguments.NewCountry)
                    Instance.Hide();
                else
                {
                    SelectedCountry = isCountryArguments.NewCountry;
                    Instance.Show();
                }
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