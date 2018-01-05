using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using Nashet.UnityUIUtils;
using Nashet.ValueSpace;
namespace Nashet.EconomicSimulation
{
    public class PoliticsPanel : DragPanel
    {
        [SerializeField]
        private Text descriptionText, movementsText;
        //public ScrollRect movementsScrollView;

        [SerializeField]
        private Button voteButton;

        [SerializeField]
        private Button forceDecisionButton;

        [SerializeField]
        private Dropdown dropDown;

        [SerializeField]
        private Scrollbar movementsHorizontalScrollBar;

        [SerializeField]
        private AbstractReform selectedReform;

        [SerializeField]
        private AbstractReformValue selectedReformValue;

        private readonly List<AbstractReformValue> assotiateTable = new List<AbstractReformValue>();

        // Use this for initialization
        void Start()
        {
            MainCamera.politicsPanel = this;
            voteButton.interactable = false;
            dropDown.interactable = false;
            forceDecisionButton.interactable = false;
            GetComponent<RectTransform>().anchoredPosition = new Vector2(150f, -150f);
            hide();
        }

        public void show(bool bringOnTop)
        {
            gameObject.SetActive(true);
            if (bringOnTop)
                panelRectTransform.SetAsLastSibling();
            //refresh(true); - recursion
        }

        void setNewReform()
        {
            if (selectedReform != null && selectedReformValue != null && selectedReformValue != selectedReform.getValue())
            {
                selectedReform.setValue(selectedReformValue);
                refresh(true, null);
                if (MainCamera.buildPanel.isActiveAndEnabled) MainCamera.buildPanel.refresh(null);
                if (MainCamera.populationPanel.isActiveAndEnabled) MainCamera.populationPanel.refreshContent();
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
            selectedReformValue = assotiateTable[dropDown.value];
            refresh(false, null);
        }
        void rebuildDropDown()
        {
            //dropDown.Hide();        
            var toDestroy = dropDown.transform.Find("Dropdown List");
            if (toDestroy != null)
                Destroy(toDestroy.gameObject);
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
                    }
                    count++;
                }
            }

            onChoiceValueChanged(); // need it to set correct caption in DropDown
            dropDown.RefreshShownValue();
            //dropDown.Show();
        }
        public void refresh(bool callRebuildDropDown, AbstractReform newSelection)
        {
            if (newSelection != null)
                selectedReform = newSelection;
            hide();
            //if (Game.Player.movements != null)
            movementsText.text = Game.Player.movements.getString();
            if (movementsText.preferredHeight > 90 && movementsText.preferredHeight < 130)
                movementsText.text += "\n\n\n\n";


            movementsHorizontalScrollBar.value = 0;
            if (selectedReform != null)
            {
                if (callRebuildDropDown) // meaning changed whole reform            
                    rebuildDropDown();
                descriptionText.text = selectedReform + " reforms " + selectedReform.getDescription()
               + "\nCurrently: " + selectedReform.getValue() + " " + selectedReform.getValue().getDescription()
               + "\nSelected: ";

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
                    if (Game.Player.government.getValue() != Government.Despotism)
                    {
                        descriptionText.text += "\n" + procentVotersSayedYes + " of voters want this reform ( ";
                        foreach (PopType type in PopType.getAllPopTypes())
                            if (divisionVotersResult[type] > 0)
                            {
                                Procent res = new Procent(divisionVotersResult[type] / (float)Game.Player.getPopulationAmountByType(type));
                                descriptionText.text += res + " of " + type + "; ";
                            }
                        descriptionText.text += ")";
                    }
                    else
                        descriptionText.text += "\nNobody to vote - Despot rule everything";

                    descriptionText.text += "\n" + procentPopulationSayedYes + " of population want this reform ( ";
                    foreach (PopType type in PopType.getAllPopTypes())
                        if (divisionPopulationResult[type] > 0)
                        {
                            Procent res = new Procent(divisionPopulationResult[type] / (float)Game.Player.getPopulationAmountByType(type));
                            descriptionText.text += res + " of " + type + "; ";
                        }
                    descriptionText.text += ")";
                }

                if (selectedReformValue != null)// && selectedReformValue != selectedReform.getValue())
                {
                    if (procentVotersSayedYes.get() >= Options.votingPassBillLimit || Game.Player.government.getValue() == Government.Despotism)
                    { // has enough voters
                        voteButton.interactable = selectedReformValue.allowed.isAllTrue(Game.Player, selectedReformValue, out voteButton.GetComponentInChildren<ToolTipHandler>().tooltip);
                        forceDecisionButton.GetComponentInChildren<ToolTipHandler>().tooltip = voteButton.GetComponentInChildren<ToolTipHandler>().tooltip;
                        forceDecisionButton.interactable = false;
                        voteButton.GetComponentInChildren<Text>().text = "Vote for " + selectedReformValue;
                    }
                    else // not enough voters
                    {
                        voteButton.interactable = false;
                        forceDecisionButton.interactable = selectedReformValue.allowed.isAllTrue(Game.Player, selectedReformValue, out forceDecisionButton.GetComponentInChildren<ToolTipHandler>().tooltip);
                        voteButton.GetComponentInChildren<ToolTipHandler>().tooltip = forceDecisionButton.GetComponentInChildren<ToolTipHandler>().tooltip;
                        voteButton.GetComponentInChildren<Text>().text = "Not enough votes";
                    }
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
    }
}