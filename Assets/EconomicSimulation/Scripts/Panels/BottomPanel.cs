
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Nashet.UnityUIUtils;

namespace Nashet.EconomicSimulation
{
    /// <summary>
    /// Just bottom panel with several buttons
    /// </summary>
    public class BottomPanel : Window
    {
        [SerializeField]
        private Text generalText;
        // Use this for initialization
        void Awake()
        {
            MainCamera.bottomPanel = this;
            Hide();
        }
       
        public override void Refresh()
        {
            generalText.text = "Economic Simulation Demo v0.16.1";
        }

        public void onStatisticsClick()
        {
            if (MainCamera.StatisticPanel.isActiveAndEnabled)
                MainCamera.StatisticPanel.Hide();
            else
                MainCamera.StatisticPanel.Show();
        }
        public void onddMapModesChange(int newMapMode)
        {
            if (Game.getMapMode() != newMapMode)
                Game.redrawMapAccordingToMapMode(newMapMode);

        }
    }
}