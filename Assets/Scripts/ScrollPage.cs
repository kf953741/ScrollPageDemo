using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using Boo.Lang.Environments;

public class ScrollPage : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    ScrollRect rect;

    //页面：0，1，2，3  索引从0开始
    //每页占的比列：0/3=0  1/3=0.333  2/3=0.6666 3/3=1
    List<float> _pagesPercent = new List<float>();
    [HideInInspector]
    public int CurrentPageIndex = -1;

    public float SmoothSpeed = 4;
    public int Height = 300;
    public int Width = 300;
    [HideInInspector]
    public int PageCount = 0;
    public Action PageChangeAction;

    float targethorizontal = 0;

    bool isDrag = false;

    public System.Action<int, int> OnPageChanged;

    float startime = 0f;
    float delay = 0.1f;

    // Use this for initialization
    void Start()
    {
        rect = transform.GetComponent<ScrollRect>();
        startime = Time.time;
        InitLayout();
        UpdatePagesPercent();
    }

    void Update()
    {
        if (Time.time < startime + delay) return;
        UpdatePagesPercent();
        if (!isDrag && _pagesPercent.Count > 0)
        {
            rect.horizontalNormalizedPosition = Mathf.Lerp(rect.horizontalNormalizedPosition, targethorizontal,
                Time.deltaTime * SmoothSpeed);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDrag = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDrag = false;

        float posX = rect.horizontalNormalizedPosition;
        int index = 0;
        //假设离第一位最近
        float nearestDis = Mathf.Abs(_pagesPercent[index] - posX);
        for (int i = 1; i < _pagesPercent.Count; i++)
        {
            float temp = Mathf.Abs(_pagesPercent[i] - posX);
            if (temp < nearestDis)
            {
                index = i;

                nearestDis = temp;
            }
        }
        ChangePage(index);
    }

    public void ChangePage(int pageNum)
    {
        if (pageNum != CurrentPageIndex)
        {
            if (pageNum > _pagesPercent.Count - 1)
            {
                pageNum = _pagesPercent.Count - 1;
            }
            if (pageNum < 0)
            {
                pageNum = 0;
            }
            CurrentPageIndex = pageNum;
            OnPageChanged(_pagesPercent.Count, CurrentPageIndex);
            isDrag = false;
        }

        targethorizontal = _pagesPercent[pageNum];
        if (PageChangeAction!=null)
        {
            PageChangeAction();
        }
    }

    private void InitLayout()
    {
        PageCount = this.rect.content.childCount;
        for (int i = 0; i < PageCount; i++)
        {
            GameObject item = this.rect.content.GetChild(i).gameObject;
            if (item.activeSelf)
            {
                LayoutElement layout = item.GetComponent<LayoutElement>();
                if (layout == null)
                {
                    layout = item.AddComponent<LayoutElement>();
                    layout.preferredWidth = Width;
                    layout.preferredHeight = Height;
                }
            }
        }
    }

    void UpdatePagesPercent()
    {
        int count = this.rect.content.childCount;
        int temp = 0;
        for (int i = 0; i < count; i++)
        {
            GameObject item = this.rect.content.GetChild(i).gameObject;
            if (item.activeSelf)
            {
                temp++;
            }
        }
        count = temp;

        if (_pagesPercent.Count != count)
        {
            if (count != 0)
            {
                _pagesPercent.Clear();
                for (int i = 0; i < count; i++)
                {
                    float page = 0;
                    if (count != 1)
                        page = i / ((float) (count - 1));
                    _pagesPercent.Add(page);
                }
            }
            OnEndDrag(null);
        }
    }
}