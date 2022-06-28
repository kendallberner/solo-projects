using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    public Animator transition;
    [Range(0.5f, 5.0f)]
    public float speed;

    public Image blocker;

    public void Start()
    {
        StartCoroutine(EnteringScene());
    }

    private IEnumerator EnteringScene()
    {
        blocker.enabled = true;

        float timeElapsed = 0f;
        while(timeElapsed < .5f)
        {
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        blocker.enabled = false;
        transition.SetTrigger("EnteringScene");
        transition.speed = speed;
    }

    public void PlayLevel(string levelName)
    {
        StartCoroutine(PlayLevelCo(levelName));
    }

    private IEnumerator PlayLevelCo(string levelName)
    {
        float timeElapsed = 0f;
        transition.SetTrigger("ExitingScene");
        transition.speed = speed;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelName);
        asyncLoad.allowSceneActivation = false;
        while (!asyncLoad.isDone && timeElapsed < 1f/speed)
        {
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;
    }
}
