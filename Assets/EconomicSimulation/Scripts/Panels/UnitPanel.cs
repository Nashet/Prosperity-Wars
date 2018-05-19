using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nashet.EconomicSimulation
{
    public class UnitPanel : MonoBehaviour
    {
        //[SerializeField]
        //private Unit unit;

        [SerializeField]
        private Text panelText;

        [SerializeField]
        private RawImage flag;
        public void SetFlag(Texture2D flag)
        {
            this.flag.texture = flag;
        }
        public void SetText(string text)
        {
            panelText.text = text;
        }
    }
}