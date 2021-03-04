using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;


// AudioInputPanelControler is used to select the microphone input we want to use
public class AudioInputPanelControler : MonoBehaviour
{
    public GameObject button; // default button to display the selectable microphone sources
    public AudioMixerSnapshot snap; // audiomixer snapshot with attenuation of -80dB to mute the microphone in the application (else it will be output twice)

    Transform scrollView; // scrollView container where the data are displayed
    Text infoText; // infoText at the bottom of the scrollView to display some data
    AudioSource audioSource;

    int nbLines; // number of lines we want to display in the scrollView (deos not really work in the end)
    float buttonHeight; // height of the button gameObject
    Vector2 uiRatio; // to get the screen ratio from the default config

    // Start is called before the first frame update
    void Start()
    {
        // Get the scrollView, the infoText and the audioSource
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

        // infoText init
        infoText.text = "";

        // Display available microphone inputs
        DisplayAvailableAudioInputs();
    }

    // Display the microphone inputs in the scrollView
    public void DisplayAvailableAudioInputs()
    {
        int numFile = 0;
        // Get list of Microphone devices and create a scaled button for each
        foreach (var device in Microphone.devices)
        {
            // Create button
            GameObject tempButton = Instantiate(button);
            tempButton.transform.SetParent(scrollView.Find("Viewport").Find("Content"));
            // Move and scale it
            tempButton.transform.localPosition = new Vector3(2f, -buttonHeight * numFile - 2f, 0f);
            tempButton.transform.localScale = new Vector3(1f, uiRatio.y, 1f);
            // Set the name of the button
            tempButton.GetComponentInChildren<Text>().text = device;
            // Add a OnClick function to be able to select this input
            tempButton.GetComponent<Button>().onClick.AddListener(() => { SelectDevice(device); });
            numFile++;
        }
    }

    // Select the chosen input and link it to the audioSource
    public void SelectDevice(string device)
    {
        // to avoid error if another microphone device is already recording
        foreach (var dev in Microphone.devices)
        {
            if (Microphone.IsRecording(dev))
            {
                Microphone.End(dev);
            }
            
        }

        StartCoroutine(RecordDevice(device));
        infoText.text = "Getting " + device + " data";

    }

    IEnumerator RecordDevice(string device)
    {
        // Stop the audioSource to ensure it is not playing
        audioSource.Stop();

        // Link the audioSource to the selected microphone
        audioSource.clip = Microphone.Start(device, true, 10, AudioSettings.outputSampleRate);


        //float delay = 0f; // for debug

        //wait until microphone position is found (?)
        while (!(Microphone.GetPosition(device) > 0))
        {
            // Debug.Log(delay + " : " + Microphone.GetPosition(device)); // for debug
            // delay += Time.deltaTime;
            yield return null;
        }

        // Set the right settings of the audioSource
        audioSource.loop = true; // to always play what comes from the microphone, even after the 10 seconds lenght of the audioClip created above
        audioSource.Play(); // Play the audioSource right now to avoid any delay between recording and playing data
        audioSource.mute = true; // Mute the audioSource to avoid any analysis of the audio visualization
        snap.TransitionTo(0f); // Select the audioMixer snapshot the "mute" the microphone
        // Display data to the user
        infoText.text = "Listening to " + device;

        // Set the current option of displaying music
        MainCanvasController.liveRecording = true;
    }

    // Clear the infoText
    public void ClearInfoText()
    {
        infoText.text = "";
    }

    // Clear the memory when quitting
    private void OnDestroy()
    {
        // end the reading of microphone for memory
        foreach (var dev in Microphone.devices)
        {
            Microphone.End(dev);
        }
    }

}
