using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace _02.Scripts.Roulette
{
    public class Roulette : MonoBehaviour
    {
        [FormerlySerializedAs("_baseRoulette")]
        [Title("Base")]
        [SerializeField] private RectTransform _rouletteBase = null;

        public RectTransform RouletteBase
        {
            get { return _rouletteBase; }
        }
    }
}