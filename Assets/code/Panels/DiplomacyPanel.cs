using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;

public class DiplomacyPanel : DragPanel
{
    public Text  captionText, generalText;
    Country selectedCountry;
    StringBuilder sb = new StringBuilder();
    // Use this for initialization
    void Start()
    {
        MainCamera.diplomacyPanel = this;
        hide();
    }
    // Update is called once per frame
    void Update()
    {
        //refresh();
    }
    public void refresh()
    {
        sb.Clear();
        sb.Append("Diplomacy of ").Append(selectedCountry);
        captionText.text = sb.ToString();

        sb.Clear();
        sb.Append("Population: ").Append(selectedCountry.getFamilyPopulation());
        sb.Append("\nState culture: ").Append(selectedCountry.getCulture());        
        sb.Append("\nGDP: ").Append(selectedCountry.getGDP()).Append("; GDP per thousand men: ").Append(selectedCountry.getGDPPer1000());
        sb.Append("\nGovernment: ").Append(selectedCountry.government.getValue()).Append(", ").Append(selectedCountry.economy.getValue()).Append(", ").Append(selectedCountry.minorityPolicy.getValue());
        sb.Append("\n\nArmy: ").Append(selectedCountry.getDefenceForces().getShortName());
        if (selectedCountry== Game.Player)
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
        refresh();
    }
    public void onSurrenderClick()
    {
        Game.givePlayerControlToAI();
    }
    public void onRegainControlClick()
    {
        Game.takePlayerControlOfThatCountry(selectedCountry);
    }
}
