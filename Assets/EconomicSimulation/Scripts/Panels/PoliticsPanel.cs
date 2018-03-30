﻿using System.Collections.Generic;
using System.Linq;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using Nashet.ValueSpace;
using UnityEngine;
using UnityEngine.UI;

namespace Nashet.EconomicSimulation
{
    public class PoliticsPanel : DragPanel
    {
        [SerializeField]
        private PoliticsPanelTable table;

        [SerializeField]
        private Text descriptionText, movementsText;

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
        private void Start()
        {
            MainCamera.politicsPanel = this;
            voteButton.interactable = false;
            dropDown.interactable = false;
            forceDecisionButton.interactable = false;
            GetComponent<RectTransform>().anchoredPosition = new Vector2(150f, -150f);
            Hide();
        }

        private void changeReformValue()
        {
            if (selectedReform != null && selectedReformValue != null && selectedReformValue != selectedReform.getValue())
            {
                selectedReform.setValue(selectedReformValue);
                MainCamera.refreshAllActive();
            }
        }

        public void onVoteClick()
        {
            changeReformValue();
        }

        public void onForceDecisionClick()
        {
            //uint votersSayedYes;
            foreach (PopUnit pop in Game.Player.GetAllPopulation())
                if (pop.canVote() && !pop.getSayingYes(selectedReformValue))
                {
                    //votersSayedYes = pop.getSayingYes(selectedReformValue);
                    //if (pop.getSayYesProcent(selectedReformValue) < Options.votingPassBillLimit)
                    pop.addDaysUpsetByForcedReform(Options.PopDaysUpsetByForcedReform);
                }

            changeReformValue();
        }

        //slider.onValueChanged.AddListener(ListenerMethod);

        public void onChoiceValueChanged()
        {
            selectedReformValue = assotiateTable[dropDown.value];
            refresh(false);
        }

        private void rebuildDropDown()
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
                    dropDown.options.Add(new Dropdown.OptionData { text = next.ToString() });
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

        public override void Refresh()
        {
            refresh(true);
        }

        public void selectReform(AbstractReform newSelection)
        {
            selectedReform = newSelection;
            if (newSelection == null)
                dropDown.interactable = false;
        }

        private void refresh(bool callRebuildDropDown)
        {
            table.Refresh();
            movementsText.text = Game.Player.movements.OrderByDescending(x => x.getRelativeStrength(Game.Player).get()).getString();
            if (movementsText.preferredHeight > 90 && movementsText.preferredHeight < 130)
                movementsText.text += "\n\n\n\n";

            movementsHorizontalScrollBar.value = 0;
            if (selectedReform == null)
            {
                voteButton.interactable = false;
                voteButton.GetComponentInChildren<Text>().text = "Select reform";
                descriptionText.text = "Select reform from left";
                forceDecisionButton.GetComponent<ToolTipHandler>().SetText("");
                voteButton.GetComponent<ToolTipHandler>().SetText("");
            } //did selected reform
            else
            {
                if (callRebuildDropDown) // meaning changed whole reform
                    rebuildDropDown();
                descriptionText.text = selectedReform + " reforms " + selectedReform.FullName
               + "\nCurrently: " + selectedReform.getValue() + " " + selectedReform.getValue().FullName
               + "\nSelected: ";

                //if (selectedReformValue != null)
                if (selectedReformValue != selectedReform.getValue())
                    descriptionText.text += selectedReformValue + " " + selectedReformValue.FullName;
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
                        voteButton.interactable = selectedReformValue.allowed.isAllTrue(Game.Player, selectedReformValue, out voteButton.GetComponent<ToolTipHandler>().text);
                        forceDecisionButton.GetComponent<ToolTipHandler>().SetText(voteButton.GetComponent<ToolTipHandler>().GetText());
                        forceDecisionButton.interactable = false;
                        voteButton.GetComponentInChildren<Text>().text = "Vote for " + selectedReformValue;
                    }
                    else // not enough voters
                    {
                        voteButton.interactable = false;
                        forceDecisionButton.interactable = selectedReformValue.allowed.isAllTrue(Game.Player, selectedReformValue, out forceDecisionButton.GetComponent<ToolTipHandler>().text);
                        voteButton.GetComponent<ToolTipHandler>().SetText(forceDecisionButton.GetComponent<ToolTipHandler>().GetText());
                        voteButton.GetComponentInChildren<Text>().text = "Not enough votes";
                        forceDecisionButton.GetComponent<ToolTipHandler>().text += "\n\nForcing decision against people's desires will drop loyalty!";
                    }
                }
            }
        }
    }
}