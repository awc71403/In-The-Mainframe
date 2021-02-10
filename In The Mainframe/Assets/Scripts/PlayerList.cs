using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerList : MonoBehaviour
{
    public static PlayerList m_singleton;

    [SerializeField]
    private Transform m_viruses;
    [SerializeField]
    private Transform m_firewalls;

    void Awake() {
        m_singleton = this;
    }

    #region Getters
    public Transform GetVirusesTransform {
        get { return m_viruses; }
    }

    public Transform GetFirewallsTransform {
        get { return m_firewalls; }
    }
    #endregion
}
