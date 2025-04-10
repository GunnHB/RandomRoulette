using UnityEngine;

using _02.Scripts.Common;

using Sirenix.OdinInspector;

namespace _02.Scripts.Roulette
{
    public class Adding : MonoBehaviour
    {
        [Title("Adding")]
        [SerializeField] private UIButton _addButton = null;

        public UIButton AddButton => _addButton;
    }
}
