using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILayoutGroupElementDynamic : UILayoutGroupElement
{
    private int m_prefabIndex = -1;

    public int prefabIndex
    {
        get { return m_prefabIndex; }
        set { m_prefabIndex = value; }
    }
}
