using Nashet.UnityUIUtils;
using Nashet.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Nashet.EconomicSimulation
{
    public class GoodsPanel : DragPanel
    {
        [SerializeField]
        private Text generaltext;

        [SerializeField]
        private RawImage priceGraph;

        private Color32 graphColor = GUIChanger.DarkestColor;
        private Color32 backGroundColor = GUIChanger.ButtonsColor;

        private Product product;
        private readonly int textureWidth = 300, textureHeight = 300;
        private Texture2D graphTexture;

        // Use this for initialization
        private void Start()
        {
            graphTexture = new Texture2D(textureWidth, textureHeight);
            //priceGraph = GameObject.Find("PriceGraph").GetComponent<RawImage>();
            MainCamera.goodsPanel = this;
            GetComponent<RectTransform>().anchoredPosition = new Vector2(800f, 200f);
            Hide();
            graphColor = GUIChanger.DarkestColor;
            backGroundColor = GUIChanger.ButtonsColor;
        }

        // Update is called once per frame
        private void Update()
        {
            //refresh();
        }

        public override void Refresh()
        {
            if (product != null)
            {
                generaltext.text = product
                    + "\n price: " + Game.Player.market.getCost(product).Get() + " supply: " + Game.Player.market.getMarketSupply(product, true).get() 
                    + " consumption: " + Game.Player.market.getBouthOnMarket(product, true).get();

                Color32[] resetColorArray = graphTexture.GetPixels32();

                for (int i = 0; i < resetColorArray.Length; i++)
                {
                    resetColorArray[i] = backGroundColor;
                }
                graphTexture.SetPixels32(resetColorArray);
                graphTexture.Apply();

                var dataStorage = Game.Player.market.priceHistory.getPool(product);
                if (dataStorage != null)
                {
                    var priceArray = dataStorage.data.ToArray();

                    var maxValue = priceArray.MaxBy(x => x.get());

                    float yValueMultiplier = 300f / maxValue.get() * 0.99f;
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

                    //foreach (Value price in Country.market.priceHistory)
                    //    //graphTexture.d;;
                    //    ;
                    //priceGraph.texture = graphTexture;
                }
            }
        }

        //public int getGraphPoint()
        //{ }
        public void show(Product inn)
        {
            product = inn;
            Show();
        }
    }
}