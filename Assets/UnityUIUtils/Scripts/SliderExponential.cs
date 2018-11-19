using System;
#if UNITY_EDITOR 
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace Nashet.UnityUIUtils
{
    [AddComponentMenu("UI/Exponential Slider", 33)]
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
#if UNITY_EDITOR
        [MenuItem("GameObject/UI/Exponential Slider", false, 10)]
        private static void CreateCustomGameObject(MenuCommand menuCommand)
        {            
            // Create a custom game object
            GameObject go = new GameObject("Exponential Slider");

            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;

            var added = go.AddComponent<SliderExponential>();
        }
#endif
    }
}