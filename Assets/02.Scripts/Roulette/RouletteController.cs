using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;

using Sirenix.OdinInspector;

using _02.Scripts.Common;
using _02.Scripts.Manager;
using DarkTonic.MasterAudio;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine.Serialization;
using ColorUtility = UnityEngine.ColorUtility;
using Sequence = DG.Tweening.Sequence;
using Vector3 = UnityEngine.Vector3;

public static class RouletteColor
{
    private static readonly string[] hexColors = new[]
    {
        "#FF0000", // Red
        "#FFA500", // Orange
        "#FFDE21", // Yellow
        "#008000", // Green
        "#135DD8", // Blue
        "#560591"  // Purple
    };

    private static readonly Color[] colors;

    static RouletteColor()
    {
        colors = new Color[hexColors.Length];
        for (int i = 0; i < hexColors.Length; i++)
        {
            ColorUtility.TryParseHtmlString(hexColors[i], out colors[i]);
        }
    }

    public static Color GetColor(int index)
    {
        // 순환되도록
        return colors[index % colors.Length];
    }
}

namespace _02.Scripts.Roulette
{
    public class RouletteController : MonoBehaviour
    {
        [Title("InputBox")]
        [SerializeField] private ScrollRect _scrollView = null;
        [SerializeField] private InputBox _inputBoxPrefab = null;

        [Title("Roulette")] 
        [SerializeField] private Roulette _roulette = null;
        [SerializeField] private RouletteItem _rouletteItemPrefab = null;
        
        [Title("Button")]
        [SerializeField] private UIButton _spinButton = null;
        [SerializeField] private Adding _addingPrefab = null;
        
        private Adding _adding = null;
        
        private List<InputBox> _inputBoxList = new List<InputBox>();
        private List<RouletteData> _rouletteDataList = new List<RouletteData>();
        private List<RouletteItem> _rouletteItemList = new List<RouletteItem>();
        
        private List<float[]> _rouletteAngleList = new List<float[]>();
        
        // PlayTick 처리용
        private List<float> _tickAngles = new(); // 경계각 리스트 (보정 포함)
        private float _prevZ;
        private float _accumulatedRotation;
        private int _tickIndex;

        private void Awake()
        {
            AddElement();
            
            BindEvent();
        }

        private void OnDestroy()
        {
            GameManager.Instance.OnRequestSpinning -= OnSpinning;
        }

        private void AddElement()
        {
            RouletteData newData = new RouletteData();
            
            _rouletteDataList.Add(newData);
            
            SetupInputBox(newData);
            SetupRouletteItem(newData);

            newData.OnChangedWeight += UpdateAllRouletteItems;
            
            int colorIndex = _rouletteDataList.Count - 1;
            
            newData.ThisColor = RouletteColor.GetColor(colorIndex);
            newData.ThisWeight = 1;
        }
        
        private void SetupAdding()
        {
            // 최대치 이상이면 추가 버튼 비활성화
            if (_rouletteDataList.Count() >= 6)
            {
                if(_adding != null)
                    _adding.gameObject.SetActive(false);
                
                return;
            }
            
            if (_addingPrefab == null)
                return;

            if (_adding == null)
            {
                _adding = Instantiate(_addingPrefab, _scrollView.content);
                
                _adding.AddButton.onClick.AddListener(AddElement);
                
                GameManager.Instance.OnRequestSpinning += OnSpinning;
            }
            
            _adding.gameObject.SetActive(true);
            
            _adding.transform.SetAsLastSibling();
        }

        private void OnSpinning(bool value)
        {
            _adding.AddButton.interactable = !value;
        }
        
        private void SetupInputBox(RouletteData newData)
        {
            InputBox newInputBox = Instantiate(_inputBoxPrefab, _scrollView.content);
            newInputBox.gameObject.SetActive(false);
            
            newInputBox.Setup(newData);
            
            _inputBoxList.Add(newInputBox);
                
            // 첫 번째 박스는 삭제 버튼 비활성화
            if (newData == _rouletteDataList[0])
            {
                SetupAdding();
                
                newInputBox.RemoveButton.gameObject.SetActive(false);
                newInputBox.gameObject.SetActive(true);
            }
            else
            {
                Sequence sequence = DOTween.Sequence()
                    .SetAutoKill(false)
                    .OnStart(() =>
                    {
                        SetupAdding();
                        
                        newInputBox.transform.localScale = Vector3.zero;
                        newInputBox.gameObject.SetActive(true);
                    })
                    .Append(newInputBox.transform.DOScale(1f, .15f).SetEase(Ease.OutBounce))
                    .Join(newInputBox.GetComponent<CanvasGroup>().DOFade(1f, .15f));
            }

            newInputBox.OnRemoveAction += OnRemoveRoulette;
        }
        
        private void SetupRouletteItem(RouletteData newData)
        {
            RouletteItem newRouletteItem = Instantiate(_rouletteItemPrefab, _roulette.RouletteBase);
                
            newRouletteItem.transform.SetAsLastSibling();
            newRouletteItem.Setup(newData);

            _rouletteItemList.Add(newRouletteItem);
        }

        private void UpdateAllRouletteItems()
        {
            float totalWeight = _rouletteDataList.Sum(item => item.ThisWeight);
            float remainingWeight = totalWeight;
            
            for (int index = 0; index < _rouletteItemList.Count; ++index)
            {
                int weight = _rouletteDataList[index].ThisWeight;
                _rouletteItemList[index].SetFill(remainingWeight, totalWeight);
                remainingWeight -= weight;
            }
            
            UpdateAllTextPositions();
        }
        
        private void UpdateAllTextPositions()
        {
            float totalWeight = _rouletteDataList.Sum(d => d.ThisWeight);
            float cumulativeFill = 0f;
            float radius = 300f; // 조절 가능

            int count = _rouletteItemList.Count;
            for (int i = 0; i < count; i++)
            {
                // 조각 순서 반대로 적용 (시계방향 조각, 반시계 그림)
                int index = count - 1 - i;

                float weight = _rouletteDataList[index].ThisWeight;
                float fill = weight / totalWeight;

                _rouletteItemList[index].UpdateTextPosition(cumulativeFill, fill, radius);
                cumulativeFill += fill;
            }
        }

        private void OnRemoveRoulette(InputBox inputBox)
        {
            if (inputBox == null || inputBox.gameObject == null)
                return;
            
            int index = _inputBoxList.IndexOf(inputBox);
            
            Sequence sequence = DOTween.Sequence()
                .Append(inputBox.transform.DOScale(0f, .15f).SetEase(Ease.InBounce))
                .Join(inputBox.GetComponent<CanvasGroup>().DOFade(0f, .15f))
                .OnComplete(() =>
                {
                    Destroy(_rouletteItemList[index].gameObject);
                    Destroy(inputBox.gameObject);
                    
                    // 데이터 삭제
                    _inputBoxList.RemoveAt(index);
                    _rouletteItemList.RemoveAt(index);
                    _rouletteDataList.RemoveAt(index);

                    // 색상 재배치
                    ReassignColors();
                    UpdateAllRouletteItems();
                    
                    SetupAdding();
                });
        }
        
        private void ReassignColors()
        {
            for (int i = 0; i < _rouletteDataList.Count; i++)
            {
                _rouletteDataList[i].ThisColor = RouletteColor.GetColor(i);
                _rouletteItemList[i].UpdateColor(_rouletteDataList[i].ThisColor);
                _inputBoxList[i].SetLabel(_rouletteDataList[i].ThisColor);
            }
        }

        private void BindEvent()
        {
            if (_spinButton == null)
                return;
            
            _spinButton.onClick.AddListener(OnClickSpinButton);
        }

        private void OnClickSpinButton()
        {
            float randomRotations = UnityEngine.Random.Range(20f, 30f);     // 20 ~ 30바퀴
            float randomAngle = UnityEngine.Random.Range(0f, 360f);         // 멈출 위치
            float totalAngle = 360f * randomRotations + randomAngle;

            SetRouletteAngleList();
            
            Sequence sequence = DOTween.Sequence()
                .SetAutoKill(false)
                .OnStart(() =>
                {
                    GameManager.Instance.IsSpinning = true;
                })
                .Append(_spinButton.transform.DOScale(0f, .3f).SetEase(Ease.InBounce))
                .Join(_spinButton.GetComponent<CanvasGroup>().DOFade(0f, .3f))
                .InsertCallback(0.2f, () => { SpinRoulette(totalAngle); });
        }

        private void SetRouletteAngleList()
        {
            // 리스트 초기화
            _rouletteAngleList.Clear();
            
            float totalWeight = _rouletteDataList.Sum(item => item.ThisWeight);
            float accumulatedWeight = 0f;
            
            for (int index = 0; index < _rouletteDataList.Count; ++index)
            {
                // 아이템 마다의 가중치를 이용해 각도를 계산
                float start = ((accumulatedWeight / totalWeight) * 360f);
                float end = (((accumulatedWeight + _rouletteDataList[index].ThisWeight) / totalWeight) * 360f);
                
                // 리스트에 추가
                _rouletteAngleList.Add(new []{ start, end });
                
                accumulatedWeight += _rouletteDataList[index].ThisWeight;
            }
        }
        
        private void SpinRoulette(float totalAngle)
        {
            if (_roulette == null)
                return;

            _roulette.RouletteBase.transform.localEulerAngles = Vector3.zero;

            _prevZ = 0f;
            _accumulatedRotation = 0f;
            _tickIndex = 0;

            SetupTickAngles();
        
            _roulette.RouletteBase.transform
                .DORotate(new Vector3(0f, 0f, -totalAngle), 5f, RotateMode.FastBeyond360).SetEase(Ease.OutQuart)
                .OnUpdate(() =>
                {
                    if(GameManager.Instance.IsSpinning)
                        PlayTick();
                })
                .OnComplete(() =>
                {
                    GameManager.Instance.IsSpinning = false;
                    
                    _spinButton.transform.localScale = Vector3.one;
                    _spinButton.GetComponent<CanvasGroup>().alpha = 1;
                            
                    float finalAngle = totalAngle % 360f;
                    RouletteData winningData = GetWinningData(finalAngle);
                    
                    GameManager.Instance.ActivateResult(winningData);
                });
        }
        
        private void SetupTickAngles()
        {
            _tickAngles.Clear();

            float totalWeight = _rouletteDataList.Sum(d => d.ThisWeight);
            float accumulated = 0f;

            for (int i = 0; i < _rouletteDataList.Count; i++)
            {
                float ratio = accumulated / totalWeight;
                float startAngle = 360f * ratio;

                _tickAngles.Add(startAngle);
                accumulated += _rouletteDataList[i].ThisWeight;
            }
        }
        
        // private void PlayTick()
        // {
        //     if (_roulette == null)
        //         return;
        //
        //     if (_angleListIndex >= _rouletteDataList.Count)
        //         _angleListIndex %= _rouletteDataList.Count;
        //     
        //     float currAngle = 360f - _roulette.RouletteBase.eulerAngles.z;
        //     if (currAngle >= _rouletteAngleList[_angleListIndex][1])
        //     {
        //         Debug.Log("Playing Tick!!");
        //         
        //         ++_angleListIndex;
        //     }
        // }
        
        private void PlayTick()
        {
            float currentZ = _roulette.RouletteBase.transform.eulerAngles.z;
            float delta = (_prevZ - currentZ + 360f) % 360f;
            _accumulatedRotation += delta;
            _prevZ = currentZ;

            int pieceCount = _tickAngles.Count;

            while (true)
            {
                int logicalIndex = _tickIndex % pieceCount;
                int lap = _tickIndex / pieceCount;
                float targetAngle = _tickAngles[logicalIndex] + lap * 360f;

                if (_accumulatedRotation >= targetAngle)
                {
                    MasterAudio.PlaySound("Tick");
                    _tickIndex++;
                }
                else break;
            }
        }
        
        private RouletteData GetWinningData(float finalAngle)
        {
            if (_rouletteAngleList.Count == 0)
                return null;

            for (int index = 0; index < _rouletteAngleList.Count; ++index)
            {
                if (finalAngle >= _rouletteAngleList[index][0] && finalAngle < _rouletteAngleList[index][1])
                    return _rouletteDataList[index];
            }

            return null;
        }
    }
}
