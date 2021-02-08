using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscapeArea : MonoBehaviour
{
    #region OnTrigger
    void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag(GameManager.VirusTag)) {
            VirusController virus = other.gameObject.GetComponentInParent<VirusController>();
            if (virus) {
                //Only does it locally
                virus.EnteredMainFrame();   
                Debug.Log($"Virus {virus.GetPlayerManager.GetPhotonView.Owner.NickName} has successfully entered the system.");
            }
        }
    }
    #endregion
}
