using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILayoutGroupElementStatic : UILayoutGroupElement
{
    [SerializeField]
    protected EIndexType m_indexType = EIndexType.Head;

    public EIndexType indexType
    {
        get { return m_indexType; }
    }

    public enum EIndexType
    {
        Head,
        Tail,
        Custom,
    }
}
