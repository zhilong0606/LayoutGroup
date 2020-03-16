using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public UILayoutGroupItemPool m_pool;

	void Start ()
	{
	    gameObject.AddComponent<Toggle>();
	    Debug.LogError(transform.GetComponentsInChildren<Toggle>().Length);
        m_pool.RegisterItemInitializer(OnInitItem);
	    m_pool.Init(OnSetItem);
	    m_pool.Apply(2);
        m_pool.Apply(10);
        m_pool.Apply(3);
        m_pool.Apply(100);
	}

    private UILayoutGroupItem OnSetItem(int itemindex, int applyindex, UILayoutGroupItemPool pool)
    {
        TestItem item = pool.GetLayoutGroupItem<TestItem>();
        if (item != null)
        {
            item.SetText(applyindex);
        }
        return item;
    }

    private TestItem OnInitItem(GameObject go)
    {
        TestItem element = go.AddComponent<TestItem>();
        element.Init();
        return element;
    }

    private class TestItem : UILayoutGroupItem
    {
        private Text m_text;

        public void Init()
        {
            m_text = transform.Find("Text").GetComponent<Text>();
        }

        public void SetText(int i)
        {
            m_text.text = i.ToString();
        }
    }
}
