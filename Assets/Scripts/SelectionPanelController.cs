using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using UnityEngine.Networking;
using UnityEngine.Audio;

public class SelectionPanelController : MonoBehaviour
{
    public GameObject button; // default button to display the selectable microphone sources
    public AudioMixerSnapshot snap; // audiomixer snapshot with no attenuation

    AudioSource audioSource; // Unique audioSource of the project

    InputField inputField; // InputField where the path is displayed
    Transform scrollView; // scrollView where the current disrectory content is displayed
    Text infoText; // infoText at the bottom of the ScrollView to display some data
    
    int nbLines; // number of lines we want to display in the scrollView (deos not really work in the end)
    float buttonHeight; // height of the button gameObject
    Vector2 uiRatio; // to get the screen ratio from the default config


    // Start is called before the first frame update
    void Start()
    {
        // Get the scrollView, the inputField, the infoText and the audioSource
        inputField = transform.Find("InputField").GetComponent<InputField>();
        scrollView = transform.Find("Scroll View");
        infoText = transform.Find("InfoText").GetComponent<Text>();
        audioSource = FindObjectOfType<AudioSource>();

        // Compute the UIRatio
        Vector2 refRes = FindObjectOfType<CanvasScaler>().referenceResolution;
        uiRatio = new Vector2(Screen.width / refRes.x, Screen.height / refRes.y);
        // Clamping uiRatio.y to limit the button size
        uiRatio.y = Mathf.Clamp(uiRatio.y, 1f, 1.2f);

        // number of elements displayed in the viewport
        nbLines = 10;
        // height of the buttons for displaying nbLines buttons in the viewport
        buttonHeight = (scrollView.GetComponent<RectTransform>().offsetMax.y - scrollView.GetComponent<RectTransform>().offsetMin.y) * uiRatio.y / nbLines;

        // Texts init
        inputField.text = "C:/";
        infoText.text = "";

        // Display content of C: directory at first
        DisplayFiles(inputField.text);
    }

    // Function to display the directory content chosen by typing a path in the InputField
    public void PathChangedManually()
    {
        DisplayFiles(inputField.text);
    }
    
    // Display the content of a directory
    public void DisplayFiles(string path)
    {
        if (!Directory.Exists(path)) return; // In case of error in the directory name

        // This is to ensure that a root path has a slash at the end, to avoid adding a repository name to it without (eg. C:Windows)
        if(path.Length < 3 && !(path.EndsWith("/") || path.EndsWith("\\")))
        {
            path += "/";
        }
        inputField.text = path;

        // Destroy all the buttons in the scrollView if any to clean the space before adding new ones
        int count = scrollView.Find("Viewport").Find("Content").childCount;
        while (count > 0)
        {
            Destroy(scrollView.Find("Viewport").Find("Content").GetChild(count-1).gameObject);
            count--;
        }
        scrollView.Find("Viewport").Find("Content").GetComponent<RectTransform>().offsetMin = new Vector2(0, 0f); // Set the size of the viewport to 0

        int numFile = 0;

        // Init with "../"
        GameObject prevButton = Instantiate(button);
        prevButton.transform.SetParent(scrollView.Find("Viewport").Find("Content"));
        prevButton.transform.localPosition = new Vector3(2f, -buttonHeight * numFile - 2f, 0f);
        prevButton.transform.localScale = new Vector3(1f, uiRatio.y, 1f);
        prevButton.GetComponentInChildren<Text>().text = "../";
        // Get the previous directory in the path
        string prevDir = path;
        if (prevDir.Length > 4)
        {
            prevDir = Directory.GetParent(prevDir).FullName;
        }
        prevButton.GetComponent<Button>().onClick.AddListener(() => { DisplayFiles(prevDir); }); // add a link to display the previous directory in the path if button is clicked
        numFile++;

        // Try to acces directory and exit the function if an exception is returned
        try
        {
            Directory.GetDirectories(path);
        }
        catch (UnauthorizedAccessException ex)
        {
            Debug.Log("Unauthorized Access Exception on " + path);
            return;
        }

        // For each directory found: create a button, scale it and assign it the function to display its own content
        foreach (string dirInDir in Directory.GetDirectories(path))
        {
            GameObject tempButton = Instantiate(button);
            tempButton.transform.SetParent(scrollView.Find("Viewport").Find("Content"));
            tempButton.transform.localPosition = new Vector3(2f, -buttonHeight * numFile - 2f, 0f);
            tempButton.transform.localScale = new Vector3(1f, uiRatio.y, 1f);
            tempButton.GetComponentInChildren<Text>().text = "./" + Path.GetFileNameWithoutExtension(dirInDir) + "/";
            tempButton.GetComponent<Button>().onClick.AddListener(() => { DisplayFiles(dirInDir); });
            numFile++;
        }

        // For each mp3, ogg and wav file found: create a button, scale it and assign it the function to select this file
        foreach (string fileInDir in Directory.GetFiles(path))
        {
            if (fileInDir.EndsWith(".mp3") || fileInDir.EndsWith(".ogg") || fileInDir.EndsWith(".wav"))
            {
                GameObject tempButton = Instantiate(button);
                tempButton.transform.SetParent(scrollView.Find("Viewport").Find("Content"));
                tempButton.transform.localPosition = new Vector3(2f, -buttonHeight * numFile - 2f, 0f);
                tempButton.transform.localScale = new Vector3(1f, uiRatio.y, 1f);
                tempButton.GetComponentInChildren<Text>().text = Path.GetFileNameWithoutExtension(fileInDir);
                tempButton.GetComponent<Button>().onClick.AddListener(() => { SelectAudioFile(fileInDir); });
                numFile++;
            }
        }

        // Resize the ViewPort to adjust it to the number of created buttons
        scrollView.Find("Viewport").Find("Content").GetComponent<RectTransform>().offsetMax = new Vector2(0f, 0f);
        scrollView.Find("Viewport").Find("Content").GetComponent<RectTransform>().offsetMin = new Vector2(0f, -buttonHeight * numFile - 4f);
        // Set the scrolling bar at the top of the viewport
        scrollView.GetComponent<ScrollRect>().verticalNormalizedPosition = 1;
    }

    // SelectAudioFile start the loading of an audio file
    public void SelectAudioFile(string filePath)
    {
        infoText.text = "Loading " + Path.GetFileNameWithoutExtension(filePath);
        StartCoroutine(LoadAudioFile(filePath));
    }

    // LoadAudiFile load a file and linked it to the audioSource
    IEnumerator LoadAudioFile(string filePath)
    {
        // Stop the audioSource to ensure it is not playing
        audioSource.Stop();

        // mp3 are not supported in Unity for Windows plateform (don't know why) so we need a specific download handler
        if (filePath.EndsWith(".mp3"))
        {
            using (UnityWebRequest uwr = UnityWebRequest.Get("file://" + filePath))
            {
                yield return uwr.SendWebRequest();
                if (uwr.isNetworkError)
                {
                    Debug.Log(uwr.error);
                    infoText.text = "Error occurs when loading file";
                    yield break;
                }
                else
                {
                    audioSource.clip = NAudioPlayer.FromMp3Data(uwr.downloadHandler.data); // NAudio create an audioClip from raw data of the DownloadHandler
                }
            }
        }
        else if (filePath.EndsWith(".ogg")) // for ogg, the download handler is audio
        {
            using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath,AudioType.OGGVORBIS))
            {
                yield return uwr.SendWebRequest();
                if (uwr.isNetworkError)
                {
                    Debug.Log(uwr.error);
                    infoText.text = "Error occurs when loading file";
                    yield break;
                }
                else
                {
                    audioSource.clip = DownloadHandlerAudioClip.GetContent(uwr);
                }
            }

        }
        else // for wav, the download handler is audio
        {
            using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.WAV))
            {
                yield return uwr.SendWebRequest();
                if (uwr.isNetworkError)
                {
                    Debug.Log(uwr.error);
                    infoText.text = "Error occurs when loading file";
                    yield break;
                }
                else
                {
                    audioSource.clip = DownloadHandlerAudioClip.GetContent(uwr);
                }
            }
        }

        // If the loading is successful display that it is in the infoText
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        if (fileName.Length > 30)
        {
            string newText = "";
            for(int i = 0; i < 27; i++)
            {
                newText += fileName[i];
            }
            fileName = newText + "...";
        }
        infoText.text = "File " + fileName + " loaded";

        // Change the setting of the audioSource to fit with the audio file selction (no loop and audioMixer snapshot that doesn't mute)
        audioSource.loop = false;
        audioSource.mute = false;
        snap.TransitionTo(0f);

        // Set the current option of displaying music
        MainCanvasController.liveRecording = false;
    }

    // public function to clear the infoText
    public void ClearInfoText()
    {
        infoText.text = "";
    }
}
