using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ZombieSceneLoader : MonoBehaviour
{
    enum SceneState
    {
        kUnloaded,
        kUnloading,
        kLoaded,
        kLoading
    }

    [SerializeField]
    string m_SceneToLoadAndUnload = "";

    [SerializeField]
    Button m_Button;

    [SerializeField]
    Text m_ButtonText;


    SceneState m_sceneState = SceneState.kUnloaded;

    private void Start()
    {
        m_ButtonText.text = "LoadScene";
        m_Button.onClick.AddListener(CycleScene);
    }

    private void OnDestroy()
    {
        m_Button.onClick.RemoveAllListeners();
    }

    private void CycleScene()
    {
        switch (m_sceneState)
        {
            case SceneState.kUnloaded:
                StartCoroutine(LoadScene());
                break;
            case SceneState.kLoaded:
                StartCoroutine(UnloadScene());
                break;
            default:
                break;
        }
    }

    private IEnumerator UnloadScene()
    {
        m_sceneState = SceneState.kUnloading;
        m_Button.interactable = false;
        m_ButtonText.text = "Unloading";
        yield return SceneManager.UnloadSceneAsync(m_SceneToLoadAndUnload);
        yield return Resources.UnloadUnusedAssets();
        m_ButtonText.text = "LoadScene";
        m_Button.interactable = true;
        EventSystem.current.SetSelectedGameObject(m_Button.gameObject);
        m_sceneState = SceneState.kUnloaded;
    }

    private IEnumerator LoadScene()
    {
        m_sceneState = SceneState.kLoading;
        m_Button.interactable = false;
        m_ButtonText.text = "Loading";
        yield return SceneManager.LoadSceneAsync(m_SceneToLoadAndUnload, LoadSceneMode.Additive);
        m_ButtonText.text = "UnLoadScene";
        m_Button.interactable = true;
        EventSystem.current.SetSelectedGameObject(m_Button.gameObject);
        m_sceneState = SceneState.kLoaded;
    }
}
