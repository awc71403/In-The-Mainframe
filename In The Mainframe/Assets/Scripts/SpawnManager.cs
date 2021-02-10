using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager m_singleton;

    [SerializeField]
    private GameObject m_virusSpawnHolder;

    [SerializeField]
    private GameObject m_firewallSpawnHolder;

    SpawnPoint[] m_virusSpawnPoints;
    SpawnPoint[] m_firewallSpawnPoints;

    void Awake() {
        m_singleton = this;
        m_virusSpawnPoints = m_virusSpawnHolder.GetComponentsInChildren<SpawnPoint>();
        m_firewallSpawnPoints = m_firewallSpawnHolder.GetComponentsInChildren<SpawnPoint>();
    }

    public Transform GetVirusSpawnPoint() {
        return m_virusSpawnPoints[Random.Range(0, m_virusSpawnPoints.Length)].transform;
    }

    public Transform GetFirewallSpawnPoint() {
        return m_firewallSpawnPoints[Random.Range(0, m_firewallSpawnPoints.Length)].transform;
    }
}
