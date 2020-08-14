using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour {

    [SerializeField]
    string m_resetScene = "Reset";

    [SerializeField]
    Button m_Button;

    private void Start()
    {
        m_Button.onClick.AddListener(LoadScene);
    }

    private void OnDestroy()
    {
        m_Button.onClick.RemoveAllListeners();
    }

    private void LoadScene()
    {
        SceneManager.LoadScene(m_resetScene);
    }
}
