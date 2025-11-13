using UnityEngine;

namespace Props.Pickups
{
    public class Pickup : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            var root = other.transform.root.gameObject;
            if (root.CompareTag("Player"))
            {
                PlayerPickedUp(root);
            }
        }
        
        // Making this a virtual method is *probably* overkill here
        // but, I feel like I might change the way pickups work in the future
        // so it's a nice enough hook
        protected virtual void PlayerPickedUp(GameObject player)
        {
        }
    }
}