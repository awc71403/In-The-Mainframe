using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager m_singleton;

    [SerializeField]
    Menu[] m_menus;

    private void Awake() {
        m_singleton = this;
    }

    public void OpenMenu(string menuName) {
        for (int i = 0; i < m_menus.Length; i++) {
            if (m_menus[i].menuName == menuName) {
                m_menus[i].Open();
            }
            else if (m_menus[i].open) {
                CloseMenu(m_menus[i]);
            }
        }
    }

    public void OpenMenu(Menu menu) {
        for (int i = 0; i < m_menus.Length; i++) {
            if (m_menus[i].open) {
                CloseMenu(m_menus[i]);
            }
        }
        menu.Open();
    }

    public void CloseMenu(Menu menu) {
        menu.Close();
    }
}
