using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Nashet.EconomicSimulation
{
    public class LinksManager : MonoBehaviour
    {

        public Material defaultCountryBorderMaterial, defaultProvinceBorderMaterial, impassableBorder;
        public GameObject UnitPrefab, UnitPanelPrefab;
        public Transform WorldSpaceCanvas;
        public GameObject r3DProvinceTextPrefab, r3DCountryTextPrefab;
        public GameObject ArmiesSelectionWindowPrefab;
        public GameObject ArmiesHolder;

        [SerializeField]public Canvas CameraLayerCanvas;

        public Material waterMaterial;

        private static LinksManager thisObject;
        public Material ProvinceSelecionMaterial;
        public Material FogOfWarMaterial;

        [SerializeField] private GameObject[] objectsToInstantiateIn2DCanvas;

        // Use this for initialization
        private void Start()
        {
            thisObject = this;
            if (ArmiesSelectionWindow.Get == null)
            {
                var window = Instantiate(ArmiesSelectionWindowPrefab, CameraLayerCanvas.transform);
                //window.hideFlags();
            }

            foreach (var item in objectsToInstantiateIn2DCanvas)
            {
                Instantiate(item, CameraLayerCanvas.transform);
            }
        }




        public static LinksManager Get
        {
            get { return thisObject; }
        }

        // Update is called once per frame
        private void Update()
        {

        }
    }
}