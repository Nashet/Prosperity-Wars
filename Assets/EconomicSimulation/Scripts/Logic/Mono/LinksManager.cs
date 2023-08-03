using UnityEngine;
namespace Nashet.EconomicSimulation
{
    public class LinksManager : MonoBehaviour
    {
        public Material defaultCountryBorderMaterial, defaultProvinceBorderMaterial, impassableBorder, riverBorder;
        public Material defaultUnitSymbol;
		public Material shoreMaterial;
		public GameObject UnitPrefab, UnitPanelPrefab;
        public Transform WorldSpaceCanvas;
        public GameObject r3DProvinceTextPrefab, r3DCountryTextPrefab;
        public GameObject ArmiesSelectionWindowPrefab;
        public GameObject ArmiesHolder;

        [SerializeField] public Canvas CameraLayerCanvas;
        [SerializeField] public KeyCode AdditionKey = KeyCode.LeftAlt;
		

		public Material waterMaterial;

        private static LinksManager thisObject;
        public Material ProvinceSelecionMaterial;
        public Material FogOfWarMaterial;
        public bl_Joystick scrolJoystic;

		[SerializeField] public Texture2D rebelsFlag;

		[SerializeField] private GameObject[] objectsToInstantiateIn2DCanvas;
        public Canvas UICanvas;

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
    }
}