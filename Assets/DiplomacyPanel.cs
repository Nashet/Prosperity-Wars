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
        sb.Append("\nArmy: ").Append(selectedCountry.homeArmy.getShortName());
        generalText.text = sb.ToString();
    }
    public void show(Country count)
    {
        gameObject.SetActive(true);
        panelRectTransform.SetAsLastSibling();
        selectedCountry = count;
        refresh();
    }   
}
