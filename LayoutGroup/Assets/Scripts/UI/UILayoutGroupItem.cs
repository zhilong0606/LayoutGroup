using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UILayoutGroupElementDynamic))]
public class UILayoutGroupItem : MonoBehaviour
{
    private UILayoutGroupElementDynamic m_element;

    public UILayoutGroupElementDynamic element
    {
        get { return m_element; }
    }

    public void InitElement()
    {
        m_element = GetComponent<UILayoutGroupElementDynamic>();
    }
}
