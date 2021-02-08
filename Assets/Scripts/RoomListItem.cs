using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoomListItem : MonoBehaviour
{
    [SerializeField]
    TMP_Text m_text;

    public RoomInfo m_info;

    public void SetUp(RoomInfo info) {
        m_info = info;
        m_text.text = info.Name;
    }

    public void OnClick() {
        Launcher.m_singleton.JoinRoom(m_info);
    }
}
