using Nashet.UnityUIUtils;
using UnityEngine;
using UnityEngine.UI;

namespace Nashet.EconomicSimulation
{
    public class LoadingPanel : Hideable
    {
        [SerializeField]
        private Text loadingText;

        // Use this for initialization
        private void Start()
        {
            MainCamera.loadingPanel = this;
        }

        public void updateStatus(string text)
        {
            loadingText.text = text;
        }
    }
}