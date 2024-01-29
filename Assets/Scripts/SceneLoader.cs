using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private bool isLoading = false;

    public void LoadSceneWithFade(string sceneName)
    {
        if (!isLoading)
        {
            StartCoroutine(LoadSceneWithFadeCoroutine(sceneName));
        }
    }

    private IEnumerator LoadSceneWithFadeCoroutine(string sceneName)
    {
        isLoading = true;

        // Load the target scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false; // Do not activate the scene immediately

        CanvasGroup canvasGroup = GetComponentInChildren<CanvasGroup>();

        // Wait until the scene is fully loaded
        while (!asyncLoad.isDone)
        {
            // Use asyncLoad.progress to get the loading progress if needed
            if (asyncLoad.progress >= 0.9f)
            {
                // Implement your fading effect here (you can use a UI canvas with an image and change its alpha)
                // For simplicity, let's assume you have a Canvas with a black image as a child.
                if (canvasGroup != null)
                {
                    float fadeDuration = 2f; // Adjust this value based on your desired fade duration
                    float currentTime = 0f;

                    while (currentTime < fadeDuration)
                    {
                        currentTime += Time.deltaTime;
                        canvasGroup.alpha = Mathf.Lerp(0f, 1f, currentTime / fadeDuration);
                        yield return null;
                    }
                }

                asyncLoad.allowSceneActivation = true; // Activate the scene
            }

            yield return null;
        }

        // Implement your fading effect here (fade out)
        if (canvasGroup != null)
        {
            float fadeOutDuration = 2f; // Adjust this value based on your desired fade out duration
            float currentTime = 0f;

            while (currentTime < fadeOutDuration)
            {
                currentTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, currentTime / fadeOutDuration);
                yield return null;
            }
        }

        // Reset isLoading
        isLoading = false;
    }
}
