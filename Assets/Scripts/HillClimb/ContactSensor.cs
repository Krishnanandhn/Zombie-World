using UnityEngine;

namespace PeakRover
{
    // Counts live collisions on a wheel so the car knows whether it is airborne.
    public sealed class ContactSensor : MonoBehaviour
    {
        public int Contacts;

        void OnCollisionEnter2D(Collision2D collision)
        {
            Contacts++;
        }

        void OnCollisionExit2D(Collision2D collision)
        {
            Contacts = Mathf.Max(0, Contacts - 1);
        }
    }
}
