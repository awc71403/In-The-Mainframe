using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirewallAnimation : MonoBehaviour
{
    private FirewallController m_firewall;

    private void Awake() {
        m_firewall = GetComponentInParent<FirewallController>();
    }

    private void Recoil() {
        m_firewall.RecoilCall();
    }

    private void Resetter() {
        m_firewall.Resetter();
    }
}
