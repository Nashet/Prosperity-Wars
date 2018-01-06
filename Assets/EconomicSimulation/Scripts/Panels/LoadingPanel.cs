using Nashet.UnityUIUtils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace Nashet.EconomicSimulation
{
    public class LoadingPanel : Hideable
    {
        [SerializeField]
        private Text loadingText;
        // Use this for initialization
        void Start()
        {
            MainCamera.loadingPanel = this;
        }

        public void updateStatus(string text)
        {
            loadingText.text = text;
        }
    }
}