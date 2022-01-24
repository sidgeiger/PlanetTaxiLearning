using UnityEngine;

namespace PlanetTaxi
{
    public class Pathfinder : MonoBehaviour
    {
        [Tooltip("Célpontok listája")] [SerializeField]
        private PathFindingTarget[] _targets;

        [Tooltip("Ha körbeért megálljon, vagy kezdje előről az első ponttól")] [SerializeField]
        private bool _loop;

        private int _currentTarget;
        private float _speed, _timeToWait;
        private Quaternion _faceToTargetQuaternion;


        private void Start()
        {
            CalculateSpeedAndRotation();
        }


        private void Update()
        {
            if (_currentTarget >= _targets.Length) return;

            transform.position = Vector3.MoveTowards(transform.position, _targets[_currentTarget].Target.position,
                _speed * Time.deltaTime);
            if (transform.rotation != _faceToTargetQuaternion)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, _faceToTargetQuaternion,
                    _targets[_currentTarget].MaxRotation * Time.deltaTime);
            if (transform.position == _targets[_currentTarget].Target.position)
            {
                if (_timeToWait <= 0.0f)
                {
                    _timeToWait = Time.time + _targets[_currentTarget].WaitWhenArrived;
                    return;
                }

                if (_timeToWait <= Time.time)
                {
                    _timeToWait = -1.0f;
                    if (++_currentTarget >= _targets.Length && _loop) _currentTarget = 0;
                    CalculateSpeedAndRotation();
                }
            }
        }

        private void CalculateSpeedAndRotation()
        {
            if (_currentTarget >= _targets.Length) return;


            _speed = Vector3.Distance(transform.position, _targets[_currentTarget].Target.position) /
                     _targets[_currentTarget].TimeToReachTarget;
            if (_targets[_currentTarget].FaceToTarget)
            {
                _faceToTargetQuaternion =
                    Quaternion.LookRotation(_targets[_currentTarget].Target.position - transform.position);
            }
        }
    }

    [System.Serializable]
    public class PathFindingTarget
    {
        [Tooltip("Cél objektum, ahová menni kell")]
        public Transform Target;

        [Tooltip("A cél felé forduljon e vagy tartsa meg a rotációját")]
        public bool FaceToTarget = true;

        [Tooltip("Maximális másodpercenkénti elfordulás fokokban megadva, ha be van kapcsolva a Face To Target")]
        public float MaxRotation = 360.0f;

        [Tooltip("Mennyi idő alatt érjen a célhoz másodpercben megadva")]
        public float TimeToReachTarget = 5.0f;

        [Tooltip("Hány másodpercig várjon, ha elérte a célt")]
        public float WaitWhenArrived = 1.0f;
    }
}