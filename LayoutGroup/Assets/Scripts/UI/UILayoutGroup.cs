using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

[DisallowMultipleComponent]
[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public abstract class UILayoutGroup : UIBehaviour, ILayoutElement, ILayoutGroup
{
    [SerializeField]
    protected RectOffset m_padding = new RectOffset();
    [SerializeField]
    protected TextAnchor m_childAlignment = TextAnchor.UpperLeft;

    protected DrivenRectTransformTracker m_tracker;
    protected int m_minIndex;
    protected int m_maxIndex;
    private RectTransform m_rectTransform;
    private Vector2 m_totalMinSize = Vector2.zero;
    private Vector2 m_totalPreferredSize = Vector2.zero;
    private Vector2 m_totalFlexibleSize = Vector2.zero;
    private List<UILayoutGroupElement> m_elementList = new List<UILayoutGroupElement>();
    private static List<UILayoutGroupElementStatic> m_tempStaticElementList = new List<UILayoutGroupElementStatic>();
    private static List<UILayoutGroupElementDynamic> m_tempDynamicElementList = new List<UILayoutGroupElementDynamic>();
    private static List<int> m_tempIntList = new List<int>();

    public RectOffset padding { get { return m_padding; } set { SetProperty(ref m_padding, value); } }
    public TextAnchor childAlignment { get { return m_childAlignment; } set { SetProperty(ref m_childAlignment, value); } }
    public List<UILayoutGroupElement> itemList { get { return m_elementList; } }

    protected RectTransform rectTransform
    {
        get
        {
            if (m_rectTransform == null)
            {
                m_rectTransform = transform as RectTransform;
            }
            return m_rectTransform;
        }
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

    public void RefreshElementList()
    {
        int staticHeadCount = 0;
        int staticTailCount = 0;
        int maxCustomStaticIndex = 0;
        m_elementList.Clear();
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
            UILayoutGroupElementStatic staticElement = element as UILayoutGroupElementStatic;
            if (staticElement != null)
            {
                m_tempStaticElementList.Add(staticElement);
                switch (staticElement.indexType)
                {
                    case UILayoutGroupElementStatic.EIndexType.Custom:
                        if (!m_tempIntList.Contains(staticElement.index))
                        {
                            m_tempIntList.Add(staticElement.index);
                            maxCustomStaticIndex = Mathf.Max(maxCustomStaticIndex, staticElement.index);
                        }
                        break;
                    case UILayoutGroupElementStatic.EIndexType.Head:
                        staticHeadCount++;
                        break;
                    case UILayoutGroupElementStatic.EIndexType.Tail:
                        staticTailCount++;
                        break;
                }
            }
            UILayoutGroupElementDynamic dynamicElement = element as UILayoutGroupElementDynamic;
            if (dynamicElement != null)
            {
                m_tempDynamicElementList.Add(dynamicElement);
            }
            m_minIndex = Mathf.Min(m_minIndex, element.index);
            m_maxIndex = Mathf.Max(m_maxIndex, element.index);
            m_elementList.Add(element);
        }
        int staticHeadRepeatCount = 0;
        for (int i = 0; i < m_tempIntList.Count; ++i)
        {
            if (m_tempIntList[i] < staticHeadCount)
            {
                staticHeadRepeatCount++;
            }
        }
        int dynamicIndex = 0;
        int staticHeadIndex = 0;
        int staticTailIndex = 0;
        for (int i = 0; i < m_elementList.Count; ++i)
        {
            UILayoutGroupElement element = m_elementList[i];
            UILayoutGroupElementDynamic dynamicElement = element as UILayoutGroupElementDynamic;
            if (dynamicElement != null)
            {
                if (Application.isPlaying)
                {
                    dynamicElement.index = staticHeadRepeatCount + staticHeadCount + dynamicIndex;
                }
                continue;
            }
            UILayoutGroupElementStatic staticElement = element as UILayoutGroupElementStatic;
            if (staticElement != null)
            {
                switch (staticElement.indexType)
                {
                    case UILayoutGroupElementStatic.EIndexType.Custom:
                        if (staticElement.index >= staticHeadCount)
                        {
                            dynamicIndex++;
                        }
                        else
                        {
                            staticHeadIndex++;
                        }
                        break;
                    case UILayoutGroupElementStatic.EIndexType.Head:
                        break;
                }
            }
        }
        m_tempIntList.Clear();
    }

    public virtual void CalculateLayoutInputHorizontal()
    {
        m_minIndex = int.MaxValue;
        m_maxIndex = int.MinValue;
        RefreshElementList();
        m_tracker.Clear();
    }

    public abstract void CalculateLayoutInputVertical();
    public virtual float minWidth { get { return GetTotalMinSize(0); } }
    public virtual float preferredWidth { get { return GetTotalPreferredSize(0); } }
    public virtual float flexibleWidth { get { return GetTotalFlexibleSize(0); } }
    public virtual float minHeight { get { return GetTotalMinSize(1); } }
    public virtual float preferredHeight { get { return GetTotalPreferredSize(1); } }
    public virtual float flexibleHeight { get { return GetTotalFlexibleSize(1); } }
    public virtual int layoutPriority { get { return 0; } }
    public abstract void SetLayoutHorizontal();
    public abstract void SetLayoutVertical();

    protected override void OnEnable()
    {
        base.OnEnable();
        SetDirty();
    }

    protected override void OnDisable()
    {
        m_tracker.Clear();
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        base.OnDisable();
    }

    protected override void OnDidApplyAnimationProperties()
    {
        SetDirty();
    }

    protected float GetTotalMinSize(int axis)
    {
        return m_totalMinSize[axis];
    }

    protected float GetTotalPreferredSize(int axis)
    {
        return m_totalPreferredSize[axis];
    }

    protected float GetTotalFlexibleSize(int axis)
    {
        return m_totalFlexibleSize[axis];
    }

    protected float GetStartOffset(int axis, float requiredSpaceWithoutPadding)
    {
        float requiredSpace = requiredSpaceWithoutPadding + (axis == 0 ? padding.horizontal : padding.vertical);
        float availableSpace = rectTransform.rect.size[axis];
        float surplusSpace = availableSpace - requiredSpace;
        float alignmentOnAxis = 0;
        if (axis == 0)
            alignmentOnAxis = ((int)childAlignment % 3) * 0.5f;
        else
            alignmentOnAxis = ((int)childAlignment / 3) * 0.5f;
        return (axis == 0 ? padding.left : padding.top) + surplusSpace * alignmentOnAxis;
    }

    protected void SetLayoutInputForAxis(float totalMin, float totalPreferred, float totalFlexible, int axis)
    {
        m_totalMinSize[axis] = totalMin;
        m_totalPreferredSize[axis] = totalPreferred;
        m_totalFlexibleSize[axis] = totalFlexible;
    }

    protected void SetChildAlongAxis(RectTransform rect, int axis, float pos, float size)
    {
        if (rect == null)
            return;

        m_tracker.Add(this, rect,
            DrivenTransformProperties.Anchors |
            DrivenTransformProperties.AnchoredPosition |
            DrivenTransformProperties.SizeDelta);

        rect.SetInsetAndSizeFromParentEdge(axis == 0 ? RectTransform.Edge.Left : RectTransform.Edge.Top, pos, size);
    }

    private bool isRootLayoutGroup
    {
        get
        {
            Transform parent = transform.parent;
            if (parent == null)
                return true;
            return transform.parent.GetComponent(typeof(ILayoutGroup)) == null;
        }
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        if (isRootLayoutGroup)
            SetDirty();
    }

    protected virtual void OnTransformChildrenChanged()
    {
        SetDirty();
    }

    protected void SetProperty<T>(ref T currentValue, T newValue)
    {
        if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
            return;
        currentValue = newValue;
        SetDirty();
    }

    protected void SetDirty()
    {
        if (!IsActive())
            return;
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        SetDirty();
    }
#endif
}