using UnityEngine;

namespace Assets.Scripts
{
    public class Trigger : MonoBehaviour
    {
        public delegate void TriggerHandler(Collider collider);
        public event TriggerHandler OnTrigger;

        private void OnTriggerEnter(Collider collider)
        {
            OnTrigger?.Invoke(collider);
        }
    }
}
