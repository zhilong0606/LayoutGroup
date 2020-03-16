using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

[DisallowMultipleComponent]
[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public abstract class UILayoutGroup : UILayoutGroupBase
{
    protected int m_minIndex;
    protected int m_maxIndex;
    protected int m_staticHeadCount;
    protected int m_staticTailCount;

    private List<UILayoutGroupElement> m_elementList = new List<UILayoutGroupElement>();
    private List<UILayoutGroupElementStatic> m_staticElementList = new List<UILayoutGroupElementStatic>();
    private List<UILayoutGroupElementDynamic> m_dynamicElementList = new List<UILayoutGroupElementDynamic>();
    private List<int> m_usedIdList = new List<int>();
    private Func<bool> m_funcOnCheckRefreshLayoutGroup;
    private Action<int> m_actionOnRefreshDenamic;

    public List<UILayoutGroupElement> elementList { get { return m_elementList; } }

    public void RegisterCheckRefreshLayoutGroup(Func<bool> funcOnCheckRefreshLayoutGroup)
    {
        m_funcOnCheckRefreshLayoutGroup = funcOnCheckRefreshLayoutGroup;
    }

    public void RegisterRefreshDenamic(Action<int> actionOnRefreshDenamic)
    {
        m_actionOnRefreshDenamic = actionOnRefreshDenamic;
    }

    public bool ContainsStaticIndex(int index)
    {
        for (int i = 0; i < m_elementList.Count; ++i)
        {
            UILayoutGroupElementStatic staticElement = m_elementList[i] as UILayoutGroupElementStatic;
            if (staticElement != null && staticElement.index == index)
            {
                return true;
            }
        }
        return false;
    }

    public void RefreshElementList(bool force = false)
    {
        m_usedIdList.Clear();
        if (!force && m_funcOnCheckRefreshLayoutGroup != null && !m_funcOnCheckRefreshLayoutGroup())
        {
            return;
        }
        int indexCursor;
        CollectElements();
        RefreshCustomStaticElements();
        RefreshHeadStaticElements(out indexCursor);
        if (m_actionOnRefreshDenamic != null)
        {
            m_actionOnRefreshDenamic(indexCursor);
        }
        else
        {
            RefreshDynamicElements(indexCursor);
        }
        RefreshTailStaticElements();
    }

    private void CollectElements()
    {

        m_elementList.Clear();
        m_staticElementList.Clear();
        m_dynamicElementList.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = rectTransform.GetChild(i);
            if (!child.gameObject.activeInHierarchy)
            {
                continue;
            }
            UILayoutGroupElement element = child.GetComponent<UILayoutGroupElement>();
            if (element == null)
            {
                continue;
            }
            m_elementList.Add(element);
            UILayoutGroupElementStatic staticElement = element as UILayoutGroupElementStatic;
            if (staticElement != null)
            {
                m_staticElementList.Add(staticElement);
            }
            UILayoutGroupElementDynamic dynamicElement = element as UILayoutGroupElementDynamic;
            if (dynamicElement != null)
            {
                m_dynamicElementList.Add(dynamicElement);
            }
        }
    }

    private void RefreshCustomStaticElements()
    {
        for (int i = 0; i < m_staticElementList.Count; ++i)
        {
            UILayoutGroupElementStatic staticElement = m_staticElementList[i];
            if (staticElement.indexType == UILayoutGroupElementStatic.EIndexType.Custom)
            {
                int index = staticElement.index;
                RecordIndex(index);
            }
        }
    }

    private void RefreshHeadStaticElements(out int indexCursor)
    {
        indexCursor = 0;
        for (int i = 0; i < m_staticElementList.Count; ++i)
        {
            UILayoutGroupElementStatic staticElement = m_staticElementList[i];
            if (staticElement.indexType == UILayoutGroupElementStatic.EIndexType.Head)
            {
                indexCursor = GetUnUsedIndex(indexCursor);
                int index = indexCursor;
                indexCursor++;

                staticElement.index = index;
                RecordIndex(index);
            }
        }
    }

    private void RefreshTailStaticElements()
    {
        int indexCursor = m_maxIndex + 1;
        for (int i = 0; i < m_staticElementList.Count; ++i)
        {
            UILayoutGroupElementStatic staticElement = m_staticElementList[i];
            if (staticElement.indexType == UILayoutGroupElementStatic.EIndexType.Tail)
            {
                indexCursor = GetUnUsedIndex(indexCursor);
                int index = indexCursor;
                indexCursor++;

                staticElement.index = indexCursor;
                RecordIndex(index);
            }
        }
    }

    private void RefreshDynamicElements(int indexCursor)
    {
        for (int i = 0; i < m_dynamicElementList.Count; ++i)
        {
            UILayoutGroupElementDynamic dynamicElement = m_dynamicElementList[i];
            indexCursor = GetUnUsedIndex(indexCursor);
            int index = indexCursor;
            indexCursor++;

            dynamicElement.index = index;
            RecordIndex(index);
        }
    }

    public int GetUnUsedIndex(int index)
    {
        while (m_usedIdList.Contains(index))
        {
            index++;
        }
        return index;
    }

    public void RecordIndex(int index)
    {
        m_usedIdList.Add(index);
        m_minIndex = Mathf.Min(index);
        m_maxIndex = Mathf.Max(index);
    }

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();
        RefreshElementList();
    }
}