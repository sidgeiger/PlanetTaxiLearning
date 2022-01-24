using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

namespace PlanetTaxi
{
	[RequireComponent(typeof(Rigidbody))]
    public class TaxiController : SingletonBase<TaxiController>, IDamageable
    {
	    [Header("A taxinak a legfelső objektumára kell tenni ezt a scriptet, ha máshol van, nem fog jól működni!")]
	    [Space(-10), Header("A taxinak a tengelyeinek alapból úgy kell állnia, hogy az X tengely vízszintesen jobbra,")]
	    [Space(-10), Header("az Y függőlegesen felfelé, a Z pedig \"a monitorba befelé\" kell hogy mutasson!")]
	    
	    [Tooltip("Az erő, amivel a taxi mozogni fog")]
	    [SerializeField] private float _taxiSpeed = 15.0f;
	    
	    [Tooltip("A taxi tervezett maximális sebessége (ha rosszul van megadva, akkor a sebeséget jelző slider rosszul fog működni eleinte!)")]
	    [SerializeField] private float _taxiMaxSpeed = 200.0f;

	    [Tooltip("Lefelé gyorsítás tiltása")] 
	    [SerializeField] private bool _lockDown = true;

	    [Tooltip("A taxi függőleges tengelye körüli megfordulás sebessége fok/másodpercben megadva")] 
	    [SerializeField] private float _rotationSpeed = 360.0f;

	    [Tooltip("Minimum sebesség, aminél a taxi elkezd dőlni")]
	    [SerializeField] private float _minSpeedToTilt = 1f;

	    [Tooltip("A taxi maximális dőlésének értéke fokokban megadva")]
	    [SerializeField] private float _maxTiltAngle = 30.0f;

	    [Tooltip("Hajtómű hangja")]
	    [SerializeField] private AudioSource _thrusterAudio;

	    [Tooltip("Oldalsó hajtómű praticlejének a gameobject-je")]
	    [SerializeField] private GameObject _sideParticle;
	    
	    
	    [Tooltip("Alsó hajtómű praticlejének a gameobject-je")]
	    [SerializeField] private GameObject _bottomParticle;
	    
	    
	    [Tooltip("Felső hajtómű praticlejének a gameobject-je")]
	    [SerializeField] private GameObject _topParticle;

	    [Tooltip("A taxi kezdő élete")]
	    [SerializeField] private float _hp = 100.0f;
	    
	    [Tooltip("Sebességfüggetlen fix sérülésérték engedélyezése ütközéskor")]
	    [SerializeField] private bool _constantDamage;

	    [Tooltip("Sebességfüggetlen fix sérülés értéke")]
	    [SerializeField] private float _constantDamageValue = 5.0f;
	    
	    [Tooltip("Sebességfüggő sérülés maximális értéke")]
	    [SerializeField] private float _maxDamage = 25.0f;

	    [Tooltip("Az ütközés sebességét enyivel szorozzuk meg, és ebből kapjuk az ütközés sérülés értkét")]
	    [SerializeField] private float _damageMultiplier = 1.0f;
	    
	    [Tooltip("Ennyi időnek kell minimum eltellnie két ütközés között, hogy újra sérüljön a taxi")]
	    [SerializeField] private float _damageWaitTime = 0.25f;
	    
	    [Tooltip("Minimum sebesség ami fölött sérül a taxi, ha ütközik")]
	    [SerializeField] private float _minDamageSpeed = 20f;
	    
	    [Tooltip("A sérüléskor hallható hang")]
	    [SerializeField] private AudioSource _crashAudio;

	    [Tooltip("A felrobbanáskor hallható hang, ez ne legyen a Taxira rakva, mert nem fog lejátszódni!")]
	    [SerializeField] private AudioSource _explosionAudio;
	
	    [Tooltip("A felrobbanáskor megjelenő particle, lehet külön prefab is")]
	    [SerializeField] private GameObject _explosionParticle;

	    [Tooltip("A felrobbanáskor megjelenő modell, lehet külön prefab is")]
	    [SerializeField] private GameObject _explosionModel;

	    [Tooltip("A HP-t jelző slider")]
	    [SerializeField] private Slider _healthSlider;

	    [Tooltip("A HP-t jelző slider ekkora élet alatt váltson az alacsony életet jelző színre")]
	    [SerializeField] private float _healthSliderLowValue;
	    
	    [Tooltip("A HP-t jelző slider alacsony életet jelző színe")]
	    [SerializeField] private Color _healthSliderLowColor = Color.red;
	    
	    [Tooltip("A sebességet jelző slider")]
	    [SerializeField] private Slider _speedSlider;
		
	    [Tooltip("A sebességet megjelenítő UI Text")]
	    [SerializeField] private Text _speedText;

	    [Tooltip("A taxiban nincs utas model")]
	    [SerializeField] private GameObject _taxiFreeModell;

	    [Tooltip("A taxiban van utas model")]
	    [SerializeField] private GameObject _taxiReservedModell;

	    private bool _thrusterAudioFilled,
		    _sideParticleFilled,
		    _bottomParticleFilled,
		    _topParticleFilled,
		    _crashAudioFilled,
		    _healthSliderFilled,
		    _speedTextFilled,
		    _speedSliderFilled,
		    _taxiFreeFilled,
		    _taxiReservedFilled,
		    _dead;
		
	    private float _maxHp, _horizontal, _vertical, _velocity, _targetRotationY, _nextCollisionTime;
	    private Color _healthSliderDefaultColor;
	    private Image _healthSliderFillImage;
	    private Rigidbody _rb;
	    
	    public float MaxSpeed { get { return _taxiMaxSpeed; } }
	    public float CurrentSpeed { get { return _velocity; } }
	    
	    private void Start ()
	    {
		    _thrusterAudioFilled = _thrusterAudio != null;
		    _sideParticleFilled = _sideParticle != null;
		    _bottomParticleFilled = _bottomParticle != null;
		    _topParticleFilled = _topParticle != null;
		    _crashAudioFilled = _crashAudio != null;
		    _healthSliderFilled = _healthSlider != null;
		    _speedTextFilled = _speedText != null;
		    _speedSliderFilled = _speedSlider != null;
		    _taxiFreeFilled = _taxiFreeModell != null;
		    _taxiReservedFilled = _taxiReservedModell != null;
		    
		    _maxHp = _hp;
		    _rb = GetComponent<Rigidbody>();
		    
		    if (_healthSliderFilled)
		    {
			    _healthSliderFillImage = _healthSlider.fillRect.GetComponent<Image>();
			    _healthSliderDefaultColor = _healthSliderFillImage.color;
		    }
		    
		    if (_topParticleFilled) _topParticle.SetActive(false);
		    TaxiFreeChange(true);
		    UpdateUI();
	    }
	
	
        private void Update ()
        {
	        if (_dead) return;
	        _velocity = _rb.velocity.magnitude;
	        if (_velocity > _taxiMaxSpeed) _taxiMaxSpeed = _velocity;
	        
	        HandleControll();
	        HandleTiltAndTurn();
			UpdateUI();
        }

	    private void FixedUpdate()
	    {
		    _rb.AddForce(_horizontal, _vertical, 0.0f, ForceMode.Force);
	    }

	    private void OnCollisionEnter(Collision other)
	    {
		    if (_velocity < _minDamageSpeed) return;
		    if (Time.time < _nextCollisionTime) return;
		    _nextCollisionTime = Time.time + _damageWaitTime;
		    if (_constantDamage) ChangeHP(-_constantDamageValue);
		    else ChangeHP(-(_velocity * _damageMultiplier > _maxDamage ? _maxDamage : _velocity * _damageMultiplier));
		    if (_crashAudioFilled) _crashAudio.Play();
	    }

	    private void HandleControll()
	    {
		    _horizontal = Input.GetAxis("Horizontal") * _taxiSpeed * Time.deltaTime;
		    _vertical = Input.GetAxis("Vertical") * _taxiSpeed * Time.deltaTime;

		    if (_lockDown && _vertical < 0) _vertical = 0.0f;
		    
		    if (!Mathf.Approximately(_horizontal, 0.0f) || !Mathf.Approximately(_vertical, 0.0f)) PlayThrusterAudio();
		    
		    if (_sideParticleFilled)
		    {
			    if (!Mathf.Approximately(_horizontal, 0.0f)) _sideParticle.SetActive(true);
			    else _sideParticle.SetActive(false);
		    }

		    if (_bottomParticleFilled)
		    {
			    if (_vertical > 0) _bottomParticle.SetActive(true);
			    else _bottomParticle.SetActive(false);
		    }

		    if (!_lockDown && _topParticleFilled)
		    {
			    if (_vertical < 0) _topParticle.SetActive(true);
			    else _topParticle.SetActive(false);
		    }
		    
	    }

	    private void PlayThrusterAudio()
	    {
		    if (!_thrusterAudioFilled) return;
		    _thrusterAudio.Play();
	    }

	    private void HandleTiltAndTurn()
	    {
		    float rotationAroundZ = Mathf.InverseLerp(_minSpeedToTilt, _taxiMaxSpeed, Mathf.Abs(_rb.velocity.x)) * _maxTiltAngle;
		    
		    if (_horizontal > 0 && !Mathf.Approximately(_targetRotationY, 0.0f)) _targetRotationY = 0.0f;
		    else if (_horizontal < 0 && !Mathf.Approximately(_targetRotationY, 180.0f)) _targetRotationY = 180.0f;
		    
		    _rb.rotation = Quaternion.RotateTowards(_rb.rotation, Quaternion.Euler(0.0f, _targetRotationY, rotationAroundZ), _rotationSpeed * Time.deltaTime);
	    }
	    
	    private void UpdateUI()
	    {
		    if (_healthSliderFilled) _healthSlider.value = _hp / _maxHp;

		    if (_speedSliderFilled) _speedSlider.value = _rb.velocity.magnitude / _taxiMaxSpeed;
		    if (_speedTextFilled) _speedText.text = _rb.velocity.magnitude.ToString("N2");
	    }


	    public void ChangeHP(float amount)
	    {
		    if (_dead) return;
		    _hp += amount;
		    if (_hp <= 0)
		    {
			    _hp = 0;
				Die();    
		    }

		    if (_hp > _maxHp) _hp = _maxHp;

		    if (_healthSliderFilled)
		    {
			    _healthSlider.value = _hp / _maxHp;
			    _healthSliderFillImage.color = _hp <= _healthSliderLowValue ? _healthSliderLowColor : _healthSliderDefaultColor;
		    }
	    }
	    
	    private void Die()
	    {
		    _dead = true;
		    if (_explosionModel != null)
		    {
			    GameObject deadModell = Instantiate(_explosionModel, transform.position, transform.rotation, null);
			    deadModell.SetActive(true);
		    }
		    if (_explosionParticle != null)
		    {
			    GameObject explosion = Instantiate(_explosionParticle, transform.position, transform.rotation, null);
			    explosion.SetActive(true);
		    }

		    if (_explosionAudio != null) _explosionAudio.Play();
		    gameObject.SetActive(false);
	    }

	    public void TaxiFreeChange(bool free)
	    {
		    if (_taxiFreeFilled) _taxiFreeModell.SetActive(free);
		    if (_taxiReservedFilled) _taxiReservedModell.SetActive(!free);
	    }

	    public void DisableTaxi()
	    {
		    _rb.isKinematic = true;
		    enabled = false;
	    }
    }

}
