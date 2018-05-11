using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinksManager : MonoBehaviour
{

    public Material defaultCountryBorderMaterial, defaultProvinceBorderMaterial, selectedProvinceBorderMaterial,
            impassableBorder;
    public GameObject UnitPrefab, UnitPanelPrefab;    
    public Transform WorldSpaceCanvas;
    public GameObject r3DProvinceTextPrefab, r3DCountryTextPrefab;

    private static LinksManager thisObject;
    // Use this for initialization
    void Start()
    {
        thisObject = this;
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
