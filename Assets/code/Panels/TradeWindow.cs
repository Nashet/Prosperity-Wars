using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Text;

public class TradeWindow : DragPanel
{
    public ScrollRect table;
    public Text txtBuyIfLessThan, txtSaleIfMoreThan;
    public SliderExponential slBuyIfLessThan, slSellIfMoreThan;
    public GameObject tradeSliders;

    private Product selectedProduct;

    // Use this for initialization
    void Start()
    {
        slBuyIfLessThan.setExponential(x => 0.2f * x * x, x => Mathf.Sqrt(x * 5f));
        slSellIfMoreThan.setExponential(x => 0.2f * x * x, x => Mathf.Sqrt(x * 5f));
        MainCamera.tradeWindow = this;
        GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, MainCamera.topPanel.GetComponent<RectTransform>().rect.height * -1f);
        hide();

    }


    public void show(bool bringOnTop)
    {
        gameObject.SetActive(true);
        if (bringOnTop)
            panelRectTransform.SetAsLastSibling();
        if (selectedProduct == null)
            selectProduct(Product.Fish);
        // refresh(); don't do it - recursion
    }


    public void refresh()
    {

        tradeSliders.SetActive(!Game.Player.isAI());
        hide();
        show(false);

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
    internal void selectProduct(Product product)
    {
        if (!product.isAbstract())
        {
            selectedProduct = product;
            slBuyIfLessThan.exponentialValue = Game.Player.getBuyIfLessLimits(selectedProduct).get();
            slSellIfMoreThan.exponentialValue = Game.Player.getSellIfMoreLimits(selectedProduct).get();
            refreshTradeLimits();
        }
    }
}

