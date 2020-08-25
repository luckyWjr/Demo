using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoodsItem : ListViewItem
{
    public Text m_nameText;
    public Text m_priceText;

    public void Init(GoodsData data)
    {
        m_nameText.text = data.name;
        m_priceText.text = data.price.ToString();
    }
}
