using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nashet.EconomicSimulation
{
    public class UnitPanel : MonoBehaviour
    {
        [SerializeField]
        private Unit unit;

        [SerializeField]
        private RawImage flag;
        public void Set(Texture2D flag)
        {
            this.flag.texture = flag;
        }
    }
}