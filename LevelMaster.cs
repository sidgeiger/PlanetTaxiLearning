using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PlanetTaxi
{
    public class LevelMaster : SingletonBase<LevelMaster>
    {
	    [Tooltip("A pálya neve")] 
	    [SerializeField] private string _levelName = "Level";
	    [Tooltip("A pálya névét megjelenítű UI Text mező")] 
	    [SerializeField] private Text _guiLevelName;
	    [Tooltip("Háttérzene AudioSource-a")] 
	    [SerializeField] private AudioSource _backgroundMusic;
		[Tooltip("Legyen e mentve a játékállás")]
	    [SerializeField] private bool _saveGame = true;
	    [Tooltip("A következő scene neve, ha sikerült teljesíteni a pályát")] 
	    [SerializeField] private string _nextLevelName = "Level";
	    [Tooltip("Az elszállítandó utasok száma")] 
	    [SerializeField] private int _passengerPass = 3;
	    [Tooltip("A már elszállított utasok száma")] 
	    [SerializeField][ReadOnly] private int _currentPassengerPass;
	    [Tooltip("Az utas elszállításáért kapható maximális összeg")] 
	    [SerializeField] private float _maxPay = 100.0f;
	    [Tooltip("Az utas elszállításáért kapható minimális összeg")] 
	    [SerializeField] private float _minPay = 10.0f;
	    [Tooltip("Az utas elszállításáért éppen megkapható összeg, ha most érkezel a célállomásra")] 
	    [SerializeField][ReadOnly] private float _actualPay;
	    [Tooltip("Az utas elszállításáért kapható összeg ennyivel csökken másodpercenként")] 
	    [SerializeField] private float _reducingRate = 5.0f;
	    [Tooltip("Ennyi másodperc után kezd el csökkenni az utas elszállításáért kapható összeg")] 
	    [SerializeField] private float _gracePeriod = 10.0f;
	    [Tooltip("Az utas elszállításáért éppen kaphatő összeget kijelző UI Text mező")] 
	    [SerializeField] private Text _guiActualPay;
	    [Tooltip("A játék során összesen összeszedett pénzt kijelző UI Text mező")] 
	    [SerializeField] private Text _guiFullPay;
	    [Tooltip("Az összesen összeszedendő TimeShardok mennyisége")] 
	    [SerializeField] private int _neededTimehard = 3;
	    [Tooltip("Az aktuálisan összeszedett TimeShardok mennyisége")] 
	    [SerializeField][ReadOnly] private int _collctedTimeShardsCount;
	    [Tooltip("Ha minden TimeShard megvan AudioSource-a")] 
	    [SerializeField] private AudioSource _allTimeShardAudio;
	    [Tooltip("Ha hiányzik még TimeShard AudioSource-a")] 
	    [SerializeField] private AudioSource _timeShardNeedAudio;
	    [Tooltip("Ha utast kell még vinni AudioSource-a")] 
	    [SerializeField] private AudioSource _passengerNeedAudio;
	    [Tooltip("Egyik feltétel sem teljesült még AudioSource-a")] 
	    [SerializeField] private AudioSource _conditionsFailedAudio;
	    [Tooltip("Ajtónyitás AudioSource-a")] 
	    [SerializeField] private AudioSource _doorOpenAudio;
	    [Tooltip("A Door scriptet tartalmazó GameObject")] 
	    [SerializeField] private Door _closedDoor;
		[Tooltip("A következő pálya betöltéséig megjelenő töltőkép")]
	    [SerializeField] private GameObject _loadingObjects;
	    [Tooltip("A pause menüt tartalmazó GameObject a Canvas-on")]
		[SerializeField] private GameObject _pauseObjects;
	    [Tooltip("A menü scene neve")]
	    [SerializeField] private string _menuSceneName = "Menu";

	    private bool _guiActualPayFilled, _guiFullPayFilled, _timeShardsCollected, _passengersPassed;
	    private float _fullPay;
	    
        private void Start ()
        {
	        Time.timeScale = 1;
	        if (_loadingObjects != null) _loadingObjects.SetActive(false);
	        _guiActualPayFilled = _guiActualPay != null;
	        _guiFullPayFilled = _guiFullPay != null;
	        _fullPay = PlayerPrefs.GetFloat("FullPay", 0);
	        if (_guiFullPayFilled) _guiFullPay.text = _fullPay.ToString("N2");
	        if (_guiActualPayFilled) _guiActualPay.text = "FREE";
	        if (_guiLevelName != null) _guiLevelName.text = _levelName;
	        if (_passengerPass == _currentPassengerPass) _passengersPassed = true;
	        if (_neededTimehard == _collctedTimeShardsCount) _timeShardsCollected = true;
        }
	
	
        private void Update () 
        {
	        if (Input.GetButtonDown("Cancel"))
	        {
		        if (Time.timeScale != 0)
		        {
			        Time.timeScale = 0;
			        if (_pauseObjects != null) _pauseObjects.SetActive(true);
		        }
		        else
		        {
			        ContinueGame();
		        }
	        }
        }

	    public void ContinueGame()
	    {
		    Time.timeScale = 1;
		    if (_pauseObjects != null) _pauseObjects.SetActive(false);
	    }

	    public void ExitToMenu()
	    {
		    Time.timeScale = 1;
		    SaveGame(SceneManager.GetActiveScene().name);
		    SceneManager.LoadSceneAsync(_menuSceneName);
		    if (_loadingObjects != null) _loadingObjects.SetActive(true);
	    }

	    public void TimeShardCollected()
	    {
		    if (++_collctedTimeShardsCount >= _neededTimehard)
		    {
			    _timeShardsCollected = true;
			    if (_allTimeShardAudio != null) _allTimeShardAudio.Play();
			    if (_passengersPassed) OpenDoor();
		    }
	    }

	    public void PassengerPickedUp()
	    {
		    StartCoroutine(PassengerOnboard());
	    }
	    
	    public void PassengerPassed()
	    {
		    StopCoroutine(PassengerOnboard());
		    if (++_currentPassengerPass >= _passengerPass)
		    {
			    _passengersPassed = true;
			    if (_timeShardsCollected) OpenDoor();
		    }
		    else TaxiStationsMaster.Instance.GenerateLocations(2.5f);

		    _fullPay += _actualPay;
		    _actualPay = 0.0f;
		    if (_guiFullPayFilled) _guiFullPay.text = _fullPay.ToString("N2");
		    if (_guiActualPayFilled) _guiActualPay.text = "FREE";

	    }

	    private IEnumerator PassengerOnboard()
	    {
		    _actualPay = _maxPay;
		    if (_guiActualPayFilled) _guiActualPay.text = _actualPay.ToString("N2");
		    yield return new WaitForSeconds(_gracePeriod);
		    while (_actualPay >= _minPay)
		    {
			    _actualPay -= _reducingRate * 0.5f;
			    if (_actualPay < _minPay) _actualPay = _minPay;
			    if (_guiActualPayFilled) _guiActualPay.text = _actualPay.ToString("N2");
			    yield return new WaitForSeconds(0.5f);
		    }
	    }

	    private void OpenDoor()
	    {
		    if (_closedDoor != null) _closedDoor.OpenDoor();
		    if (_doorOpenAudio != null) _doorOpenAudio.Play();
	    }

	    public void LoadNextLevel()
	    {
		    if (_loadingObjects != null) _loadingObjects.SetActive(true);
		    TaxiController.Instance.DisableTaxi();
		    
		    SaveGame(_nextLevelName);
		    
		    SceneManager.LoadSceneAsync(_nextLevelName);
	    }

	    public void SaveGame(string levelName)
	    {
		    if (_saveGame)
		    {
			    PlayerPrefs.SetString("CurrentLevel", levelName);
			    PlayerPrefs.SetFloat("FullPay", _fullPay);
		    }
	    }

	    public void TaxiAtTheClosedDoor()
	    {
		    if (!_passengersPassed && !_timeShardsCollected)
		    {
			    if (_conditionsFailedAudio != null) _conditionsFailedAudio.Play();
			    return;
		    }

		    if (!_passengersPassed)
		    {
			    if (_passengerNeedAudio != null) _passengerNeedAudio.Play();
			    return;
		    }

		    if (!_timeShardsCollected)
		    {
			    if (_timeShardNeedAudio != null) _timeShardNeedAudio.Play();
		    }
	    }
    }

}
