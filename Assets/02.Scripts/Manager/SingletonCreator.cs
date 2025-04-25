using System;
using _02.Scripts.Result;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _02.Scripts.Manager
{
    public class SingletonCreator : MonoBehaviour
    {
        [Title("GameManager")]
        [SerializeField] private ResultPanel _resultPanel = null;
        
        private void Awake()
        {
            InitSingletonObject<GameManager>();

            if (GameManager.Instance != null)
                GameManager.Instance.SetupResultPanel(_resultPanel);
        }

        private void InitSingletonObject<T>() where T : Singleton<T>
        {
            GameObject obj = new GameObject(typeof(T).Name);

            if (obj != null)
                obj.AddComponent(typeof(T));
        }
    }
}