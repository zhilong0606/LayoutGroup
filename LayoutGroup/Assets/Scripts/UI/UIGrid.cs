using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGrid : UILayoutGroup
{
    public enum Corner { UpperLeft = 0, UpperRight = 1, LowerLeft = 2, LowerRight = 3 }
    public enum Axis { Horizontal = 0, Vertical = 1 }
    public enum Constraint { Flexible = 0, FixedColumnCount = 1, FixedRowCount = 2 }

    [SerializeField]
    protected Corner m_startCorner = Corner.UpperLeft;
    [SerializeField]
    protected Axis m_startAxis = Axis.Horizontal;
    [SerializeField]
    protected Vector2 m_cellSize = new Vector2(100, 100);
    [SerializeField]
    protected Vector2 m_spacing = Vector2.zero;
    [SerializeField]
    protected Constraint m_constraint = Constraint.Flexible;
    [SerializeField]
    protected int m_constraintCount = 2;

    public Corner startCorner { get { return m_startCorner; } set { SetProperty(ref m_startCorner, value); } }
    public Axis startAxis { get { return m_startAxis; } set { SetProperty(ref m_startAxis, value); } }
    public Vector2 cellSize { get { return m_cellSize; } set { SetProperty(ref m_cellSize, value); } }
    public Vector2 spacing { get { return m_spacing; } set { SetProperty(ref m_spacing, value); } }
    public Constraint constraint { get { return m_constraint; } set { SetProperty(ref m_constraint, value); } }
    public int constraintCount { get { return m_constraintCount; } set { SetProperty(ref m_constraintCount, Mathf.Max(1, value)); } }
    
#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        constraintCount = constraintCount;
    }
#endif

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();

        int itemCount = m_maxIndex;
        int minColumns = 0;
        int preferredColumns = 0;
        if (m_constraint == Constraint.FixedColumnCount)
        {
            minColumns = preferredColumns = m_constraintCount;
        }
        else if (m_constraint == Constraint.FixedRowCount)
        {
            minColumns = preferredColumns = Mathf.CeilToInt(itemCount / (float)m_constraintCount - 0.001f);
        }
        else
        {
            minColumns = 1;
            preferredColumns = Mathf.CeilToInt(Mathf.Sqrt(itemCount));
        }

        SetLayoutInputForAxis(
            padding.horizontal + (cellSize.x + spacing.x) * minColumns - spacing.x,
            padding.horizontal + (cellSize.x + spacing.x) * preferredColumns - spacing.x,
            -1, 0);
    }

    public override void CalculateLayoutInputVertical()
    {
        int itemCount = m_maxIndex;
        int minRows = 0;
        if (m_constraint == Constraint.FixedColumnCount)
        {
            minRows = Mathf.CeilToInt(itemCount / (float)m_constraintCount - 0.001f);
        }
        else if (m_constraint == Constraint.FixedRowCount)
        {
            minRows = m_constraintCount;
        }
        else
        {
            float width = rectTransform.rect.size.x;
            int cellCountX = Mathf.Max(1, Mathf.FloorToInt((width - padding.horizontal + spacing.x + 0.001f) / (cellSize.x + spacing.x)));
            minRows = Mathf.CeilToInt(itemCount / (float)cellCountX);
        }

        float minSpace = padding.vertical + (cellSize.y + spacing.y) * minRows - spacing.y;
        SetLayoutInputForAxis(minSpace, minSpace, -1, 1);
    }

    public override void SetLayoutHorizontal()
    {
        SetCellsAlongAxis(0);
    }

    public override void SetLayoutVertical()
    {
        SetCellsAlongAxis(1);
    }

    private void SetCellsAlongAxis(int axis)
    {
        if (axis == 0)
        {
            for (int i = 0; i < itemList.Count; i++)
            {
                UILayoutGroupElement element = itemList[i];
                RectTransform rect = element.transform as RectTransform;

                m_tracker.Add(this, rect,
                    DrivenTransformProperties.Anchors |
                    DrivenTransformProperties.AnchoredPosition |
                    DrivenTransformProperties.SizeDelta);

                rect.anchorMin = Vector2.up;
                rect.anchorMax = Vector2.up;
                rect.sizeDelta = cellSize;
            }
            return;
        }

        float width = rectTransform.rect.size.x;
        float height = rectTransform.rect.size.y;

        int virtualItemCount = m_maxIndex;
        int cellCountX = 1;
        int cellCountY = 1;
        if (m_constraint == Constraint.FixedColumnCount)
        {
            cellCountX = m_constraintCount;
            cellCountY = Mathf.CeilToInt(virtualItemCount / (float)cellCountX - 0.001f);
        }
        else if (m_constraint == Constraint.FixedRowCount)
        {
            cellCountY = m_constraintCount;
            cellCountX = Mathf.CeilToInt(virtualItemCount / (float)cellCountY - 0.001f);
        }
        else
        {
            if (cellSize.x + spacing.x <= 0)
                cellCountX = int.MaxValue;
            else
                cellCountX = Mathf.Max(1, Mathf.FloorToInt((width - padding.horizontal + spacing.x + 0.001f) / (cellSize.x + spacing.x)));

            if (cellSize.y + spacing.y <= 0)
                cellCountY = int.MaxValue;
            else
                cellCountY = Mathf.Max(1, Mathf.FloorToInt((height - padding.vertical + spacing.y + 0.001f) / (cellSize.y + spacing.y)));
        }

        int cornerX = (int)startCorner % 2;
        int cornerY = (int)startCorner / 2;

        int cellsPerMainAxis, actualCellCountX, actualCellCountY;
        if (startAxis == Axis.Horizontal)
        {
            cellsPerMainAxis = cellCountX;
            actualCellCountX = Mathf.Clamp(cellCountX, 1, virtualItemCount);
            actualCellCountY = Mathf.Clamp(cellCountY, 1, Mathf.CeilToInt(virtualItemCount / (float)cellsPerMainAxis));
        }
        else
        {
            cellsPerMainAxis = cellCountY;
            actualCellCountY = Mathf.Clamp(cellCountY, 1, virtualItemCount);
            actualCellCountX = Mathf.Clamp(cellCountX, 1, Mathf.CeilToInt(virtualItemCount / (float)cellsPerMainAxis));
        }

        Vector2 requiredSpace = new Vector2(
                actualCellCountX * cellSize.x + (actualCellCountX - 1) * spacing.x,
                actualCellCountY * cellSize.y + (actualCellCountY - 1) * spacing.y
                );
        Vector2 startOffset = new Vector2(
                GetStartOffset(0, requiredSpace.x),
                GetStartOffset(1, requiredSpace.y)
                );

        for (int i = 0; i < itemList.Count; i++)
        {
            UILayoutGroupElement element = itemList[i];
            int index = element.index;
            int positionX;
            int positionY;
            if (startAxis == Axis.Horizontal)
            {
                positionX = index % cellsPerMainAxis;
                positionY = index / cellsPerMainAxis;
            }
            else
            {
                positionX = index / cellsPerMainAxis;
                positionY = index % cellsPerMainAxis;
            }

            if (cornerX == 1)
                positionX = actualCellCountX - 1 - positionX;
            if (cornerY == 1)
                positionY = actualCellCountY - 1 - positionY;

            SetChildAlongAxis(itemList[i].transform as RectTransform, 0, startOffset.x + (cellSize[0] + spacing[0]) * positionX, cellSize[0]);
            SetChildAlongAxis(itemList[i].transform as RectTransform, 1, startOffset.y + (cellSize[1] + spacing[1]) * positionY, cellSize[1]);
        }
    }
}
