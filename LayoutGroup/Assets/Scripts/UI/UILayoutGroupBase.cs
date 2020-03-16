using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

[DisallowMultipleComponent]
[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public abstract class UILayoutGroupBase : UIBehaviour, ILayoutElement, ILayoutGroup
{
    [SerializeField]
    protected RectOffset m_padding = new RectOffset();
    [SerializeField]
    protected TextAnchor m_childAlignment = TextAnchor.UpperLeft;

    private RectTransform m_rectTransform;
    private Vector2 m_totalMinSize = Vector2.zero;
    private Vector2 m_totalPreferredSize = Vector2.zero;
    private Vector2 m_totalFlexibleSize = Vector2.zero;

    protected DrivenRectTransformTracker m_tracker;

    public RectOffset padding { get { return m_padding; } set { SetProperty(ref m_padding, value); } }
    public TextAnchor childAlignment { get { return m_childAlignment; } set { SetProperty(ref m_childAlignment, value); } }

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

    public virtual void CalculateLayoutInputHorizontal()
    {
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