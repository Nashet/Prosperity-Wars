using Nashet.UnityUIUtils;
using UnityEngine;
using UnityEngine.UI;

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
        private void Awake() // used to position other windows
        {
            MainCamera.bottomPanel = this;
            generalText.text = "Prosperity Wars v0.19.2";
            Hide();
        }

        public override void Refresh()
        {
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