using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MainCamera : MonoBehaviour
{
    public Game game;
    internal static Camera cameraMy;
    static GameObject mapPointer;
    public static TopPanel topPanel;
    public static ProvincePanel provincePanel;
    public static PopulationPanel populationPanel;
    public static PopUnitPanel popUnitPanel;

    internal static TradeWindow tradeWindow;
    internal static ProductionWindow productionWindow;
    internal static FactoryPanel factoryPanel;
    internal static GoodsPanel goodsPanel;
    internal static InventionsPanel inventionsPanel;
    internal static BuildPanel buildPanel;
    internal static PoliticsPanel politicsPanel;

    // Use this for initialization
    //public Text generalText;
    void Start()
    {
        
        GameObject gameControllerObject = GameObject.FindWithTag("MainCamera");
        if (gameControllerObject != null)
        {
            cameraMy = gameControllerObject.GetComponent<Camera>();
        }
        if (cameraMy == null)
        {
            Debug.Log("Cannot find 'cameraMy' ");
        }
        mapPointer = GameObject.FindWithTag("pointerMy");
        if (mapPointer == null)
        {
            Debug.Log("Cannot find 'pointerMy' ");
        }
        //topPanel = transform.FindChild("TopPanel").gameObject;
        //.GetComponent<Panel>()
        game = new Game();
    }
    int GetRayCastMeshNumber()
    {
        RaycastHit hit = new RaycastHit();//temp
                                          //int layerMask = 1 << 8;
                                          //DefaultRaycastLayers
                                          //Physics.DefaultRaycastLayers;

        if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            if (!Physics.Raycast(cameraMy.ScreenPointToRay(Input.mousePosition), out hit))
                return -1;
            else; // go on
        else return -3; //hovering over UI

        MeshCollider meshCollider = hit.collider as MeshCollider;

        if (meshCollider == null || meshCollider.sharedMesh == null)
            return -2;
        Mesh mesh = meshCollider.sharedMesh;

        //Vector3[] vertices = mesh.vertices;
        //int[] triangles = mesh.triangles;

        //Vector3 p0 = vertices[triangles[hit.triangleIndex * 3 + 0]];

        //mapPointer.transform.position = p0;
        //// Gets a vector that points from the player's position to the target's.
        //Vector3 heading = mapPointer.transform.position - Vector3.zero;
        //heading.Normalize();

        // mapPointer.transform.position += heading * 50;
        // mapPointer.transform.rotation = Quaternion.LookRotation(heading, Vector3.up) * Quaternion.Euler(-90, 0, 0);

        //return groundMesh.triangles[hit.triangleIndex * 3];
        ;
        return System.Convert.ToInt32(mesh.name);
    }
    internal void SelectProvince(int number)
    {
        if (Game.selectedProvince != null && number >= 0)
            Game.selectedProvince.meshRenderer.material.color = Game.selectedProvince.colorID;

        if (number >= 0)
        {
            if (Province.allProvinces[number] == Game.selectedProvince)// same province selected
            {
                Game.selectedProvince.meshRenderer.material.color = Game.selectedProvince.colorID;
                Game.selectedProvince = null;
                provincePanel.hide();
                if (buildPanel.isActiveAndEnabled)
                    buildPanel.refresh();
            }
            else // new province selected
            {
                Province.allProvinces[number].meshRenderer.material.color = Color.gray;
                //Game.selectedProvince = Province.allProvinces[GetRayCastMeshNumber()];
                Game.selectedProvince = Province.allProvinces[number];
                provincePanel.show();
                if (buildPanel.isActiveAndEnabled)
                    buildPanel.refresh();
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // clicked and released left button
        {
            int meshNumber = GetRayCastMeshNumber();
            //found something correct            
            SelectProvince(meshNumber);
        }
        if (Game.haveToStepSimulation || Game.haveToRunSimulation)
        {
            Game.stepSimulation();

            if (topPanel.isActiveAndEnabled) topPanel.refresh();
            if (popUnitPanel.isActiveAndEnabled) popUnitPanel.refresh();
            if (populationPanel.isActiveAndEnabled) populationPanel.refresh();
            if (tradeWindow.isActiveAndEnabled) tradeWindow.Refresh();
            if (factoryPanel.isActiveAndEnabled) factoryPanel.refresh();
            if (productionWindow.isActiveAndEnabled) productionWindow.refresh();
            if (goodsPanel.isActiveAndEnabled) goodsPanel.refresh();
            if (inventionsPanel.isActiveAndEnabled) inventionsPanel.refresh();
            if (buildPanel.isActiveAndEnabled) buildPanel.refresh();
            if (politicsPanel.isActiveAndEnabled) politicsPanel.refresh(true);
        }
        if (Game.haveToStepSimulation)
            Game.haveToStepSimulation = false;

        if (Game.selectedProvince != null)
            ProvincePanel.UpdateProvinceWindow(Game.selectedProvince);

    }
    // This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    void FixedUpdate()
    {
        float xyCameraSpeed = 10f;
        float zCameraSpeed = 250f;
        float xMove = Input.GetAxis("Horizontal");
        float yMove = Input.GetAxis("Vertical");
        float zMove = Input.GetAxis("Mouse ScrollWheel");
        transform.Translate(xMove * xyCameraSpeed, yMove * xyCameraSpeed, zMove * zCameraSpeed);
    }

}
