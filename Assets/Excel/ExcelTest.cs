using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExcelTest : MonoBehaviour
{
    public Button btn;
    void Start()
    {
        btn.onClick.AddListener(OnClicked);
    }

    void OnClicked()
    {
        Debug.Log("price:"+ItemManager.Instance.dataArray[0].itemPrice);
    }
}
