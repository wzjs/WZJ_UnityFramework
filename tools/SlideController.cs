using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// scrollRect 的扩张,轮播图的功能， 能自动定位舞台上的图片并放大
/// </summary>
namespace WZJ_UnityFrameWork
{
    public class SlideController : ScrollRect
    {

        enum Dicrct
        {
            left,
            right
        }

        //所有图片
        public List<RectTransform> lists;
        //舞台上的图片
        public RectTransform target;
        Dicrct dit;
        private HorizontalLayoutGroup _selfLayout;


        private float lastFramePs = 0;

        private float offset = 0.5f;

        private const int IMAGE_WIDTH = 1200;

        // if go sliding
        private bool m_sliding = false;
        private bool beginDrag = false;

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            beginDrag = true;
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            beginDrag = false;
            m_sliding = true;
        }

        private void Update()
        {
            //手不移动的话不算做滑动
            //slide duration
            if (m_sliding)
            {
                float off = content.anchoredPosition3D.x - lastFramePs;
                //对齐
                Align(off);
                //放大
                LargeImage();
            }
        }

        private void Align(float off)
        {
            dit = off > 0 ? Dicrct.right : Dicrct.left;
            int num = (int)Mathf.Abs(content.anchoredPosition3D.x) / IMAGE_WIDTH + 1;
            float distance = num * IMAGE_WIDTH + (num - 1) * _selfLayout.spacing;
            float before = (num - 1) * _selfLayout.spacing + (num - 1) * IMAGE_WIDTH;

            if (Mathf.Abs(off) <= offset && Mathf.Abs(off) != 0)
            {
                //越过几个广告
                if (Mathf.Abs(content.anchoredPosition3D.x) - before > distance - Mathf.Abs(content.anchoredPosition3D.x))
                {
                    //左大右小
                    content.DOAnchorPos3D(new Vector2(-(distance + _selfLayout.spacing), content.anchoredPosition3D.y), 0.5f)
                        .OnUpdate(() => { if (beginDrag) { content.DOKill(); } });
                }
                else
                {
                    content.DOAnchorPos3D(new Vector2(-before, content.anchoredPosition3D.y), 0.5f)
                       .OnUpdate(() => { if (beginDrag) { content.DOKill(); } });
                }
            }
            m_sliding = false;
        }

        private void LargeImage()
        {
            for (int i = 0; i < lists.Count; i++)
            {
                if (lists[i].position.x >= (viewport.position.x - viewport.rect.width / 2)
                    && lists[i].position.x <= (viewport.position.x + viewport.rect.width / 2))
                {
                    if (lists[i] != target)
                    {
                        var flag = target;
                        flag.DOKill();
                        flag.DOScale(1, 1);
                        target = lists[i];
                        target.DOKill();
                        target.DOScale(1.2f, 1.2f);
                    }
                }
            }
        }
    }

}
