using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PlanetTaxi
{
    public class TaxiStationsMaster : SingletonBase<TaxiStationsMaster>
    {
	    [Tooltip("Az utas prefabot kell ide behúzni, ami tartalmazza az animációt is")]
	    [SerializeField] private GameObject _passengerModell;

	    [Tooltip("Az utas sebessége amikor mozog")]
	    [SerializeField] private float _passengerRunSpeed = 10.0f;

	    [Tooltip("Az utas beszállásakor lejátszódó hang audiosource-a")]
	    [SerializeField] private AudioSource _getInAudio;

	    [Tooltip("Az utas kiszállásakor lejátszódó hang audiosource-a")]
	    [SerializeField] private AudioSource _getOutAudio;
	    
	    private List<TaxiStation> _stations = new List<TaxiStation>();
	    private Animator _passengerAnimator;

	    private int _lastStartLocation = -1;
	    private TaxiStation _sendStation, _recieveStation, _currentStation;

	    private bool _passengerOnBoard, _passengerToTaxi;
	    private Coroutine _passengerMove;
	    
	    public TaxiStation CurrentStation { set { _currentStation = value; } }
	    
        private void Start () 
        {
	        if (_passengerModell == null)
	        {
		        Debug.Log("Nincsen behúzva az utas modellje a Taxi Station Master scriptbe, így nem fog működni a játék!");
#if UNITY_EDITOR
		        UnityEditor.EditorApplication.isPlaying = false;
#endif
	        }
	        else
	        {
		        GameObject tmp = Instantiate(_passengerModell);
		        tmp.SetActive(false);
		        _passengerAnimator = tmp.GetComponent<Animator>();
		        if (_passengerAnimator == null)
		        {
			        Debug.Log("Rossz prefab van behúzva utasnak a Taxi Station Master scriptbe, így nem fog működni a játék!");
#if UNITY_EDITOR
			        UnityEditor.EditorApplication.isPlaying = false;
#endif    
		        }
	        }
	        
	        GenerateLocations(2.0f);
        }
	
	
        private void Update () 
        {
	        if (_currentStation == null) return;
	        if (_passengerOnBoard && _currentStation == _recieveStation)
	        {
		        if (TaxiController.Instance.CurrentSpeed < 1.0f)
		        {
			        TaxiController.Instance.TaxiFreeChange(true);
			        LevelMaster.Instance.PassengerPassed();
			        _passengerOnBoard = false;
			        _passengerAnimator.transform.position = TaxiController.Instance.transform.position;
			        _passengerAnimator.transform.LookAt(_recieveStation.transform);
			        
			        
			        if (GuiFaceToTarget.Instance != null) GuiFaceToTarget.Instance.SetTarget();
			        if (_getOutAudio != null) _getOutAudio.Play();
			        _passengerMove = StartCoroutine(MovePassengerToPosition(_recieveStation.PassengerPosition,
				        () =>
				        {
					        _passengerAnimator.gameObject.SetActive(false);
					        
				        }));
			        _recieveStation = _sendStation = null;
			        ResetStations();
		        }
	        }
	        else if (!_passengerOnBoard && _currentStation == _sendStation)
	        {
		        if (TaxiController.Instance.CurrentSpeed < 1.0f)
		        {
			        if (!_passengerToTaxi)
			        {
				        if (_passengerMove != null) StopCoroutine(_passengerMove);
				        _passengerAnimator.transform.LookAt(TaxiController.Instance.transform);
				        _passengerToTaxi = true;
				        _passengerMove =
					        StartCoroutine(MovePassengerToPosition(TaxiController.Instance.transform.position, () =>
					        {
						        TaxiController.Instance.TaxiFreeChange(false);
						        LevelMaster.Instance.PassengerPickedUp();
						        _passengerAnimator.gameObject.SetActive(false);
						        if (_getInAudio != null) _getInAudio.Play();
						        if (GuiFaceToTarget.Instance != null) GuiFaceToTarget.Instance.SetTarget(_recieveStation.transform);
						        _passengerOnBoard = true;
						        _passengerToTaxi = false;
					        }));
			        }    
		        }
		        else
		        {
			        if (_passengerMove != null && _passengerToTaxi)
			        {
				        StopCoroutine(_passengerMove);
				        _passengerToTaxi = false;
				        _passengerMove = StartCoroutine(MovePassengerToPosition(_sendStation.PassengerPosition));
			        }
		        }
	        }
        }

	    private IEnumerator MovePassengerToPosition(Vector3 target, Action onArrive = null)
	    {
		    _passengerAnimator.SetBool("Run", true);
		    float targetDistance = Vector3.Distance(_passengerAnimator.transform.position, target) * 0.05f;
		    while (targetDistance < Vector3.Distance(_passengerAnimator.transform.position, target))
		    {
			    _passengerAnimator.transform.position = Vector3.MoveTowards(_passengerAnimator.transform.position, target,
				    _passengerRunSpeed * Time.deltaTime);
			    yield return null;
		    }
		    _passengerAnimator.SetBool("Run", false);
		    if (onArrive != null) onArrive.Invoke();
	    }
	    
	    
	    
	    public void AddStationToList(TaxiStation station)
	    {
		    _stations.Add(station);
	    }

	    public void GenerateLocations(float waitTime)
	    {
		    Invoke("GenerateLocations", waitTime);
	    }
	    
	    private void GenerateLocations()
	    {
		    
		    int startLocation = RandomStationNumber(_lastStartLocation);
		    _sendStation = _stations[startLocation]; 
		    
		    _sendStation.SetStation(true);
		    _passengerAnimator.transform.position = _sendStation.PassengerPosition;
		    _passengerAnimator.transform.LookAt(_sendStation.transform.position);
		    _passengerAnimator.gameObject.SetActive(true);

		    int recieveLocation = RandomStationNumber(startLocation);
		    _recieveStation = _stations[recieveLocation]; 
		    _recieveStation.SetStation(false);

		    _lastStartLocation = recieveLocation;
		    
		    if (GuiFaceToTarget.Instance != null) GuiFaceToTarget.Instance.SetTarget(_sendStation.transform);
	    }

	    private void ResetStations()
	    {
		    for (int i = 0; i < _stations.Count; i++) _stations[i].ResetStation();
	    }

	    private int RandomStationNumber(int exclude)
	    {
		    int tmp = exclude;
		    while (tmp == exclude)
		    {
			    tmp = Random.Range(0, _stations.Count);
		    }

		    return tmp;
	    }
    }

}
