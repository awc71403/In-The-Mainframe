using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrorHitbox : MonoBehaviour
{
    //Figure out reference
    private PhotonView m_PV;

    private const int TerrorRadius = 60;

    private const float MinimumDelay = .75f;
    private const float MaximumDelay = 2.75f;

    private void Awake() {
        m_PV = GetComponentInParent<PhotonView>();
    }

    private void OnTriggerEnter(Collider other) {
        if (!m_PV.IsMine && other.gameObject.CompareTag(GameManager.VirusTag)) {
            VirusController hitVirus = other.gameObject.GetComponentInParent<VirusController>();
            if (hitVirus) {
                float volume = CalculateVolume(hitVirus);
                hitVirus.TerrorSound(MaximumDelay - (MaximumDelay - MinimumDelay) * volume, volume, false);
            }
        }
    }

    private void OnTriggerStay(Collider other) {
        if (!m_PV.IsMine && other.gameObject.CompareTag(GameManager.VirusTag)) {
            VirusController hitVirus = other.gameObject.GetComponentInParent<VirusController>();
            if (hitVirus) {
                float volume = CalculateVolume(hitVirus);
                hitVirus.TerrorSound(MaximumDelay - (MaximumDelay - MinimumDelay) * volume, volume, false);
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if (!m_PV.IsMine && other.gameObject.CompareTag(GameManager.VirusTag)) {
            VirusController hitVirus = other.gameObject.GetComponentInParent<VirusController>();
            if (hitVirus) {
                hitVirus.TerrorSound(0, 0, true);
            }
        }
    }

    private float CalculateVolume(VirusController virus) {
        GameObject virusLocation = virus.GetComponentInChildren<Rigidbody>().gameObject;
        float dist = Vector3.Distance(this.transform.position, virusLocation.transform.position);
        return 1 - dist / TerrorRadius;
    }
}
