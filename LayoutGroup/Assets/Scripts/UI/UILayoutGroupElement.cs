using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public abstract class UILayoutGroupElement : UIBehaviour
{
    [SerializeField]
    private int m_index;

    public int index
    {
        get { return m_index; }
        set
        {
            if (value != m_index)
            {
                m_index = value;
                SetDirty();
            }
        }
    }
    
#if false
    protected override void OnEnable()
    {
        base.OnEnable();
        SetDirty();
    }

    protected override void OnTransformParentChanged()
    {
        SetDirty();
    }

    protected override void OnDisable()
    {
        SetDirty();
        base.OnDisable();
    }

    protected override void OnDidApplyAnimationProperties()
    {
        SetDirty();
    }

    protected override void OnBeforeTransformParentChanged()
    {
        SetDirty();
    }
#endif

    public void SetDirty()
    {
        if (isActiveAndEnabled)
        {
            LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        SetDirty();
    }

#endif
}
