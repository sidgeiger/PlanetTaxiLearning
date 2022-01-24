using System.Collections;
using System.Collections.Generic;
using PlanetTaxi;
using UnityEditorInternal;
using UnityEngine;

namespace PlanetTaxi
{

    public class CameraController : MonoBehaviour
    {
        [Tooltip("Követendő objektum")]
        [SerializeField] private TaxiController _taxi;

        [Tooltip("Késleltetés")]
        [SerializeField] private float _delayTime = 0.25f;

        [Tooltip("A kamera pozíciójának minimális Z értéke")]
        [SerializeField] private float _minOpen = -10.0f;
	
        [Tooltip("A kamera pozíciójának maximális Z értéke")]
        [SerializeField] private float _maxOpen = -25.0f;

        private Vector3 _cameraVelocity, _nextPosition, _pastPosition, _pastTaxiPosition;

#if UNITY_EDITOR
        private void Start()
        {
            if (_taxi == null)
            {
                Debug.Log("Kérlek nézd át a CameraController scriptbe behúzott referenciákat!");
                UnityEditor.EditorApplication.isPlaying = false;
            }
        }
#endif

        private void FixedUpdate () 
        {
            _nextPosition = _taxi.transform.position;
            _nextPosition.z = Mathf.Lerp(_minOpen, _maxOpen, _taxi.CurrentSpeed / _taxi.MaxSpeed);
            transform.position = Vector3.SmoothDamp(transform.position, _nextPosition, ref _cameraVelocity, _delayTime);
            transform.LookAt(_taxi.transform);
        }

		
    }

}
