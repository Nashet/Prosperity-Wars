using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideStandAlone : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
#if !UNITY_STANDALONE
        gameObject.SetActive(false);
#endif
    }

    // Update is called once per frame
    void Update()
    {

    }
}
