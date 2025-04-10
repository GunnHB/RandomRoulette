using System;
using _02.Scripts.Common;
using _02.Scripts.Roulette;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace _02.Scripts.Result
{
    public class ResultPanel : MonoBehaviour
    {
        [Title("Result")]
        [SerializeField] private GameObject _popup = null;
        [SerializeField] private TMP_Text _text = null;
        [SerializeField] private UIButton _closeButton = null;

        public Action OnRequestClosePanel;

        public GameObject Popup => _popup;

        public void SetupPanel(RouletteData data)
        {
            if (data == null)
                return;
            
            _text.SetText(data.ThisText);
        }
        
        private void Start()
        {
            BindEvent();
        }

        private void BindEvent()
        {
            if (_closeButton == null)
                return;
            
            _closeButton.onClick.AddListener(ClosePanel);
        }

        private void ClosePanel()
        {
            OnRequestClosePanel?.Invoke();
        }
    }
}