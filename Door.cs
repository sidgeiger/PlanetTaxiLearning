using System.Collections;
using System.Collections.Generic;
using PlanetTaxi;
using UnityEngine;

public class Door : MonoBehaviour
{
	[Tooltip("Ha minden feltétel teljesül, akkor ez az objektum kapcsol majd be a zárt ajtó helyett")]
	[SerializeField] private GameObject _openedDoor;

	private void Start()
	{
		if (_openedDoor != null) _openedDoor.SetActive(false);
	}

	public void OpenDoor()
	{
		if (_openedDoor != null) _openedDoor.SetActive(true);
		gameObject.SetActive(false);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.GetComponent<TaxiController>() != null)
		{
			LevelMaster.Instance.TaxiAtTheClosedDoor();
		}
	}
}
