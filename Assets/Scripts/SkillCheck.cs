using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillCheck : MonoBehaviour
{
    #region Variables
    [SerializeField]
    private RectTransform m_goodSkillCheck;

    [SerializeField]
    private int m_lowestRange;
    [SerializeField]
    private int m_highestRange;

    [SerializeField]
    private float m_maxValue;

    private VirusController m_virus;

    private Slider m_skillCheckSlider;

    private float m_currentValue;
    private float m_beginningGood;
    private float m_endGood;
    private float m_endGreat;

    private VirusController.ActionStates m_action;

    private const int BAD = 0;
    private const int GOOD = 1;
    private const int GREAT = 2;
    #endregion

    #region Events and Delegates
    public delegate void SkillCheckOutcome(int outcome, VirusController.ActionStates action);
    public static event SkillCheckOutcome SkillCheckOutcomeEvent;
    #endregion

    #region Initialization
    //-58 and 84 = 142
    void Awake() {
        m_skillCheckSlider = gameObject.GetComponent<Slider>();
    }
    #endregion

    #region Update
    void Update()
    {
        if (!m_virus.SkillChecking) {
            gameObject.SetActive(false);
            GameManager.m_singleton.GetSkillCheckHolder.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            float convertedValue = m_currentValue * 100 / m_maxValue;
            if (convertedValue >= m_beginningGood && convertedValue <= m_endGood) {
                SkillCheckOutcomeEvent(GOOD, m_action);
            }
            else if (convertedValue >= m_endGood && convertedValue <= m_endGreat) {
                SkillCheckOutcomeEvent(GREAT, m_action);
            }
            else {
                SkillCheckOutcomeEvent(BAD, m_action);
            }
            m_virus.SkillChecking = false;
            gameObject.SetActive(false);
            GameManager.m_singleton.GetSkillCheckHolder.SetActive(false);
        }
        m_currentValue += Time.deltaTime;
        m_skillCheckSlider.value = m_currentValue / m_maxValue;

        if (m_currentValue > m_maxValue) {
            SkillCheckOutcomeEvent(BAD, m_action);
            m_virus.SkillChecking = false;
            gameObject.SetActive(false);
            GameManager.m_singleton.GetSkillCheckHolder.SetActive(false);
        }
    }
    #endregion

    #region Setter
    public VirusController VirusController {
        set { m_virus = value; }
    }

    public VirusController.ActionStates Action {
        set { m_action = value; }
    }
    #endregion

    public void Initial() {
        if (!m_virus.SkillChecking) {
            gameObject.SetActive(false);
            GameManager.m_singleton.GetSkillCheckHolder.SetActive(false);
        }
        m_currentValue = 0;

        RandomizeSkillCheck();
    }

    public void LeftOnSkillCheck() {
        SkillCheckOutcomeEvent(BAD, m_action);
        m_virus.SkillChecking = false;
        GameManager.m_singleton.GetSkillCheckHolder.SetActive(false);
        gameObject.SetActive(false);
    }

    public void NullifySkillCheck() {
        m_virus.SkillChecking = false;
        GameManager.m_singleton.GetSkillCheckHolder.SetActive(false);
        gameObject.SetActive(false);
    }

    void RandomizeSkillCheck() {
        m_beginningGood = Random.Range(m_lowestRange, m_highestRange);

        m_endGood = m_beginningGood + 20;
        m_endGreat = m_endGood + (20 / 3);

        float value = m_beginningGood * 142f / 100 - 58;

        m_goodSkillCheck.anchoredPosition = new Vector3(value, 0, 0);
    }
}
