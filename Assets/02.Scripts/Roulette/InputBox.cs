using System;
using _02.Scripts.Common;
using _02.Scripts.Manager;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _02.Scripts.Roulette
{
    public class InputBox : MonoBehaviour
    {
        [Title("InputBox")]
        [SerializeField] private TMP_InputField _inputField = null;
        [SerializeField] private Button _upButton = null;
        [SerializeField] private Button _downButton = null;
        
        [Title("Common")]
        [SerializeField] private TMP_Text _numText = null;
        [SerializeField] private Image _label = null;
        [SerializeField] private Button _removeButton = null;
        
        private RouletteData _rouletteData = null;

        public Action<InputBox> OnRemoveAction;

        private void Start()
        {
            if(_upButton != null)
                _upButton.onClick.AddListener(OnClickUpButton);
            
            if(_downButton != null)
                _downButton.onClick.AddListener(OnClickDownButton);
            
            if(_removeButton != null)
                _removeButton.onClick.AddListener(OnClickRemoveButton);

            GameManager.Instance.OnRequestSpinning += OnSpinning;
        }

        private void OnDestroy()
        {
            GameManager.Instance.OnRequestSpinning -= OnSpinning;
        }

        public Button RemoveButton => _removeButton;

        public void SetLabel(Color color)
        {
            if (_label == null)
                return;

            _label.color = color;
        }

        public void Setup(RouletteData data)
        {
            if (data == null)
                return;

            _rouletteData = data;

            _rouletteData.OnChangedColor += UpdateLabelColor;
            _rouletteData.OnChangedWeight += UpdateWeight;
            
            _inputField.onValueChanged.AddListener(OnValueChangedInputField);
        }

        private void UpdateLabelColor()
        {
            if (_rouletteData == null)
                return;

            _label.color = _rouletteData.ThisColor;
        }

        private void UpdateWeight()
        {
            if (_rouletteData == null)
                return;
            
            _numText.SetText(_rouletteData.ThisWeight.ToString());
        }
        
        private void OnValueChangedInputField(string value)
        {
            if (_rouletteData == null)
                return;
            
            _rouletteData.ThisText = value;
        }

        private void OnClickUpButton()
        {
            if (GameManager.Instance.IsSpinning)
                return;
            
            if (_rouletteData == null)
                return;
            
            _rouletteData.ThisWeight += 1;
            _numText.SetText(_rouletteData.ThisWeight.ToString());
        }

        private void OnClickDownButton()
        {
            if (GameManager.Instance.IsSpinning)
                return;
            
            if (_rouletteData == null)
                return;

            if (_rouletteData.ThisWeight == 1)
                return;

            _rouletteData.ThisWeight -= 1;
            _numText.SetText(_rouletteData.ThisWeight.ToString());
        }

        private void OnClickRemoveButton()
        {
            OnRemoveAction?.Invoke(this);
        }

        private void OnSpinning(bool value)
        {
            _inputField.interactable = !value;
            _removeButton.interactable = !value;
        }
    }
}
