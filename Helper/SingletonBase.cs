using UnityEngine;

namespace PlanetTaxi
{
    public class SingletonBase<T> : MonoBehaviour where T : MonoBehaviour
    {

        private static T _instance;
        public static T Instance
        {
            get { return _instance; }
        }


        private void Awake()
        {
            if (_instance == null) _instance = this as T;
            else Destroy(gameObject);
        }	
    }
}

