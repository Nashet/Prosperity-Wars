using System;
using UnityEngine.UI;
namespace Nashet.UnityUIUtils
{
    public class SliderExponential : Slider
    {
        public Func<float, float> getValueFunction = x => x;
        public Func<float, float> setValueFunction = x => x;

        public float exponentialValue
        {
            get
            {
                return getValueFunction(value);
            }

            set
            {
                base.value = setValueFunction(value);
            }
        }
        [Obsolete]
        public override float value
        {
            get
            {
                return base.value;
            }

            set
            {
                base.value = value;
            }
        }

        public void setExponential(Func<float, float> getValueFunction, Func<float, float> setValueFunction)
        {
            this.getValueFunction = getValueFunction;
            this.setValueFunction = setValueFunction;
        }
    }
}