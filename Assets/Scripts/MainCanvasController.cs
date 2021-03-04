using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainCanvasController : MonoBehaviour
{
    public Sprite slideUp;
    public Sprite slideDown;

    AudioSource audioSource;
    bool displayPanel = true;

    GameObject controlPanel;
    GameObject slideButton;
    GameObject selectionPanel;
    Vector3 controlPanelPosition;
    Vector3 selectionPanelPosition;
    Vector3 selectionPanelPositionInit;
    GameObject fileTab;
    GameObject inputTab;
    bool fileTabSelected = true;
    Coroutine controlPanelSlide;
    Coroutine selectionPanelSlide;

    public static bool liveRecording = false;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = FindObjectOfType<AudioSource>();
        controlPanel = transform.Find("ControlPanel").gameObject;
        slideButton = controlPanel.transform.Find("SlideButton").gameObject;
        controlPanelPosition = controlPanel.transform.localPosition;

        selectionPanel = transform.Find("SelectionPanel").gameObject;
        selectionPanelPosition = selectionPanel.transform.localPosition;
        selectionPanelPositionInit = selectionPanel.transform.localPosition; ;

        fileTab = selectionPanel.transform.Find("FileTab").gameObject;
        inputTab = selectionPanel.transform.Find("InputTab").gameObject;
    }

    public void ActionOnControlPanel()
    {
        if (displayPanel)
        {
            RetractControlPanel();
        }
        else
        {
            DisplayControlPanel();
        }
    }

    void DisplayControlPanel()
    {
        controlPanelPosition = controlPanelPosition + new Vector3(0f, -50f, 0f);
        if (controlPanelSlide != null) StopCoroutine(controlPanelSlide);
        controlPanelSlide = StartCoroutine(ControlPanelSlide());
        slideButton.GetComponent<Image>().sprite = slideUp;
        displayPanel = true;
    }

    void RetractControlPanel()
    {
        controlPanelPosition = controlPanelPosition + new Vector3(0f, 50f, 0f);
        if (controlPanelSlide != null) StopCoroutine(controlPanelSlide);
        controlPanelSlide = StartCoroutine(ControlPanelSlide());
        slideButton.GetComponent<Image>().sprite = slideDown;
        displayPanel = false;
    }

    IEnumerator ControlPanelSlide()
    {
        while (Mathf.Abs(controlPanel.transform.localPosition.y - controlPanelPosition.y) > 0.01)
        {
            yield return controlPanel.transform.localPosition = Vector3.Lerp(controlPanel.transform.localPosition, controlPanelPosition, 0.2f);
        }
        controlPanel.transform.localPosition = controlPanelPosition;
    }

    IEnumerator SelectionPanelSlide()
    {
        while (Mathf.Abs(selectionPanel.transform.localPosition.x - selectionPanelPosition.x) > 0.01)
        {
            yield return selectionPanel.transform.localPosition = Vector3.Lerp(selectionPanel.transform.localPosition, selectionPanelPosition, 0.2f);
        }
        selectionPanel.transform.localPosition = selectionPanelPosition;
    }

    public void PlayMusic()
    {
        if (!liveRecording) // Case for audio files
        {
            audioSource.Play();
            selectionPanelPosition = selectionPanelPositionInit + new Vector3(350f, 0f, 0f);
            if (selectionPanelSlide != null) StopCoroutine(selectionPanelSlide);
            selectionPanelSlide = StartCoroutine(SelectionPanelSlide());
            selectionPanel.GetComponentInChildren<SelectionPanelController>().ClearInfoText();
        }
        else // Case for microphone input
        {
            audioSource.mute = false;
            selectionPanelPosition = selectionPanelPositionInit + new Vector3(350f, 0f, 0f);
            if (selectionPanelSlide != null) StopCoroutine(selectionPanelSlide);
            selectionPanelSlide = StartCoroutine(SelectionPanelSlide());
        }
    }

    public void StopMusic()
    {
        if (!liveRecording) // Case for audio files
        {
            audioSource.Stop();
        }
        else // Case for microphone input
        {
            audioSource.mute = true;
        }
        selectionPanelPosition = selectionPanelPositionInit;
        if(selectionPanelSlide != null) StopCoroutine(selectionPanelSlide);
        selectionPanelSlide = StartCoroutine(SelectionPanelSlide());

    }

    public void PauseMusic()
    {
        if (!liveRecording) // Case for audio files
        {
            if (audioSource.isPlaying)
            {
                audioSource.Pause();
            }
            else
            {
                audioSource.UnPause();
            }
        }
        else // Case for microphone input
        {
            if (audioSource.mute)
            {
                audioSource.mute = false;
            }
            else
            {
                audioSource.mute = true;
            }
        }
            
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void FileTabAction()
    {
        if (!fileTabSelected)
        {
            StartCoroutine(SwitchPanels("AudioInputSelectionPanel", "FileSelectionPanel"));
            inputTab.GetComponent<Image>().color = new Color(83f / 255, 86f / 255, 91f / 255, 128f / 255);
            fileTab.GetComponent<Image>().color = new Color(83f / 255, 86f / 255, 91f / 255, 1f);
            fileTabSelected = true;

            // Clear the reading of microphone for memory purpose
            foreach (var dev in Microphone.devices)
            {
                Microphone.End(dev);
            }
        }
    }

    public void AudioInputTabAction()
    {
        if (fileTabSelected)
        {
            StartCoroutine(SwitchPanels("FileSelectionPanel", "AudioInputSelectionPanel"));
            fileTab.GetComponent<Image>().color = new Color(83f / 255, 86f / 255, 91f / 255, 128f / 255);
            inputTab.GetComponent<Image>().color = new Color(83f / 255, 86f / 255, 91f / 255, 1f);
            fileTabSelected = false;
        }
    }

    IEnumerator SwitchPanels(string from, string to)
    {
        Transform firstPanel = selectionPanel.transform.Find(from).transform;
        while(firstPanel.localPosition.x < 299.99)
        {
            yield return firstPanel.localPosition = Vector3.Lerp(firstPanel.localPosition, new Vector3(300f, 0f, 0f), 0.3f);
        }
        firstPanel.localPosition = new Vector3(300f, 0f, 0f);

        Transform secondPanel = selectionPanel.transform.Find(to).transform;
        while (secondPanel.localPosition.x > 0.01)
        {
            yield return secondPanel.localPosition = Vector3.Lerp(secondPanel.localPosition, new Vector3(0f, 0f, 0f), 0.3f);
        }
        secondPanel.localPosition = new Vector3(0f, 0f, 0f);
    }
}
