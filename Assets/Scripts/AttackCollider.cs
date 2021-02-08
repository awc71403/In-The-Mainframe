using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCollider : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision) {
        VirusController hitVirus = collision.gameObject.GetComponentInParent<VirusController>();
        if (hitVirus && !hitVirus.Hurt) {
            GetComponent<Collider>().enabled = false;
            hitVirus.TakeDamage();
        }
    }

    private void OnTriggerEnter(Collider other) {
        VirusController hitVirus = other.gameObject.GetComponentInParent<VirusController>();
        if (hitVirus && !hitVirus.Hurt) {
            GetComponent<Collider>().enabled = false;
            hitVirus.TakeDamage();
        }
    }

    private void OnCollisionStay(Collision collision) {
        VirusController hitVirus = collision.gameObject.GetComponentInParent<VirusController>();
        if (hitVirus && !hitVirus.Hurt) {
            GetComponent<Collider>().enabled = false;
            hitVirus.TakeDamage();
        }
    }

    private void OnTriggerStay(Collider other) {
        VirusController hitVirus = other.gameObject.GetComponentInParent<VirusController>();
        if (hitVirus && !hitVirus.Hurt) {
            hitVirus.TakeDamage();
        }
    }

    private void OnCollisionExit(Collision collision) {
        VirusController hitVirus = collision.gameObject.GetComponentInParent<VirusController>();
        if (hitVirus && !hitVirus.Hurt) {
            GetComponent<Collider>().enabled = false;
            hitVirus.TakeDamage();
        }
    }

    private void OnTriggerExit(Collider other) {
        VirusController hitVirus = other.gameObject.GetComponentInParent<VirusController>();
        if (hitVirus && !hitVirus.Hurt) {
            GetComponent<Collider>().enabled = false;
            hitVirus.TakeDamage();
        }
    }
}
