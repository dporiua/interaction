using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class ImageUpdater : MonoBehaviour
{
    [Header("Google Script URL")]
    public string googleScriptURL;

    [Header("Sprite Renderer")]
    public SpriteRenderer targetSpriteRenderer;

    private string currentFileName = "";

    void Start()
    {
        StartCoroutine(FetchInitialImage());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F7))
        {
            StartCoroutine(CheckAndUpdateImage());
        }
    }

    private IEnumerator FetchInitialImage()
    {
        Debug.Log("Fetching base imafge from google drive...");

        UnityWebRequest request = UnityWebRequest.Get(googleScriptURL);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Fetched file list successfully");

            // Parse the JSON response
            string jsonResponse = request.downloadHandler.text;
            List<ImageMetadata> files = JsonUtility.FromJson<ImageMetadataList>($"{{\"files\":{jsonResponse}}}").files;

            if (files.Count > 0)
            {
                // Use the first file in the list
                currentFileName = files[0].fileName;
                yield return StartCoroutine(UpdateSprite(files[0].downloadUrl));
            }
            else
            {
                Debug.Log("No images found in the folder.");
            }
        }
        else
        {
            Debug.LogError("Error fetching base image: " + request.error);
        }
    }

    private IEnumerator CheckAndUpdateImage()
    {
        Debug.Log("Checking for image updates...");

        UnityWebRequest request = UnityWebRequest.Get(googleScriptURL);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Fetched file list successfully!");

            // Parse the JSON response
            string jsonResponse = request.downloadHandler.text;
            List<ImageMetadata> files = JsonUtility.FromJson<ImageMetadataList>($"{{\"files\":{jsonResponse}}}").files;

            if (files.Count > 0 && files[0].fileName != currentFileName)
            {
                Debug.Log($"Image file changed. Updating sprite... New file name: {files[0].fileName}");

                currentFileName = files[0].fileName;
                yield return StartCoroutine(UpdateSprite(files[0].downloadUrl));
            }
            else
            {
                Debug.Log("No new image found.");
            }
        }
        else
        {
            Debug.LogError("Error checking for image updates: " + request.error);
        }
    }

    private IEnumerator UpdateSprite(string imageUrl)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Image downloaded successfully!");

            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            targetSpriteRenderer.sprite = newSprite;
        }
        
    }
}

[System.Serializable]
public class ImageMetadata
{
    public string fileName;
    public string downloadUrl;
}

[System.Serializable]
public class ImageMetadataList
{
    public List<ImageMetadata> files;
}
