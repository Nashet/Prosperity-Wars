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
    public Slider slBuyIfLessThan, slSellIfMoreThan;
    public GameObject tradeSliders;

    private Product selectedProduct;

    // Use this for initialization
    void Start()
    {
        MainCamera.tradeWindow = this;
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
        sb.Append(selectedProduct).Append(": Buy if less than: ").Append(slBuyIfLessThan.value);
        txtBuyIfLessThan.text = sb.ToString();
        txtSaleIfMoreThan.text = "Sell if more than: " + slSellIfMoreThan.value;
    }
    public void onslBuyIfLessThanChange()
    {
        if (slBuyIfLessThan.value > slSellIfMoreThan.value)
            //slBuyIfLessThan.value = slSellIfMoreThan.value;
            slSellIfMoreThan.value = slBuyIfLessThan.value;
        Game.Player.setBuyIfLessLimits(selectedProduct, slBuyIfLessThan.value);
        refreshTradeLimits();
    }
    public void onslSellIfMoreThanChange()
    {
        if (slBuyIfLessThan.value > slSellIfMoreThan.value)
            //slSellIfMoreThan.value = slBuyIfLessThan.value;
            slBuyIfLessThan.value = slSellIfMoreThan.value;
        Game.Player.setSellIfMoreLimits(selectedProduct, slSellIfMoreThan.value);
        refreshTradeLimits();
    }
    internal void selectProduct(Product product)
    {
        if (!product.isAbstract())
        {
            selectedProduct = product;
            slBuyIfLessThan.value = Game.Player.getBuyIfLessLimits(selectedProduct).get();
            slSellIfMoreThan.value = Game.Player.getSellIfMoreLimits(selectedProduct).get();
            refreshTradeLimits();
        }
    }
}

