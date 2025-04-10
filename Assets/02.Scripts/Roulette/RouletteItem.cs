using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _02.Scripts.Roulette
{
    public class RouletteItem : MonoBehaviour
    {
        [Title("PI")]
        [SerializeField] private Image _piImage = null;
        [SerializeField] private TMP_Text _piText = null;
        
        private RouletteData _rouletteData = null;

        public void Setup(RouletteData data)
        {
            if (data == null)
                return;
            
            _rouletteData = data;
            _rouletteData.OnChangedText += UpdatePiText;
            _rouletteData.OnChangedColor += UpdatePiColor;
            
            _piImage.color = data.ThisColor;
        }

        public void SetFill(float remainingWeight, float totalWeight)
        {
            if (_rouletteData == null || _piImage == null)
                return;

            _piImage.fillAmount = remainingWeight / totalWeight;
        }
        
        public void UpdateColor(Color newColor)
        {
            _piImage.color = newColor;
        }

        private void UpdatePiText()
        {
            if (_rouletteData == null)
                return;

            _piText.SetText(_rouletteData.ThisText);
        }

        private void UpdatePiColor()
        {
            if (_rouletteData == null)
                return;
            
            _piImage.color = _rouletteData.ThisColor;
        }
        
        public void UpdateTextPosition(float cumulativeFill, float currentFill, float radius)
        {
            if (_piText == null) return;

            // 기준 각도: fillOrigin = Top = 90도
            float baseAngle = 90f;
            float centerAngleDeg = baseAngle - (cumulativeFill + currentFill / 2f) * 360f;
            float angleRad = centerAngleDeg * Mathf.Deg2Rad;

            // 위치 계산 (원 중심 기준)
            Vector2 pos = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * radius;
            _piText.rectTransform.anchoredPosition = pos;

            // 텍스트는 항상 정방향(수직)으로 보이게
            _piText.rectTransform.rotation = Quaternion.identity;
        }
    }
}
