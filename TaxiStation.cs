using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetTaxi
{
    public class TaxiStation : MonoBehaviour
    {

	    [Tooltip("Ez az objektum kapcsol be, ha az állomás a küldő állomás")]
	    [SerializeField] private GameObject _stationSend;
	    [Tooltip("Ez az objektum kapcsol be, ha az állomás a célállomás")]
	    [SerializeField] private GameObject _stationRecieve;
	    [Tooltip("Itt jelenik meg a várakozó utas, és ide fut, majd, amikor kiszáll a taxiból")]
	    [SerializeField] private Transform _passengerPosition;
	    
	    public Vector3 PassengerPosition { get { return _passengerPosition.position; } }
	    
        private void Start ()
        {
	        if (_passengerPosition == null) _passengerPosition = transform;
	        ResetStation();
			TaxiStationsMaster.Instance.AddStationToList(this);
        }

	    public void SetStation(bool send)
	    {
		    if (_stationSend != null) _stationSend.SetActive(send);
		    if (_stationRecieve != null) _stationRecieve.SetActive(!send);
	    }

	    public void ResetStation()
	    {
		    if (_stationSend != null) _stationSend.SetActive(false);
		    if (_stationRecieve != null) _stationRecieve.SetActive(false);
	    }

	    private void OnTriggerEnter(Collider other)
	    {
		    if (other.GetComponent<TaxiController>() != null) TaxiStationsMaster.Instance.CurrentStation = this;
	    }
	    
	    private void OnTriggerExit(Collider other)
	    {
		    if (other.GetComponent<TaxiController>() != null) TaxiStationsMaster.Instance.CurrentStation = null;
	    }


    }
}
