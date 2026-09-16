using UnityEngine;

namespace Dacodelaac.Common
{
    public class Rotator : MonoBehaviour
    {
        [SerializeField] private float speed;
        [SerializeField] Vector3 axis;
        [SerializeField] Space space = Space.Self;

        void Update()
        {
            transform.Rotate(axis, speed * Time.unscaledDeltaTime, space);
        }
    }
}