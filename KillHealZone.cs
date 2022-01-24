using System.Collections.Generic;
using UnityEngine;

namespace PlanetTaxi
{
	[RequireComponent(typeof(Collider))]
    public class KillHealZone : MonoBehaviour
	{
		[Tooltip("Mennyit növelje/csökkentse a benne tartózkodó objektumok életét másodpercenként a trigger")]
		[SerializeField] private float _hpChange = 10.0f;

		[Tooltip("Objektum ami bekapcsol, ha van valami a triggerben")]
		[SerializeField] private GameObject _workingIndicator;

		[Tooltip("Objektum ami bekapcsol, ha van nincsen semmi a triggerben")]
		[SerializeField] private GameObject _notWorkingIndicator;
		
		
		private List<IDamageable> _currentlyInTrigger = new List<IDamageable>();

		private void Start()
		{
			if (_workingIndicator != null) _workingIndicator.SetActive(false);
			if (_notWorkingIndicator != null) _notWorkingIndicator.SetActive(true);
		}

		private void Update () 
        {
			if (_currentlyInTrigger.Count > 0) for (int i = 0; i < _currentlyInTrigger.Count; i++) _currentlyInTrigger[i].ChangeHP(_hpChange * Time.deltaTime);		
        }

		private void OnTriggerEnter(Collider other)
		{
			IDamageable tmp = other.GetComponent<IDamageable>();
			if (tmp != null)
			{
				_currentlyInTrigger.Add(tmp);
				if (_workingIndicator != null) _workingIndicator.SetActive(true);
				if (_notWorkingIndicator != null) _notWorkingIndicator.SetActive(false);
			}
			
		}
		
		private void OnTriggerExit(Collider other)
		{
			IDamageable tmp = other.GetComponent<IDamageable>();
			if (tmp != null)
			{
				_currentlyInTrigger.Remove(tmp);
				if (_currentlyInTrigger.Count < 1)
				{
					if (_workingIndicator != null) _workingIndicator.SetActive(false);
					if (_notWorkingIndicator != null) _notWorkingIndicator.SetActive(true);
				}
			}
		}
	}
}

