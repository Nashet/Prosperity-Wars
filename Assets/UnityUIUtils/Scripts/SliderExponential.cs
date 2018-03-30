using System;
using UnityEngine.UI;

namespace Nashet.UnityUIUtils
{
    //MenuItem("Tools/MyTool/Do It in C#")]
    public class SliderExponential : Slider
    {
        // has to be public
        private Func<float, float> getValueFunction = x => x;

        private Func<float, float> setValueFunction = x => x;

        public float exponentialValue
        {
            get { return getValueFunction(value); }
            set { base.value = setValueFunction(value); }
        }

        [Obsolete]
        public override float value
        {
            get { return base.value; }
            set { base.value = value; }
        }

        public void setExponential(Func<float, float> getValueFunction, Func<float, float> setValueFunction)
        {
            this.getValueFunction = getValueFunction;
            this.setValueFunction = setValueFunction;
        }

        public float ConvertToSliderFormat(float data)
        {
            return setValueFunction(data);
        }

        public float ConvertFromSliderFormat(float data)
        {
            return getValueFunction(data);
        }
    }
}