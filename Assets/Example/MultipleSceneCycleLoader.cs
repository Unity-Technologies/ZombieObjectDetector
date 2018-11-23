using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MultipleSceneCycleLoader : MonoBehaviour
{

    [SerializeField]
    string m_SceneToLoadAndUnload = "";

    [SerializeField]
    int m_NumberOfTimesToLoadAndUnload = 100000;
    [SerializeField]
    Button m_Button;
    [SerializeField]
    Text m_ButtonText;

    private bool m_IsCycling = false;

    private void Start()
    {
        m_ButtonText.text = string.Format("Load and Unload {0} {1} times", m_SceneToLoadAndUnload, m_NumberOfTimesToLoadAndUnload);
        m_Button.onClick.AddListener(StartSceneCycling);
    }

    private void OnDestroy()
    {
        m_Button.onClick.RemoveAllListeners();
    }

    private void OnGUI()
    {
        if (m_IsCycling)
        {
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "Logging Zombie Objects!");
        }
    }

    private void StartSceneCycling()
    {
        StartCoroutine(LoadUnloadScenes());
    }

    private IEnumerator LoadUnloadScenes()
    {
        m_IsCycling = true;
        m_Button.interactable = false;

        for (int i = m_NumberOfTimesToLoadAndUnload; i > 0; i--)
        {
            m_ButtonText.text = string.Format("{0} Loads Remaining", i);

            yield return SceneManager.LoadSceneAsync(m_SceneToLoadAndUnload, LoadSceneMode.Additive);

            yield return SceneManager.UnloadSceneAsync(m_SceneToLoadAndUnload);

        }

        yield return Resources.UnloadUnusedAssets();

        m_ButtonText.text = string.Format("Load and Unload {0} {1} times", m_SceneToLoadAndUnload, m_NumberOfTimesToLoadAndUnload);
        m_Button.interactable = true;
        m_IsCycling = false;

    }

}
