using System;
using UnityEngine;

namespace _02.Scripts.Roulette
{
    public class RouletteData
    {
        private string _text = string.Empty;
        private int _weight = 0;
        private Color _color = Color.clear;

        public Action OnChangedText;
        public Action OnChangedWeight;
        public Action OnChangedColor;
        
        public string ThisText
        {
            get => _text;
            set
            {
                _text = value;
                OnChangedText?.Invoke();
            }
        }

        public int ThisWeight
        {
            get => _weight;
            set
            {
                _weight = value;
                OnChangedWeight?.Invoke();;
            }
        }

        public Color ThisColor
        {
            get => _color;
            set
            {
                _color = value;
                OnChangedColor?.Invoke();
            }
        }
    }
}