using UnityEngine;


namespace PlanetTaxi
{
    
    [RequireComponent(typeof(Collider))]
    public class LevelEnd : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<TaxiController>() != null) LevelMaster.Instance.LoadNextLevel();
        }
    }

}
