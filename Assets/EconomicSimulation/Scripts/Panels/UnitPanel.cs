﻿using Nashet.UnityUIUtils;
using UnityEngine;
using UnityEngine.UI;

namespace Nashet.EconomicSimulation
{
	public class UnitPanel : Hideable//MonoBehaviour
    {        
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
        //public void Move(Province where)
        //{
        //    var position = where.getPosition();
        //    position.y += unitPanelYOffset;
        //    transform.position = position;
        //}
    }
}