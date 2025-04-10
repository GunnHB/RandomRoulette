using UnityEngine;

using _02.Scripts.Common;

using Sirenix.OdinInspector;
using UnityEngine.UI;

namespace _02.Scripts.Roulette
{
    public class Adding : MonoBehaviour
    {
        [Title("Adding")]
        [SerializeField] private Button _addButton = null;

        public Button AddButton => _addButton;
    }
}
