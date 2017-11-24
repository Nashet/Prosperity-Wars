using System;
using UnityEngine.UI;

public class SliderExponential : Slider
{
    private Func<float, float> getValueFunction = x => x;
    private Func<float, float> setValueFunction = x => x;
   
    public float expotentialValue
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
  
    public void setExpotential(Func<float, float> getValueFunction, Func<float, float> setValueFunction)
    {
        this.getValueFunction = getValueFunction;
        this.setValueFunction = setValueFunction;
    }
}
