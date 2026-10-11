using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HoverSelectable2D : MonoBehaviour
{
    public UnityEvent onSelected;

    private HoverOutlineFromCollider2D[] outlines;

    void Awake()
    {
        outlines =
            GetComponentsInChildren<
                HoverOutlineFromCollider2D
            >(true);

        SetHovered(false);
    }

    public void SetHovered(bool isHovered)
    {
        for (int i = 0; i < outlines.Length; i++)
        {
            if (outlines[i] != null)
            {
                outlines[i].SetVisible(isHovered);
            }
        }
    }

    public void Select()
    {
        onSelected?.Invoke();
    }
}