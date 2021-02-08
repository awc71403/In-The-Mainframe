using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VirusListItem : MonoBehaviourPunCallbacks {
    PhotonView m_PV;
    VirusController m_virus;
    TextMeshProUGUI m_playerName;
    [SerializeField]
    Image m_playerHP;
    Slider m_playerDownedSlider;

    float m_currentDownedTimer;
    float m_maxDownedTimer;

    private const int maxDownedTimes = 3;

    void Awake() {
        m_playerName = GetComponentInChildren<TextMeshProUGUI>();
        m_playerDownedSlider = GetComponentInChildren<Slider>();
        m_playerDownedSlider.gameObject.SetActive(false);
    }

    void Update() {
        if (m_playerDownedSlider.gameObject.activeSelf) {
            m_currentDownedTimer -= Time.deltaTime;
            UpdateSlider();
        }
    }

    public void SetUp(Player player) {
        m_playerName.text = player.NickName;
        m_playerHP.color = new Color(0, 1, 0);
        gameObject.transform.SetParent(GameManager.m_singleton.GetVirusList);
    }

    public VirusController VirusController {
        set { m_virus = value; }
    }

    public float MaxDownedTimer {
        set { m_maxDownedTimer = value; }
    }

    public void UpdateHealthUI(int health, int timesDowned) {
        if (timesDowned > maxDownedTimes) {
            m_playerHP.color = new Color(0, 0, 0, 0);
            m_virus.Death();
            GameManager.m_singleton.AddDeadVirus();
        }
        else {
            switch (health) {
                case 2:
                    m_playerHP.color = new Color(0, 1, 0);
                    //Tell player what the current value to store is
                    m_playerDownedSlider.gameObject.SetActive(false);
                    break;
                case 1:
                    m_playerHP.color = new Color(1, 0, 0);
                    //Tell player what the current value to store is
                    m_playerDownedSlider.gameObject.SetActive(false);
                    break;
                case 0:
                    m_playerHP.color = new Color(0, 0, 0);
                    m_currentDownedTimer = m_maxDownedTimer - (m_maxDownedTimer * (timesDowned - 1) / maxDownedTimes);
                    m_playerDownedSlider.gameObject.SetActive(true);
                    break;
            }
        }
    }

    void UpdateSlider() {
        m_playerDownedSlider.value = m_currentDownedTimer / m_maxDownedTimer;
        if (m_playerDownedSlider.value <= 0) {
            m_virus.Death();
            //If all virus are dead or have left, reset game.
            GameManager.m_singleton.AddDeadVirus();
        }
    }
}
