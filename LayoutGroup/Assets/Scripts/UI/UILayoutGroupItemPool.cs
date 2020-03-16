using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILayoutGroupItemPool : MonoBehaviour
{
    public delegate UILayoutGroupItem FuncOnSetLayoutGroupItem(int itemIndex, int applyIndex, UILayoutGroupItemPool pool);
    public delegate T FuncOnItemInitialized<T>(GameObject go) where T : UILayoutGroupItem;

    [SerializeField]
    private List<UILayoutGroupElementDynamic> m_dynamicPrefabList = new List<UILayoutGroupElementDynamic>();
    [SerializeField]
    private UILayoutGroup m_layoutGroup;
    [SerializeField]
    private UILayoutGroupViewport m_viewport;
    
    private Dictionary<Type, ItemInitializer> m_itemInitializerMap = new Dictionary<Type, ItemInitializer>();
    private List<UILayoutGroupItem> m_usingItemList = new List<UILayoutGroupItem>();
    private List<UILayoutGroupItem> m_spareItemList = new List<UILayoutGroupItem>();
    private List<UILayoutGroupItem> m_releasedItemList = new List<UILayoutGroupItem>();
    private FuncOnSetLayoutGroupItem m_funcOnSetLayoutGroupItem;
    private bool m_needPrefabHide = false;

    public void Init(FuncOnSetLayoutGroupItem funcOnSetLayoutGroupItem)
    {
        m_funcOnSetLayoutGroupItem = funcOnSetLayoutGroupItem;
        HideAllPrefab();
    }

    public void RegisterItemInitializer<T>(FuncOnItemInitialized<T> funcOnInitLayoutGroupItem) where T : UILayoutGroupItem
    {
        ItemInitializer initializer = new ItemInitializer<T>(funcOnInitLayoutGroupItem);
        m_itemInitializerMap.Add(typeof(T), initializer);
    }
    
    public void Apply(int count)
    {
        ReleaseAllDynamicItem();
        m_layoutGroup.RefreshElementList();
        for (int i = 0, index = 0; i < count; ++i)
        {
            while (m_layoutGroup.ContainsStaticIndex(index))
            {
                index++;
            }
            UILayoutGroupItem item = SetLayoutGroupItem(index, i);
            if (item != null)
            {
                item.element.index = index;
            }
            index++;
        }
    }

    public T GetLayoutGroupItem<T>(int prefabIndex = 0) where T : UILayoutGroupItem
    {
        T item = DrawOutFromList<T>(m_releasedItemList, prefabIndex);
        if (item == null)
        {
            item = DrawOutFromList<T>(m_spareItemList, prefabIndex);
        }
        if (item == null)
        {
            UILayoutGroupElementDynamic prefab = GetDynamicPrefab(prefabIndex);
            if (prefab == null)
            {
                return null;
            }
            if (!prefab.gameObject.activeSelf)
            {
                prefab.gameObject.SetActive(true);
                m_needPrefabHide = true;
            }
            GameObject go = Instantiate(prefab.gameObject, m_layoutGroup.transform);
            UILayoutGroupElementDynamic element = go.GetComponent<UILayoutGroupElementDynamic>();
            element.prefabIndex = prefabIndex;
            ItemInitializer itemInitializer;
            if (m_itemInitializerMap.TryGetValue(typeof(T), out itemInitializer))
            {
                item = itemInitializer.Initialize(go) as T;
                item.InitElement();
            }
        }
        m_usingItemList.Add(item);
        return item;
    }

    private void ReleaseAllDynamicItem()
    {
        m_releasedItemList.AddRange(m_usingItemList);
        m_usingItemList.Clear();
    }

    private T DrawOutFromList<T>(List<UILayoutGroupItem> list, int prefabIndex) where T : UILayoutGroupItem
    {
        for (int i = 0; i < list.Count; ++i)
        {
            UILayoutGroupItem item = list[i];
            if (item.element.prefabIndex != prefabIndex)
            {
                continue;
            }
            T tItem = item as T;
            if (tItem != null)
            {
                list.RemoveAt(i);
                return tItem;
            }
        }
        return null;
    }

    private UILayoutGroupItem SetLayoutGroupItem(int itemIndex, int applyIndex)
    {
        if (m_funcOnSetLayoutGroupItem != null)
        {
            return m_funcOnSetLayoutGroupItem(itemIndex, applyIndex, this);
        }
        return null;
    }

    private void HideAllPrefab()
    {
        for (int i = 0; i < m_dynamicPrefabList.Count; ++i)
        {
            UILayoutGroupElementDynamic prefab = m_dynamicPrefabList[i];
            if (prefab != null && prefab.gameObject.activeSelf)
            {
                prefab.gameObject.SetActive(false);
            }
        }
    }

    private UILayoutGroupElementDynamic GetDynamicPrefab(int prefabIndex)
    {
        if (prefabIndex >= 0 || prefabIndex < m_dynamicPrefabList.Count)
        {
            return m_dynamicPrefabList[prefabIndex];
        }
        return null;
    }

    private void Update()
    {
        for (int i = 0; i < m_releasedItemList.Count; ++i)
        {
            UILayoutGroupItem item = m_releasedItemList[i];
            item.gameObject.SetActive(false);
        }
        m_spareItemList.AddRange(m_releasedItemList);
        m_releasedItemList.Clear();
        if (m_needPrefabHide)
        {
            m_needPrefabHide = false;
            HideAllPrefab();
        }
    }

    public class ItemInitializer<T> : ItemInitializer where T : UILayoutGroupItem
    {
        private FuncOnItemInitialized<T> m_funcOnItemInitialized;

        public ItemInitializer(FuncOnItemInitialized<T> funcOnItemInitialized)
        {
            m_funcOnItemInitialized = funcOnItemInitialized;
        }

        public override UILayoutGroupItem Initialize(GameObject go)
        {
            if (m_funcOnItemInitialized != null)
            {
                return m_funcOnItemInitialized(go);
            }
            return null;
        }
    }

    public abstract class ItemInitializer
    {
        public abstract UILayoutGroupItem Initialize(GameObject go);
    }
}
