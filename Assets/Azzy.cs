using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

/// <summary>
/// Azzy - Based on a powerful, esay-to-use image downloading and caching library for Unity in Run-Time with blackjack and Davinci
/// v 1.1
/// Developed by ShamsDEV.com
/// copyright (c) ShamsDEV.com All Rights Reserved.
/// Licensed under the MIT License.
/// https://github.com/shamsdev/davinci
/// </summary>
public class Azzy : MonoBehaviour
{
    private static bool ENABLE_GLOBAL_LOGS = true;

    private bool enableLog = false;
    private float fadeTime = 1;
    private bool cached = true;

    private enum RendererType
    {
        none,
        uiImage,
        renderer
    }

    private RendererType rendererType = RendererType.none;
    private GameObject targetObj;
    private string url = null;

    private Texture2D loadingPlaceholder, errorPlaceholder;
    private Sprite loadingPlaceholderSprite, errorPlaceholderSprite;

    private UnityAction onStartAction,
        onDownloadedAction,
        OnLoadedAction,
        onEndAction;

    private UnityAction<int> onDownloadProgressChange;
    private UnityAction<string> onErrorAction;

    private static Dictionary<string, Azzy> underProcessAzzyes
        = new Dictionary<string, Azzy>();

    private string uniqueHash;
    private int progress;

    private bool success = false;

    static string filePath = Application.persistentDataPath + "/" +
             "Azzy" + "/";


    /// <summary>
    /// Get instance of Azzy class
    /// </summary>
    public static Azzy get()
    {
        return new GameObject("Azzy").AddComponent<Azzy>();
    }

    /// <summary>
    /// Set image url for download.
    /// </summary>
    /// <param name="url">Image Url</param>
    /// <returns></returns>
    public Azzy load(string url)
    {
        if (enableLog)
            Debug.Log("[Azzy] Url set : " + url);

        this.url = url;
        return this;
    }

    /// <summary>
    /// Set fading animation time.
    /// </summary>
    /// <param name="fadeTime">Fade animation time. Set 0 for disable fading.</param>
    /// <returns></returns>
    public Azzy setFadeTime(float fadeTime)
    {
        if (enableLog)
            Debug.Log("[Azzy] Fading time set : " + fadeTime);

        this.fadeTime = fadeTime;
        return this;
    }

    /// <summary>
    /// Set target Image component.
    /// </summary>
    /// <param name="image">target Unity UI image component</param>
    /// <returns></returns>
    public Azzy into(Image image)
    {
        if (enableLog)
            Debug.Log("[Azzy] Target as UIImage set : " + image);

        rendererType = RendererType.uiImage;
        this.targetObj = image.gameObject;
        return this;
    }

    /// <summary>
    /// Set target Renderer component.
    /// </summary>
    /// <param name="renderer">target renderer component</param>
    /// <returns></returns>
    public Azzy into(Renderer renderer)
    {
        if (enableLog)
            Debug.Log("[Azzy] Target as Renderer set : " + renderer);

        rendererType = RendererType.renderer;
        this.targetObj = renderer.gameObject;
        return this;
    }

    #region Actions
    public Azzy withStartAction(UnityAction action)
    {
        this.onStartAction = action;

        if (enableLog)
            Debug.Log("[Azzy] On start action set : " + action);

        return this;
    }

    public Azzy withDownloadedAction(UnityAction action)
    {
        this.onDownloadedAction = action;

        if (enableLog)
            Debug.Log("[Azzy] On downloaded action set : " + action);

        return this;
    }

    public Azzy withDownloadProgressChangedAction(UnityAction<int> action)
    {
        this.onDownloadProgressChange = action;

        if (enableLog)
            Debug.Log("[Azzy] On download progress changed action set : " + action);

        return this;
    }

    public Azzy withLoadedAction(UnityAction action)
    {
        this.OnLoadedAction = action;

        if (enableLog)
            Debug.Log("[Azzy] On loaded action set : " + action);

        return this;
    }

    public Azzy withErrorAction(UnityAction<string> action)
    {
        this.onErrorAction = action;

        if (enableLog)
            Debug.Log("[Azzy] On error action set : " + action);

        return this;
    }

    public Azzy withEndAction(UnityAction action)
    {
        this.onEndAction = action;

        if (enableLog)
            Debug.Log("[Azzy] On end action set : " + action);

        return this;
    }
    #endregion

    /// <summary>
    /// Show or hide logs in console.
    /// </summary>
    /// <param name="enable">'true' for show logs in console.</param>
    /// <returns></returns>
    public Azzy setEnableLog(bool enableLog)
    {
        this.enableLog = enableLog;

        if (enableLog)
            Debug.Log("[Azzy] Logging enabled : " + enableLog);

        return this;
    }

    /// <summary>
    /// Set the sprite of image when Azzy is downloading and loading image
    /// </summary>
    /// <param name="loadingPlaceholder">loading texture</param>
    /// <returns></returns>
    public Azzy setLoadingPlaceholder(Texture2D loadingPlaceholder)
    {
        this.loadingPlaceholder = loadingPlaceholder;

        if (enableLog)
            Debug.Log("[Azzy] Loading placeholder has been set.");

        return this;
    }

    /// <summary>
    /// Set the sprite of image when Azzy is downloading and loading image
    /// </summary>
    /// <param name="loadingPlaceholder">loading texture</param>
    /// <returns></returns>
    public Azzy setLoadingPlaceholder(Sprite loadingPlaceholder)
    {
        this.loadingPlaceholder = null;

        if (enableLog)
            Debug.Log("[Azzy] Loading placeholder has been set.");

        return this;
    }

    /// <summary>
    /// Set image sprite when some error occurred during downloading or loading image
    /// </summary>
    /// <param name="errorPlaceholder">error texture</param>
    /// <returns></returns>
    public Azzy setErrorPlaceholder(Texture2D errorPlaceholder)
    {
        this.errorPlaceholder = errorPlaceholder;

        if (enableLog)
            Debug.Log("[Azzy] Error placeholder has been set.");

        return this;
    }

    /// <summary>
    /// Set image sprite when some error occurred during downloading or loading image
    /// </summary>
    /// <param name="errorPlaceholder">error texture</param>
    /// <returns></returns>
    public Azzy setErrorPlaceholder(Sprite errorPlaceholder)
    {
        this.errorPlaceholderSprite = errorPlaceholder;

        if (enableLog)
            Debug.Log("[Azzy] Error placeholder has been set.");

        return this;
    }

    /// <summary>
    /// Enable cache
    /// </summary>
    /// <returns></returns>
    public Azzy setCached(bool cached)
    {
        this.cached = cached;

        if (enableLog)
            Debug.Log("[Azzy] Cache enabled : " + cached);

        return this;
    }

    /// <summary>
    /// Start Azzy process.
    /// </summary>
    public void start()
    {
        if (url == null)
        {
            error("Url has not been set. Use 'load' funtion to set image url.");
            return;
        }

        try
        {
            Uri uri = new Uri(url);
            this.url = uri.AbsoluteUri;
        }
        catch (Exception ex)
        {
            error("Url is not correct.");
            return;
        }

        if (rendererType == RendererType.none || targetObj == null)
        {
            error("Target has not been set. Use 'into' function to set target component.");
            return;
        }

        if (enableLog)
            Debug.Log("[Azzy] Start Working.");

        if (loadingPlaceholder != null)
            SetLoadingImage();

        if (onStartAction != null)
            onStartAction.Invoke();

        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }

        uniqueHash = CreateMD5(url);

        if (underProcessAzzyes.ContainsKey(uniqueHash))
        {
            Azzy sameProcess = underProcessAzzyes[uniqueHash];
            sameProcess.onDownloadedAction += () =>
            {
                if (onDownloadedAction != null)
                    onDownloadedAction.Invoke();

                loadSpriteToImage();
            };
        }
        else
        {
            if (File.Exists(filePath + uniqueHash))
            {
                if (onDownloadedAction != null)
                    onDownloadedAction.Invoke();

                loadSpriteToImage();
            }
            else
            {
                underProcessAzzyes.Add(uniqueHash, this);
                StopAllCoroutines();
                StartCoroutine("Downloader");
            }
        }
    }

    IEnumerator Downloader()
    {
        if(enableLog)
            Debug.Log("[Azzy] Download started.");

        UnityWebRequest wr = UnityWebRequestTexture.GetTexture(url);
        yield return wr.SendWebRequest();

        if (wr.isHttpError || wr.isNetworkError)
        {
            error("Error while downloading the image : " + wr.error);
            yield break;
        }

        progress = Mathf.FloorToInt(wr.downloadProgress * 100);
        if (onDownloadProgressChange != null)
            onDownloadProgressChange.Invoke(progress);

        if (enableLog)
            Debug.Log("[Azzy] Downloading progress : " + progress + "%");

        if (!wr.isHttpError || !wr.isNetworkError)
            File.WriteAllBytes(filePath + uniqueHash, wr.downloadHandler.data);  

        yield return null;

        if (onDownloadedAction != null)
            onDownloadedAction.Invoke();

        loadSpriteToImage();

        underProcessAzzyes.Remove(uniqueHash);
    }

    private void loadSpriteToImage()
    {
        progress = 100;
        if (onDownloadProgressChange != null)
            onDownloadProgressChange.Invoke(progress);

        if (enableLog)
            Debug.Log("[Azzy] Downloading progress : " + progress + "%");

        if (!File.Exists(filePath + uniqueHash))
        {
            error("Loading image file has been failed.");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(ImageLoader(null as Texture2D));
    }

    private void SetLoadingImage(Sprite sprites = null)
    {
        switch (rendererType)
        {
            case RendererType.renderer:
                Renderer renderer = targetObj.GetComponent<Renderer>();
                renderer.material.mainTexture = loadingPlaceholder;
                break;

            case RendererType.uiImage:
                Image image = targetObj.GetComponent<Image>();
                if (sprites == null)
                {
                    Sprite sprite = Sprite.Create(loadingPlaceholder,
                         new Rect(0, 0, loadingPlaceholder.width, loadingPlaceholder.height),
                         new Vector2(0.5f, 0.5f));
                    image.sprite = sprite;
                }
                else
                {
                    image.sprite = sprites;
                }
                break;
        }
    }

    private IEnumerator ImageLoader(Texture2D texture = null)
    {
        if (enableLog)
            Debug.Log("[Azzy] Start loading image.");

        if (texture == null)
        {
            byte[] fileData;
            fileData = File.ReadAllBytes(filePath + uniqueHash);
            texture = new Texture2D(2, 2);
            //ImageConversion.LoadImage(texture, fileData);
            texture.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }

        Color color;

        if (targetObj != null)
            switch (rendererType)
            {
                case RendererType.renderer:
                    Renderer renderer = targetObj.GetComponent<Renderer>();

                    if (renderer == null || renderer.material == null)
                        break;

                    renderer.material.mainTexture = texture;
                    float maxAlpha;

                    if (fadeTime > 0 && renderer.material.HasProperty("_Color"))
                    {
                        color = renderer.material.color;
                        maxAlpha = color.a;

                        color.a = 0;

                        renderer.material.color = color;
                        float time = Time.time;
                        while (color.a < maxAlpha)
                        {
                            color.a = Mathf.Lerp(0, maxAlpha, (Time.time - time) / fadeTime);

                            if (renderer != null)
                                renderer.material.color = color;

                            yield return null;
                        }
                    }

                    break;

                case RendererType.uiImage:
                    Image image = targetObj.GetComponent<Image>();

                    if (image == null)
                        break;

                    Sprite sprite = Sprite.Create(texture,
                         new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                    image.sprite = sprite;
                    color = image.color;
                    maxAlpha = color.a;

                    if (fadeTime > 0)
                    {
                        color.a = 0;
                        image.color = color;

                        float time = Time.time;
                        while (color.a < maxAlpha)
                        {
                            color.a = Mathf.Lerp(0, maxAlpha, (Time.time - time) / fadeTime);

                            if (image != null)
                                image.color = color;
                            yield return null;
                        }
                    }
                    break;
            }

        if (OnLoadedAction != null)
            OnLoadedAction.Invoke();

        if (enableLog)
            Debug.Log("[Azzy] Image has been loaded.");

        success = true;
        finish();
    }

    private IEnumerator ImageLoader(Sprite texture = null)
    {
        if (enableLog)
            Debug.Log("[Azzy] Start loading image.");

        if (texture == null)
        {
            byte[] fileData;
            fileData = File.ReadAllBytes(filePath + uniqueHash);
            // Update this
            //texture = new Texture2D(2, 2);
            //ImageConversion.LoadImage(texture, fileData);
            // Update this
            //texture.LoadImage(fileData); //..this will auto-resize the texture dimensions.

            // Trying to convert byte to Sprite
            Texture2D texture2d = new Texture2D(10, 10);
            texture2d.LoadImage(fileData);
            texture = Sprite.Create(texture2d,
                new Rect(0, 0, texture2d.width, texture2d.height), new Vector2(0.5f, 0.5f));
        }

        Color color;

        if (targetObj != null)
            switch (rendererType)
            {
                case RendererType.renderer:
                    Renderer renderer = targetObj.GetComponent<Renderer>();

                    if (renderer == null || renderer.material == null)
                        break;

                    // Update this
                   // renderer.material.mainTexture = texture;
                    float maxAlpha;

                    if (fadeTime > 0 && renderer.material.HasProperty("_Color"))
                    {
                        color = renderer.material.color;
                        maxAlpha = color.a;

                        color.a = 0;

                        renderer.material.color = color;
                        float time = Time.time;
                        while (color.a < maxAlpha)
                        {
                            color.a = Mathf.Lerp(0, maxAlpha, (Time.time - time) / fadeTime);

                            if (renderer != null)
                                renderer.material.color = color;

                            yield return null;
                        }
                    }

                    break;

                case RendererType.uiImage:
                    Image image = targetObj.GetComponent<Image>();

                    if (image == null)
                        break;

                    Sprite sprite = texture;

                    image.sprite = sprite;
                    color = image.color;
                    maxAlpha = color.a;

                    if (fadeTime > 0)
                    {
                        color.a = 0;
                        image.color = color;

                        float time = Time.time;
                        while (color.a < maxAlpha)
                        {
                            color.a = Mathf.Lerp(0, maxAlpha, (Time.time - time) / fadeTime);

                            if (image != null)
                                image.color = color;
                            yield return null;
                        }
                    }
                    break;
            }

        if (OnLoadedAction != null)
            OnLoadedAction.Invoke();

        if (enableLog)
            Debug.Log("[Azzy] Image has been loaded.");

        success = true;
        finish();
    }

    public static string CreateMD5(string input)
    {
        // Use input string to calculate MD5 hash
        using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
        {
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }

    private void error(string message)
    {
        success = false;

        if (enableLog)
            Debug.LogError("[Azzy] Error : " + message);

        if (onErrorAction != null)
            onErrorAction.Invoke(message);

        if (errorPlaceholder != null)
            StartCoroutine(ImageLoader(errorPlaceholder));
        if (errorPlaceholderSprite != null)
            StartCoroutine(ImageLoader(errorPlaceholderSprite));
        else finish();
    }

    private void finish()
    {
        if (enableLog)
            Debug.Log("[Azzy] Operation has been finished.");

        if (!cached)
        {
            try
            {
                File.Delete(filePath + uniqueHash);
            }
            catch (Exception ex)
            {
                if (enableLog)
                    Debug.LogError($"[Azzy] Error while removing cached file: {ex.Message}");
            }
        }

        if (onEndAction != null)
            onEndAction.Invoke();

        Invoke("destroyer", 0.5f);
    }

    private void destroyer()
    {
        Destroy(gameObject);
    }


    /// <summary>
    /// Clear a certain cached file with its url
    /// </summary>
    /// <param name="url">Cached file url.</param>
    /// <returns></returns>
    public static void ClearCache(string url)
    {
        try
        {
            File.Delete(filePath + CreateMD5(url));

            if (ENABLE_GLOBAL_LOGS)
                Debug.Log($"[Azzy] Cached file has been cleared: {url}");
        }
        catch (Exception ex)
        {
            if (ENABLE_GLOBAL_LOGS)
                Debug.LogError($"[Azzy] Error while removing cached file: {ex.Message}");
        }
    }

    /// <summary>
    /// Clear all Azzy cached files
    /// </summary>
    /// <returns></returns>
    public static void ClearAllCachedFiles()
    {
        try
        {
            Directory.Delete(filePath, true);

            if (ENABLE_GLOBAL_LOGS)
                Debug.Log("[Azzy] All Azzy cached files has been cleared.");
        }
        catch (Exception ex)
        {
            if (ENABLE_GLOBAL_LOGS)
                Debug.LogError($"[Azzy] Error while removing cached file: {ex.Message}");
        }
    }
}
