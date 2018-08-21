﻿using Nashet.EconomicSimulation.Reforms;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using Nashet.ValueSpace;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private AbstractReform selectedReformType;

        [SerializeField]
        private IReformValue selectedReformValue;

        private readonly List<IReformValue> assotiateTable = new List<IReformValue>();

        // Use this for initialization
        private void Start()
        {
            MainCamera.politicsPanel = this;
            voteButton.interactable = false;
            dropDown.interactable = false;
            forceDecisionButton.interactable = false;
            GetComponent<RectTransform>().anchoredPosition = new Vector2(15f, 45f);
            Hide();
        }

        private void changeReformValue()
        {
            if (!(ReferenceEquals(selectedReformType, null)) && selectedReformValue != null && selectedReformType != selectedReformValue)
            {
                selectedReformType.SetValue(selectedReformValue);
                MainCamera.refreshAllActive();
            }
        }

        public void onVoteClick()
        {
            changeReformValue();
        }

        public void onForceDecisionClick()
        {
            if (Game.Player.government != Government.Despotism)
                foreach (PopUnit pop in Game.Player.Provinces.AllPops)
                {
                    if (pop.CanVoteInOwnCountry() && !pop.getSayingYes(selectedReformValue))// can vote and voted no
                    {
                        pop.addDaysUpsetByForcedReform(Options.PopDaysUpsetByForcedReform);
                    }
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
            foreach (IReformValue next in selectedReformType.AllPossibleValues)
            {
                //if (next.isAvailable(Game.player))
                {
                    dropDown.options.Add(new Dropdown.OptionData { text = next.ToString() });
                    assotiateTable.Add(next);
                    if (selectedReformType == next)
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
            selectedReformType = newSelection;
            if (ReferenceEquals(newSelection, null))
                dropDown.interactable = false;
            Refresh();
        }


        private void refresh(bool callRebuildDropDown)
        {
            table.Refresh();
            movementsText.text = Game.Player.movements.OrderByDescending(x => x.getRelativeStrength(Game.Player).get()).getString();
            if (movementsText.preferredHeight > 90 && movementsText.preferredHeight < 130)
                movementsText.text += "\n\n\n\n";

            movementsHorizontalScrollBar.value = 0;
            if (ReferenceEquals(selectedReformType, null))
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
                descriptionText.text = selectedReformType.ShortName + " reforms " + selectedReformType.FullName
               + "\nCurrently: " + selectedReformType.value;

                var isNamedReform = selectedReformType.value as INameable;
                if (isNamedReform != null)
                    descriptionText.text += isNamedReform.FullName;

                descriptionText.text += "\nSelected: ";

                if (selectedReformType == selectedReformValue)
                {
                    descriptionText.text += "current";
                    forceDecisionButton.interactable = false;
                    voteButton.interactable = false;
                }
                else
                {
                    descriptionText.text += selectedReformValue;

                    var isNamedReform2 = selectedReformValue as INameable;
                    if (isNamedReform2 != null)
                        descriptionText.text += isNamedReform2.FullName;

                    Procent procentPopulationSayedYes = new Procent(0f);
                    Procent procentVotersSayedYes = Game.Player.Provinces.getYesVotes(selectedReformValue, ref procentPopulationSayedYes);

                    Dictionary<PopType, int> divisionPopulationResult = new Dictionary<PopType, int>();
                    Dictionary<PopType, int> divisionVotersResult = Game.Player.Provinces.getYesVotesByType(selectedReformValue, ref divisionPopulationResult);

                    RefreshInfoAboutVotes(procentVotersSayedYes, procentPopulationSayedYes, divisionVotersResult, divisionPopulationResult);

                    // Control buttons interactability && tooltips
                    if (selectedReformValue != null)
                    {
                        if (procentVotersSayedYes.get() >= Options.votingPassBillLimit && Game.Player.government != Government.Despotism)
                        { // can vote for reform
                            voteButton.interactable = selectedReformValue.IsAllowed(Game.Player, selectedReformValue, out voteButton.GetComponent<ToolTipHandler>().text);
                            forceDecisionButton.GetComponent<ToolTipHandler>().SetText(voteButton.GetComponent<ToolTipHandler>().GetText());
                            forceDecisionButton.interactable = false;
                            voteButton.GetComponentInChildren<Text>().text = "Vote for " + selectedReformValue;
                        }
                        else // can't vote for reform or is despotism
                        {
                            voteButton.interactable = false;
                            forceDecisionButton.interactable = selectedReformValue.IsAllowed(Game.Player, selectedReformValue, out forceDecisionButton.GetComponent<ToolTipHandler>().text);
                            voteButton.GetComponent<ToolTipHandler>().SetText(forceDecisionButton.GetComponent<ToolTipHandler>().GetText());
                            voteButton.GetComponentInChildren<Text>().text = "Not enough votes";
                            if (Game.Player.government == Government.Despotism)
                                forceDecisionButton.GetComponent<ToolTipHandler>().text += "\n\nPeople wouldn't be that angry if you force decisions as Despot";
                            else
                                forceDecisionButton.GetComponent<ToolTipHandler>().text += "\n\nForcing decision against people's desires will drop loyalty!";
                        }
                    }
                }
            }
        }

        private void RefreshInfoAboutVotes(Procent procentVotersSayedYes, Procent procentPopulationSayedYes, Dictionary<PopType, int> divisionVotersResult, Dictionary<PopType, int> divisionPopulationResult)
        {
            if (Game.Player.government == Government.Despotism)
                descriptionText.text += "\nNobody to vote - Despot rules everything";
            else
            {
                descriptionText.text += "\n" + procentVotersSayedYes + " of voters want this reform ( ";
                foreach (PopType type in PopType.getAllPopTypes())
                    if (divisionVotersResult[type] > 0)
                    {
                        Procent res = new Procent(divisionVotersResult[type] / (float)Game.Player.Provinces.getPopulationAmountByType(type));
                        descriptionText.text += res + " of " + type + "; ";
                    }
                descriptionText.text += ")";
            }

            descriptionText.text += "\n" + procentPopulationSayedYes + " of population want this reform ( ";
            foreach (PopType type in PopType.getAllPopTypes())
                if (divisionPopulationResult[type] > 0)
                {
                    Procent res = new Procent(divisionPopulationResult[type] / (float)Game.Player.Provinces.getPopulationAmountByType(type));
                    descriptionText.text += res + " of " + type + "; ";
                }
            descriptionText.text += ")";
        }
    }
}