using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoodsItem : ListViewItem
{
    [SerializeField] Text m_nameText;
    [SerializeField] Text m_priceText;

    public GoodsData goodsData { get; private set; }

    public void Init(GoodsData data)
    {
        goodsData = data;
        m_nameText.text = data.name;
        m_priceText.text = data.price.ToString();
    }
}
