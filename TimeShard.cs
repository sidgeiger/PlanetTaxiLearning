using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetTaxi
{
    public class TimeShard : MonoBehaviour
    {
	    [Header("A GameObject-hez kapcsolt Collider-nek Trigger-nek kell lennie!")]
	    [Tooltip("Objektum ami bekapcsol felvételkor")]
	    [SerializeField] private GameObject _objectToTurnOn;

	    [Tooltip("Hang ami lejátszódik felvételkor. Ne legyen az AudioSource ezen a GameObject-en!")]
	    [SerializeField] private AudioSource _pickUpSound;


	    private void Start()
	    {
		    if (_objectToTurnOn != null) _objectToTurnOn.SetActive(false);
	    }

	    private void OnTriggerEnter(Collider other)
	    {
		    if (other.GetComponent<TaxiController>() != null)
		    {
			    LevelMaster.Instance.TimeShardCollected();
			    if (_objectToTurnOn != null) _objectToTurnOn.SetActive(true);
			    if (_pickUpSound != null) _pickUpSound.Play();
			    Destroy(gameObject);
		    }
	    }
    }
}
