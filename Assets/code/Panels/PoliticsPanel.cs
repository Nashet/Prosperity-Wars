using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
public class PoliticsPanel : DragPanel
{
    public GameObject politicsPanel;
    public ScrollRect table;
    public Text descriptionText;
    public Button voteButton;
    public Button forceDecisionButton;
    public Dropdown choise;
    public AbstractReform selectedReform;
    public AbstractReformValue selectedReformValue;
    List<AbstractReformValue> assotiateTable = new List<AbstractReformValue>();
    Procent procentVotersSayedYes = new Procent(0);
    //Province province;
    // Use this for initialization
    void Start()
    {
        MainCamera.politicsPanel = this;
        voteButton.interactable = false;
        choise.interactable = false;
        forceDecisionButton.interactable = false;
        hide();
    }
    public void hide()
    {
        politicsPanel.SetActive(false);
        //todo add button removal?      
    }
    public void show(bool bringOnTop)
    {
        politicsPanel.SetActive(true);
        if (bringOnTop)
            panelRectTransform.SetAsLastSibling();

    }
    public void onCloseClick()
    {
        hide();
    }

    void setNewReform()
    {
        if (selectedReform != null && selectedReformValue != null && selectedReformValue != selectedReform.getValue())
        {
            selectedReform.setValue(selectedReformValue);
            refresh(true);
            if (MainCamera.buildPanel.isActiveAndEnabled) MainCamera.buildPanel.refresh();
            if (MainCamera.populationPanel.isActiveAndEnabled) MainCamera.populationPanel.refresh();
            if (MainCamera.factoryPanel.isActiveAndEnabled) MainCamera.factoryPanel.refresh();

        }
    }
    public void onVoteClick()
    {
        setNewReform();
    }
    public void onForceDecisionClick()
    {

        //write-in here POp's ipset for forcing wrong reform 
        uint votersSayedYes;
        foreach (Province pro in Game.player.ownedProvinces)
            foreach (PopUnit pop in pro.allPopUnits)
            {
                //if (pop.canVote())
                {
                    votersSayedYes = pop.getSayingYesPopulation(selectedReformValue);
                    if (pop.getSayYesProcent(selectedReformValue) < Game.votingPassBillLimit)
                        pop.addDaysUpsetByForcedReform(Game.PopDaysUpsetByForcedReform);
                }
            }
        setNewReform();
    }
    //slider.onValueChanged.AddListener(ListenerMethod);

    public void onChoiceValueChanged()
    {
        ////if current reform does not contain reform value
        //bool contain = false;
        //foreach (AbstractReformValue reformValue in selectedReform)
        //{
        //    if (reformValue == selectedReformValue) contain = true;
        //}
        //if (!contain)
        {
            //selectedReformValue = selectedReform.getValue(assotiateTable[choise.value]);
            selectedReformValue = assotiateTable[choise.value];
            refresh(false);
        }
    }
    void rebuildDropDown()
    {
        choise.interactable = true;
        choise.ClearOptions();
        byte count = 0;
        assotiateTable.Clear();
        foreach (AbstractReformValue next in selectedReform)
        {
            //if (next.isAvailable(Game.player))
            {
                choise.options.Add(new Dropdown.OptionData() { text = next.ToString() });
                assotiateTable.Add(next);
                if (next == selectedReform.getValue())
                {
                    //selectedReformValue = next;
                    // selecting non empty option
                    choise.value = count;
                    choise.RefreshShownValue();
                }
                count++;
            }
        }
        onChoiceValueChanged(); // need it to set correct caption in DropDown
    }
    public void refresh(bool callRebuildDropDown)
    {
        hide();
        if (selectedReform != null)
        {
            if (callRebuildDropDown) // meaning changed whole reform            
                rebuildDropDown();

            descriptionText.text = selectedReform + " reforms - " + selectedReform.getDescription()
           + "\n\nCurrently: " + selectedReform.getValue() + " - " + selectedReform.getValue().getDescription()
           + "\n\nSelected: ";

            //if (selectedReformValue != null)
            if (selectedReformValue != selectedReform.getValue())
                descriptionText.text += selectedReformValue + " - " + selectedReformValue.getDescription();
            else
                descriptionText.text += "current";

            // calc how much of population wants selected reform
            uint totalPopulation = Game.player.getMenPopulation();
            uint votingPopulation = 0;
            uint populationSayedYes = 0;
            uint votersSayedYes = 0;
            Procent procentPopulationSayedYes = new Procent(0f);
            foreach (Province pro in Game.player.ownedProvinces)
                foreach (PopUnit pop in pro.allPopUnits)
                {
                    populationSayedYes += pop.getSayingYesPopulation(selectedReformValue);
                    if (pop.canVote())
                    {
                        votersSayedYes += pop.getSayingYesPopulation(selectedReformValue);
                        votingPopulation += pop.population;
                    }
                }
            if (totalPopulation != 0)
                procentPopulationSayedYes.set((float)populationSayedYes / totalPopulation);
            else
                procentPopulationSayedYes.set(0);

            if (votingPopulation == 0)
                procentVotersSayedYes.set(0);
            else
                procentVotersSayedYes.set((float)votersSayedYes / votingPopulation);

            // division by pop types
            Dictionary<PopType, uint> divisionVotersResult = new Dictionary<PopType, uint>();
            foreach (PopType type in PopType.allPopTypes)
            {
                divisionVotersResult.Add(type, 0);
                foreach (Province pro in Game.player.ownedProvinces)
                {
                    var popList = pro.FindAllPopUnits(type);
                    foreach (PopUnit pop in popList)
                        if (pop.canVote())
                            divisionVotersResult[type] += pop.getSayingYesPopulation(selectedReformValue);

                }
            }

            Dictionary<PopType, uint> divisionPopulationResult = new Dictionary<PopType, uint>();
            foreach (PopType type in PopType.allPopTypes)
            {
                divisionPopulationResult.Add(type, 0);
                foreach (Province pro in Game.player.ownedProvinces)
                {
                    var popList = pro.FindAllPopUnits(type);
                    foreach (PopUnit pop in popList)
                        divisionPopulationResult[type] += pop.getSayingYesPopulation(selectedReformValue);
                }
            }
            ////
            if (selectedReformValue != selectedReform.getValue())
            {
                if (Game.player.government.status != Government.Despotism)
                {
                    descriptionText.text += "\n\n" + procentVotersSayedYes + " of voters want this reform ( ";
                    foreach (PopType type in PopType.allPopTypes)
                        if (divisionVotersResult[type] > 0)
                        {
                            Procent res = new Procent(divisionVotersResult[type] / (float)Game.player.FindPopulationAmountByType(type));
                            descriptionText.text += res + " of " + type + " ";
                        }
                    descriptionText.text += ")";
                }
                else
                    descriptionText.text += "\n\nNobody to vote - Despot rule everything";
                descriptionText.text += "\n" + procentPopulationSayedYes + " of population want this reform ( ";
                foreach (PopType type in PopType.allPopTypes)
                    if (divisionPopulationResult[type] > 0)
                    {
                        Procent res = new Procent(divisionPopulationResult[type] / (float)Game.player.FindPopulationAmountByType(type));
                        descriptionText.text += res + " of " + type + " ";
                    }
                descriptionText.text += ")";
            }


            if (selectedReformValue != null && selectedReformValue != selectedReform.getValue())
            {
                if (procentVotersSayedYes.get() >= Game.votingPassBillLimit || Game.player.government.status == Government.Despotism)
                { // has enough voters
                    voteButton.interactable = selectedReformValue.condition.isAllTrue(Game.player, out voteButton.GetComponentInChildren<ToolTipHandler>().tooltip);
                    forceDecisionButton.GetComponentInChildren<ToolTipHandler>().tooltip = voteButton.GetComponentInChildren<ToolTipHandler>().tooltip;
                    forceDecisionButton.interactable = false;
                    voteButton.GetComponentInChildren<Text>().text = "Vote " + selectedReformValue;
                }
                else // not enough voters
                {
                    voteButton.interactable = false;
                    forceDecisionButton.interactable = selectedReformValue.condition.isAllTrue(Game.player, out forceDecisionButton.GetComponentInChildren<ToolTipHandler>().tooltip);
                    voteButton.GetComponentInChildren<ToolTipHandler>().tooltip = forceDecisionButton.GetComponentInChildren<ToolTipHandler>().tooltip;
                    voteButton.GetComponentInChildren<Text>().text = "Not enough votes";
                }
            }
            else // this reform already enacted
            {
                voteButton.interactable = false;
                forceDecisionButton.interactable = false;
                forceDecisionButton.GetComponentInChildren<ToolTipHandler>().tooltip = "";
                voteButton.GetComponentInChildren<ToolTipHandler>().tooltip = "";
            }
        } //didnt selected reform
        else
        {
            voteButton.interactable = false;
            voteButton.GetComponentInChildren<Text>().text = "Select reform";
            descriptionText.text = "Select reform from left";
            forceDecisionButton.GetComponentInChildren<ToolTipHandler>().tooltip = "";
            voteButton.GetComponentInChildren<ToolTipHandler>().tooltip = "";
        }

        show(false);
    }
    // Update is called once per frame
    //   void Update () {

    //}
}
