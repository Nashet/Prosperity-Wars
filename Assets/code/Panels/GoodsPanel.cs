using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GoodsPanel : DragPanel
{    
    public Text generaltext;
    Product product;
    RawImage priceGraph;
    static int textureWidth = 300, textureHeight = 300;
    Texture2D graphTexture;
    // Use this for initialization
    void Start()
    {
        graphTexture = new Texture2D(textureWidth, textureHeight);
        priceGraph = GameObject.Find("PriceGraph").GetComponent<RawImage>();
        MainCamera.goodsPanel = this;
        hide();
    }

    // Update is called once per frame
    void Update()
    {
        //refresh();
    }
    public void refresh()
    {
        if (product != null)
        {
            generaltext.text = product.ToString()
                + "\n price: " + Game.market.findPrice(product) + " supply: " + Game.market.getSupply(product, true) + " consumption: " + Game.market.getBouth(product, true);


            //            graphTexture.
            // Reset all pixels color to transparent
            Color32 resetColor = new Color32(0, 0, 0, 255);
            Color32[] resetColorArray = graphTexture.GetPixels32();

            for (int i = 0; i < resetColorArray.Length; i++)
            {
                resetColorArray[i] = resetColor;
            }
            graphTexture.SetPixels32(resetColorArray);
            graphTexture.Apply();
            Color32 graphColor = new Color32(10, 200, 20, 255);

            int yValue = 0;
            float remainsA = 0;
            float nearestPoint = 0;
            var dataStorage = Game.market.priceHistory.getPool(product);
            if (dataStorage != null)
            {
                var priceArray = dataStorage.data.ToArray();
                for (int i = 0; i < textureWidth; i++)
                {

                    if (i == 0)
                    {
                        yValue = (int)priceArray[0].get();
                        graphTexture.SetPixel(i, yValue, graphColor);
                    }
                    else
                    {
                        //nearestPoint = (float)i / PricePool.lenght;
                        nearestPoint = (float)i / (textureWidth / (float)PricePool.lenght);
                        int nearesPointInt = (int)nearestPoint;
                        remainsA = nearestPoint - nearesPointInt;
                        if (remainsA == 0f)
                        {
                            yValue = (int)priceArray[nearesPointInt].get();
                            graphTexture.SetPixel(i, yValue, graphColor);
                        }
                        else if (nearesPointInt +1 < priceArray.Length)
                        {
                            float remainsB = 1f - remainsA;
                            int yValueMiddle = (int)(priceArray[nearesPointInt].get() * remainsB + priceArray[nearesPointInt + 1].get() * remainsA);
                            graphTexture.SetPixel(i, yValueMiddle, graphColor);
                        }
                        //float dif = 
                    }


                }
                graphTexture.Apply();
                priceGraph.texture = graphTexture;

                //foreach (Value price in Game.market.priceHistory)
                //    //graphTexture.d;;
                //    ;
                //priceGraph.texture = graphTexture;
            }
        }
    }
    //internal int getGraphPoint()
    //{ }
    public void Show(Product inn, bool bringOnTop)
    {
        gameObject.SetActive(true);
        product = inn;
        if (bringOnTop)
        panelRectTransform.SetAsLastSibling();
    }
    
   

}
