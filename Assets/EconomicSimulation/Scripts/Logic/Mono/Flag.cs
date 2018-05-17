using Nashet.EconomicSimulation;
using Nashet.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Nashet
{
    public class Flag : MonoBehaviour
    {

        void Start()
        {

        }



        // Update is called once per frame
        void Update()
        {

        }
        enum StripesDirection { horizontal, vertical }
        public static Texture2D Generate(int textureWidth, int textureHeight)
        {
            int stripesAmount = Rand.Get.Next(3) + 1;
            int stripeSize = textureWidth / stripesAmount;
            var res = new Texture2D(textureWidth, textureHeight);
            //for (int i = 0; i < stripesAmount; i++)
            {
                //Vertical stripes
                int stripeNumber = 0;
                var color = ColorExtensions.getRandomColor();
                for (int x = 0; x < textureWidth; x++)
                {
                    if (x > stripeSize * stripeNumber)
                    {
                        stripeNumber++;
                        color = ColorExtensions.getRandomColor();
                    }
                    for (int y = 0; y < textureHeight; y++)
                    {
                        res.SetPixel(x, y, color);
                    }
                }

            }
            res.Apply();
            return res;
        }

    }
}