using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _02.Scripts.Common
{
    public class UIButton : Button
    {
        public override void OnPointerEnter(PointerEventData eventData)
        {
            transform.DOScale(1.12f, .1f).SetEase(Ease.InOutSine);
        }
        
        public override void OnPointerExit(PointerEventData eventData)
        {
            transform.DOScale(1f, .1f).SetEase(Ease.InOutSine);
        }
    }
}