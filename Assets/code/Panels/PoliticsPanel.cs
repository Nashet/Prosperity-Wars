using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
public class PoliticsPanel : DragPanel
{   
    public ScrollRect table;
    public Text descriptionText;
    public Button voteButton;
    public Button forceDecisionButton;
    public Dropdown dropDown;
    public AbstractReform selectedReform;
    public AbstractReformValue selectedReformValue;
    List<AbstractReformValue> assotiateTable = new List<AbstractReformValue>();
   
    
    // Use this for initialization
    void Start()
    {
        MainCamera.politicsPanel = this;
        voteButton.interactable = false;
        dropDown.interactable = false;
        forceDecisionButton.interactable = false;
        hide();
    }
    //public void hide()
    //{
    //    politicsPanel.SetActive(false);
    //    //todo add button removal?      
    //}
    public void show(bool bringOnTop)
    {
        gameObject.SetActive(true);
        if (bringOnTop)
            panelRectTransform.SetAsLastSibling();

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
        //uint votersSayedYes;
        foreach (Province pro in Game.Player.ownedProvinces)
            foreach (PopUnit pop in pro.allPopUnits)
            {
                if (pop.canVote() && !pop.getSayingYes(selectedReformValue))
                {
                    //votersSayedYes = pop.getSayingYes(selectedReformValue);
                    //if (pop.getSayYesProcent(selectedReformValue) < Options.votingPassBillLimit)
                    pop.addDaysUpsetByForcedReform(Options.PopDaysUpsetByForcedReform);
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
            selectedReformValue = assotiateTable[dropDown.value];
            refresh(false);
        }
    }
    void rebuildDropDown()
    {
        dropDown.interactable = true;
        dropDown.ClearOptions();
        byte count = 0;
        assotiateTable.Clear();
        foreach (AbstractReformValue next in selectedReform)
        {
            //if (next.isAvailable(Game.player))
            {
                dropDown.options.Add(new Dropdown.OptionData() { text = next.ToString() });
                assotiateTable.Add(next);
                if (next == selectedReform.getValue())
                {
                    //selectedReformValue = next;
                    // selecting non empty option
                    dropDown.value = count;
                    dropDown.RefreshShownValue();
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

            descriptionText.text = selectedReform + " reforms " + selectedReform.getDescription()
           + "\n\nCurrently: " + selectedReform.getValue() + " " + selectedReform.getValue().getDescription()
           + "\n\nSelected: ";

            //if (selectedReformValue != null)
            if (selectedReformValue != selectedReform.getValue())
                descriptionText.text += selectedReformValue + " " + selectedReformValue.getDescription();
            else
                descriptionText.text += "current";     

           
            ////
            Procent procentPopulationSayedYes = new Procent(0f);
            Procent procentVotersSayedYes = Game.Player.getYesVotes(selectedReformValue, ref procentPopulationSayedYes);

            Dictionary<PopType, int> divisionPopulationResult = new Dictionary<PopType, int>();
            Dictionary<PopType, int> divisionVotersResult = Game.Player.getYesVotesByType(selectedReformValue, ref divisionPopulationResult);

            if (selectedReformValue != selectedReform.getValue())
            {
                if (Game.Player.government.status != Government.Despotism)
                {
                    descriptionText.text += "\n\n" + procentVotersSayedYes + " of voters want this reform ( ";
                    foreach (PopType type in PopType.getAllPopTypes())
                        if (divisionVotersResult[type] > 0)
                        {
                            Procent res = new Procent(divisionVotersResult[type] / (float)Game.Player.getPopulationAmountByType(type));
                            descriptionText.text += res + " of " + type + "; ";
                        }
                    descriptionText.text += ")";
                }
                else
                    descriptionText.text += "\n\nNobody to vote - Despot rule everything";
                descriptionText.text += "\n" + procentPopulationSayedYes + " of population want this reform ( ";
                foreach (PopType type in PopType.getAllPopTypes())
                    if (divisionPopulationResult[type] > 0)
                    {
                        Procent res = new Procent(divisionPopulationResult[type] / (float)Game.Player.getPopulationAmountByType(type));
                        descriptionText.text += res + " of " + type + "; ";
                    }
                descriptionText.text += ")";
            }


            if (selectedReformValue != null && selectedReformValue != selectedReform.getValue())
            {
                if (procentVotersSayedYes.get() >= Options.votingPassBillLimit || Game.Player.government.status == Government.Despotism)
                { // has enough voters
                    voteButton.interactable = selectedReformValue.allowed.isAllTrue(Game.Player, out voteButton.GetComponentInChildren<ToolTipHandler>().tooltip);
                    forceDecisionButton.GetComponentInChildren<ToolTipHandler>().tooltip = voteButton.GetComponentInChildren<ToolTipHandler>().tooltip;
                    forceDecisionButton.interactable = false;
                    voteButton.GetComponentInChildren<Text>().text = "Vote for " + selectedReformValue;
                }
                else // not enough voters
                {
                    voteButton.interactable = false;
                    forceDecisionButton.interactable = selectedReformValue.allowed.isAllTrue(Game.Player, out forceDecisionButton.GetComponentInChildren<ToolTipHandler>().tooltip);
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
        } //didn't selected reform
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
