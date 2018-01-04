using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GoodsPanel : DragPanel
{
    [SerializeField]
    private Text generaltext;

    [SerializeField]
    private RawImage priceGraph;

    private Product product;    
    private readonly int textureWidth = 300, textureHeight = 300;
    private Texture2D graphTexture;
    // Use this for initialization
    void Start()
    {
        graphTexture = new Texture2D(textureWidth, textureHeight);
        //priceGraph = GameObject.Find("PriceGraph").GetComponent<RawImage>();
        MainCamera.goodsPanel = this;
        GetComponent<RectTransform>().anchoredPosition = new Vector2(800f, 200f);
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
                + "\n price: " + Game.market.getPrice(product).get() + " supply: " + Game.market.getMarketSupply(product, true).get() + " consumption: " + Game.market.getBouthOnMarket(product, true).get();


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

            var dataStorage = Game.market.priceHistory.getPool(product);
            if (dataStorage != null)
            {
                var priceArray = dataStorage.data.ToArray();

                var maxValue = priceArray.MaxBy(x => x.get());

                float yValueMultiplier = 300f / maxValue.get() * 0.99f; ;
                if (maxValue.get() < 300f)
                    yValueMultiplier = yValueMultiplier / 2f;

                for (int textureX = 0; textureX < textureWidth; textureX++)
                {
                    int yValue = 0;
                    if (textureX == 0)
                    {
                        yValue = (int)(priceArray[0].get() * yValueMultiplier);
                        graphTexture.SetPixel(textureX, yValue, graphColor);
                    }
                    else
                    {
                        //nearestPoint = (float)i / PricePool.lenght;
                        float nearestPoint = (float)textureX / (textureWidth / (float)PricePool.lenght);
                        int nearesPointInt = (int)nearestPoint;
                        float remainsA = nearestPoint - nearesPointInt;
                        if (remainsA == 0f)
                        {
                            yValue = (int)(priceArray[nearesPointInt].get() * yValueMultiplier);
                            graphTexture.SetPixel(textureX, yValue, graphColor);
                        }
                        else if (nearesPointInt + 1 < priceArray.Length)
                        {
                            float remainsB = 1f - remainsA;
                            int yValueMiddle = (int)((priceArray[nearesPointInt].get() * remainsB
                                + priceArray[nearesPointInt + 1].get() * remainsA) * yValueMultiplier);
                            graphTexture.SetPixel(textureX, yValueMiddle, graphColor);
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
