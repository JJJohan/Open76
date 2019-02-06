using UnityEngine;

namespace Assets.Scripts.Entities
{
    public class Sign : WorldEntity
    {
        private bool _dead;
        private Rigidbody _rigidbody;
        private MeshCollider[] _colliders;
        private Trigger _trigger;

        public override bool Alive
        {
            get { return !_dead; }
        }

        public Sign(GameObject gameObject) : base(gameObject)
        {
            _colliders = gameObject.GetComponentsInChildren<MeshCollider>();
            for (int i = 0; i < _colliders.Length; ++i)
            {
                _colliders[i].convex = true;
                _colliders[i].isTrigger = true;
                _colliders[i].gameObject.layer = LayerMask.NameToLayer("Sign");
            }

            _rigidbody = gameObject.GetComponent<Rigidbody>();
            if (_rigidbody == null)
            {
                _rigidbody = gameObject.AddComponent<Rigidbody>();
            }

            _trigger = gameObject.AddComponent<Trigger>();
            _trigger.OnTrigger += OnTriggerEnter;

            _rigidbody.mass = 1;
            _rigidbody.isKinematic = true;
        }

        public override void Destroy()
        {
            _trigger.OnTrigger -= OnTriggerEnter;
            base.Destroy();
        }

        private void OnTriggerEnter(Collider collider)
        {
            Rigidbody rigidBody = collider.gameObject.GetComponentInParent<Rigidbody>();
            if (rigidBody != null && rigidBody.velocity.magnitude > 2)
            {
                _dead = true;
                for (int i = 0; i < _colliders.Length; ++i)
                {
                    _colliders[i].isTrigger = false;
                    _colliders[i].gameObject.layer = 0;
                }
                _rigidbody.isKinematic = false;
                _rigidbody.AddForce(Vector3.up * 500);
            }
        }
    }
}