using UnityEngine;
using System.Collections;

namespace RaftTest
{
    public class WriteToDepth : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {

            GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
