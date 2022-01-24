using UnityEngine;

namespace PlanetTaxi
{
    public class GuiFaceToTarget : SingletonBase<GuiFaceToTarget>
    {
		[Tooltip("A pontos célzáshoz a nyílnak 0,0,0 forágsnál lefelé kell mutatnia. Ha a modell, vagy a kép nem ilyen, akkor itt lehet megadni, hogy ehhez képest mennyit kell forgatni rajta")]
	    [SerializeField] private float _angelCompensation;
	    
	    private Transform _target;
	    private Vector3 _direction;
	    
        private void Start () 
        {
			SetTarget();
        }
	
	
        private void Update ()
        {
	        _direction = TaxiController.Instance.transform.position - _target.transform.position;
	    	transform.rotation = Quaternion.AngleAxis((Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg) + _angelCompensation, Vector3.forward);    
        }

	    public void SetTarget(Transform target = null)
	    {
		    _target = target;
		    if (_target == null) gameObject.SetActive(false);
		    else gameObject.SetActive(true);
	    }
    }
}
