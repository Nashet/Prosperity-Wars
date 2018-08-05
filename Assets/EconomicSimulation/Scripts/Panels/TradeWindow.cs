using System.Text;
using Nashet.UnityUIUtils;
using UnityEngine;
using UnityEngine.UI;

namespace Nashet.EconomicSimulation
{
    public class TradeWindow : DragPanel
    {
        [SerializeField]
        private CountryStorageTable countryStorageTable;

        [SerializeField]
        private WorldMarketTable worldMarketTable;

        [SerializeField]
        private Text txtBuyIfLessThan, txtSaleIfMoreThan;

        [SerializeField]
        private SliderExponential slBuyIfLessThan, slSellIfMoreThan;

        [SerializeField]
        private GameObject tradeSliders;

        private Product selectedProduct;

        // Use this for initialization
        private void Start()
        {
            slBuyIfLessThan.setExponential(x => 0.2f * x * x, x => Mathf.Sqrt(x * 5f));
            slSellIfMoreThan.setExponential(x => 0.2f * x * x, x => Mathf.Sqrt(x * 5f));
            MainCamera.tradeWindow = this;
            GetComponent<RectTransform>().anchoredPosition = new Vector2(-775f, -642);                
            Hide();
        }

        public override void Show()
        {
            if (selectedProduct == null)
                selectProduct(Product.Fish);
            base.Show();
        }

        public override void Refresh()
        {
            tradeSliders.SetActive(!Game.Player.isAI());
            worldMarketTable.Refresh();
            countryStorageTable.Refresh();
        }

        public void refreshTradeLimits()
        {
            var sb = new StringBuilder();
            sb.Append(selectedProduct).Append(": Buy if less than: ").Append(slBuyIfLessThan.exponentialValue.ToString("F0"));
            txtBuyIfLessThan.text = sb.ToString();
            txtSaleIfMoreThan.text = "Sell if more than: " + slSellIfMoreThan.exponentialValue.ToString("F0");
        }

        public void onslBuyIfLessThanChange()
        {
            if (slBuyIfLessThan.exponentialValue > slSellIfMoreThan.exponentialValue)
            {
                slSellIfMoreThan.exponentialValue = slBuyIfLessThan.exponentialValue;
                //Game.Player.setSellIfMoreLimits(selectedProduct, slSellIfMoreThan.value);
            }
            Game.Player.setBuyIfLessLimits(selectedProduct, slBuyIfLessThan.exponentialValue);
            refreshTradeLimits();
        }

        public void onslSellIfMoreThanChange()
        {
            if (slBuyIfLessThan.exponentialValue > slSellIfMoreThan.exponentialValue)
            {
                slBuyIfLessThan.exponentialValue = slSellIfMoreThan.exponentialValue;
                //Game.Player.setBuyIfLessLimits(selectedProduct, slBuyIfLessThan.value);
            }
            Game.Player.setSellIfMoreLimits(selectedProduct, slSellIfMoreThan.exponentialValue);
            refreshTradeLimits();
        }

        public void selectProduct(Product product)
        {
            if (!product.isAbstract() && product.IsStorable && product != Product.Gold)
            {
                selectedProduct = product;
                slBuyIfLessThan.exponentialValue = Game.Player.getBuyIfLessLimits(selectedProduct).get();
                slSellIfMoreThan.exponentialValue = Game.Player.getSellIfMoreLimits(selectedProduct).get();
                refreshTradeLimits();
            }
        }
    }
}