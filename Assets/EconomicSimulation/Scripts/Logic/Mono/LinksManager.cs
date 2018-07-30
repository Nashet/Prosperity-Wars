using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Nashet.EconomicSimulation
{
    public class LinksManager : MonoBehaviour
    {

        public Material defaultCountryBorderMaterial, defaultProvinceBorderMaterial, 
                impassableBorder;
        public GameObject UnitPrefab, UnitPanelPrefab;
        public Transform WorldSpaceCanvas;
        public GameObject r3DProvinceTextPrefab, r3DCountryTextPrefab;
        public GameObject ArmiesSelectionWindowPrefab;
        public GameObject ArmiesHolder;
        [SerializeField]
        public Canvas CameraLayerCanvas;

        public Material waterMaterial;

        private static LinksManager thisObject;
        public Material ProvinceSelecionMaterial;
        public Material FogOfWarMaterial;

        // Use this for initialization
        void Start()
        {
            thisObject = this;
            if (ArmiesSelectionWindow.Get == null)
            {
                var window = Instantiate(ArmiesSelectionWindowPrefab, CameraLayerCanvas.transform);
                //window.hideFlags();
            }
        }

        public static LinksManager Get
        {
            get { return thisObject; }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}