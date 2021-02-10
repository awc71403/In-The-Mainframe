using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Spectator : MonoBehaviour
{
    private bool m_enabled;
    private int m_spectating;
    private int m_remainder;

    private void Awake() {
        m_spectating = 0;
        m_remainder = 0;
    }

    private void Update() {
        Debug.Log("Spectator Update");
        Debug.Log($"m_enabled: {m_enabled}");
        if (m_enabled && Input.GetMouseButtonDown(0)) {
            Debug.Log("Left");
            m_spectating--;
            Transfer();
        } else if (m_enabled && Input.GetMouseButtonDown(1)) {
            Debug.Log("Right");
            m_spectating++;
            Transfer();
        }
    }

    public void Enable() {
        m_enabled = true;
        transform.SetParent(PlayerList.m_singleton.GetVirusesTransform.GetComponentsInChildren<VirusController>()[m_remainder].GetCameraHolder.transform);
        transform.localPosition = new Vector3(0, 1, -3);
        transform.localRotation = Quaternion.identity;
        GameManager.m_singleton.Spectating(GetComponentInParent<VirusController>().GetPlayerManager.GetPhotonView.Owner.NickName);
    }

    public void Change() {
        m_spectating++;
        Transfer();
    }

    private void Transfer() {
        int viruses = PlayerList.m_singleton.GetVirusesTransform.GetComponentsInChildren<VirusController>().Length;
        if (viruses > 0) {
            if (m_spectating >= 0) {
                m_remainder = m_spectating % viruses;
            }
            else {
                m_spectating += viruses;
                m_remainder = m_spectating % viruses;
            }
            transform.SetParent(PlayerList.m_singleton.GetVirusesTransform.GetComponentsInChildren<VirusController>()[m_remainder].GetCameraHolder.transform);
            transform.localPosition = new Vector3(0, 1, -3);
            transform.localRotation = Quaternion.identity;
            GameManager.m_singleton.Spectating(GetComponentInParent<VirusController>().GetPlayerManager.GetPhotonView.Owner.NickName);
        }
    }
}
