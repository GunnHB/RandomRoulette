using UnityEngine;

using System;

using _02.Scripts.Result;
using _02.Scripts.Roulette;

using DG.Tweening;
using Sirenix.OdinInspector;
using DarkTonic.MasterAudio;

namespace _02.Scripts.Manager
{
    public class GameManager : Singleton<GameManager>
    {
        private ResultPanel _resultPanel = null;
        
        private static bool _bIsSpinning = false;

        public Action<bool> OnRequestSpinning;

        private void Start()
        {
            if (_resultPanel != null)
                _resultPanel.OnRequestClosePanel += DeactivateResult;
        }

        public void SetupResultPanel(ResultPanel panel)
        {
            if (panel != null)
                _resultPanel = panel;
        }

        public bool IsSpinning
        {
            get => _bIsSpinning;
            set
            { 
                _bIsSpinning = value;
                
                OnRequestSpinning?.Invoke(_bIsSpinning);
            }
        }
        
        public void ActivateResult(RouletteData data)
        {
            if (data == null)
                return;
            
            _resultPanel.SetupPanel(data);

            Sequence sequence = DOTween.Sequence()
                .OnStart(() =>
                {
                    _resultPanel.Popup.transform.localScale = Vector3.zero;
                    _resultPanel.Popup.GetComponent<CanvasGroup>().alpha = 0;

                    _resultPanel.gameObject.SetActive(true);
                })
                .Append(_resultPanel.Popup.transform.DOScale(1f, .15f).SetEase(Ease.OutBounce))
                .Join(_resultPanel.Popup.GetComponent<CanvasGroup>().DOFade(1f, .15f))
                .JoinCallback(() =>
                {
                    MasterAudio.PlaySound("Ta-da");
                });
        }

        private void DeactivateResult()
        {
            Sequence sequence = DOTween.Sequence()
                .Append(_resultPanel.Popup.transform.DOScale(0f, .15f).SetEase(Ease.InBounce))
                .Join(_resultPanel.Popup.GetComponent<CanvasGroup>().DOFade(0f, .15f))
                .OnComplete(() =>
                {
                    _resultPanel.gameObject.SetActive(false);
                });
        }
    }
}
