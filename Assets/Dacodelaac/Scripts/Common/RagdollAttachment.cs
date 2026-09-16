// #define RAGDOLL
#if RAGDOLL
using System.Collections;
using Dacodelaac.Utils;
using DG.Tweening;
using UnityEngine;

namespace Dacodelaac.Common
{
    public class RagdollAttachment : MonoBehaviour
    {
        [SerializeField] Rigidbody rigid;
        [SerializeField] Collider collider;
        
        public void Initialize()
        {
        }

        public void Activate(Vector3 force, bool disable)
        {
            var clone = Instantiate(gameObject).GetComponent<RagdollAttachment>();
            clone.transform.Copy(transform, true, true, true, true);
            clone.Initialize();
            clone.DoActive(force, disable);
            gameObject.SetActive(false);
        }

        void DoActive(Vector3 force, bool disable)
        {
            rigid.isKinematic = false;
            collider.enabled = true;
            rigid.AddForceAtPosition(force, rigid.position + Random.onUnitSphere, ForceMode.Impulse);
            gameObject.layer = LayerMask.NameToLayer("Default");

            if (disable)
            {
                StartCoroutine(IEDisable());
            }
        }
        
        IEnumerator IEDisable()
        {
            yield return new WaitForSeconds(2f);
            Deactivate();
            while (transform.position.y > -10)
            {
                transform.position += Vector3.down * 10 * Time.deltaTime;
                yield return true;
            }
            gameObject.SetActive(false);
        }

        public void Deactivate()
        {
            rigid.isKinematic = true;
            collider.enabled = false;
            gameObject.SetActive(true);
        }
    }
}
#endif