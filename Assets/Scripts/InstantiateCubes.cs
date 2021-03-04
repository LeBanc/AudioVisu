using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateCubes : MonoBehaviour
{
    public GameObject cubePrefab;
    public GameObject bufferPrefab;
    public GameObject ghostPrefab;
    public float maxScale;

    public bool displayCircle = true;
    public bool displayBands = true;
    public bool displayBuffers = true;
    public bool displayGhosts = true;
    public bool displayRain = true;

    private GameObject[] cubesArray;
    private GameObject[] bandsArray;
    private GameObject[] bandsBufferArray;
    private GameObject[] ghostsArray;
    private float size;
    private int bands;
    private int enableRain = 0;

    public void SetPrefabs(int samplesNumber, int bandsNumber)
    {
        bands = bandsNumber;

        // full samples circle
        cubesArray = new GameObject[samplesNumber];

        for (int i = 0; i < samplesNumber; i++)
        {
            GameObject _instance = Instantiate(cubePrefab);
            _instance.transform.position = transform.position;
            _instance.transform.parent = transform;
            _instance.name = "Cube" + i;
            transform.eulerAngles = new Vector3(0, i * -360 / samplesNumber, 0);
            _instance.transform.position = Vector3.forward * 100;
            cubesArray[i] = _instance;
        }

        // selected bands
        transform.rotation = Quaternion.identity;
        bandsArray = new GameObject[bandsNumber];
        bandsBufferArray = new GameObject[bandsNumber];
        size = (1050 - (5* (bandsNumber -1))) / (bandsNumber - 1); // (distanceMax - x * espace entre les barres) / x ou x est le nombre de barres -1

        for (int i = 0; i < bandsNumber; i++)
        {
            GameObject _instance = Instantiate(cubePrefab);
            _instance.transform.position = transform.position + new Vector3(-525,0f,0f);
            _instance.transform.parent = transform;
            _instance.name = "Band" + (i+1);
            _instance.transform.Translate(new Vector3( i*1050/(bandsNumber-1), 0f, 0f));
            _instance.AddComponent<Boundaries>();
            bandsArray[i] = _instance;
            }

        for (int i = 0; i < bandsNumber; i++)
        {
            GameObject _instance = Instantiate(bufferPrefab);
            _instance.transform.position = transform.position + new Vector3(-525, 0f, 0f);
            _instance.transform.parent = transform;
            _instance.name = "BandBuffer" + (i + 1);
            _instance.transform.Translate(new Vector3(i * 1050 / (bandsNumber - 1), 0f, 0f));
            _instance.transform.localScale = new Vector3(size, 10, 10);
            _instance.AddComponent<Boundaries>();
            bandsBufferArray[i] = _instance;
        }

        ghostsArray = new GameObject[bandsNumber];
        for (int i = 0; i < bandsNumber; i++)
        {
            GameObject _instance = Instantiate(ghostPrefab);
            _instance.transform.position = transform.position + new Vector3(-525, 0f, 0f);
            _instance.transform.parent = transform;
            _instance.name = "Ghost" + (i + 1);
            _instance.transform.Translate(new Vector3(i * 1050 / (bandsNumber - 1), 0f, 0f));
            _instance.transform.localScale = new Vector3(size, 10, 10);
            _instance.AddComponent<Boundaries>();
            ghostsArray[i] = _instance;
        }

    }


    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < cubesArray.Length; i++)
        {
            cubesArray[i].SetActive(displayCircle);
            cubesArray[i].transform.localScale = new Vector3(1, (AudioVisualization.m_samplesLeft[i] + AudioVisualization.m_samplesRight[i])/2 * maxScale + 2, 1);
        }

        for (int i = 0; i < bandsArray.Length; i++)
        {
            bandsArray[i].SetActive(displayBands);
            bandsArray[i].transform.localScale = new Vector3(size, AudioVisualization.m_frequencyBands[i] * maxScale/4 + 2, 10);
            bandsArray[i].transform.localPosition = new Vector3(bandsArray[i].transform.localPosition.x, AudioVisualization.m_frequencyBands[i] * maxScale / 8 + 1, bandsArray[i].transform.localPosition.z);
        }

        for (int i = 0; i < bandsBufferArray.Length; i++)
        {
            bandsBufferArray[i].SetActive(displayBuffers);
            bandsBufferArray[i].transform.localPosition = new Vector3(bandsBufferArray[i].transform.localPosition.x, AudioVisualization.m_bandBuffer[i] * maxScale / 4 + 8, bandsBufferArray[i].transform.localPosition.z);
        }

        for (int i = 0; i < ghostsArray.Length; i++)
        {
            ghostsArray[i].SetActive(displayGhosts);
            ghostsArray[i].transform.localPosition = new Vector3(ghostsArray[i].transform.localPosition.x, (AudioVisualization.m_maxBandValue[i] / AudioVisualization.m_coeff[i]) * maxScale / 4 + 8, ghostsArray[i].transform.localPosition.z);
            float _emission = AudioVisualization.m_bandBuffer[i] / Mathf.Max(1f, AudioVisualization.m_maxBandValue[i]);
            ghostsArray[i].GetComponent<MeshRenderer>().materials[0].SetColor("_EmissionColor", new Color(_emission, _emission, _emission));
        }
    }

    private void FixedUpdate()
    {
        enableRain += 1;
        if (displayRain && enableRain == 1)
        {
            for (int i = 0; i < bands; i++)
            {
                GameObject _instance = Instantiate(bufferPrefab);
                _instance.transform.position = transform.position + new Vector3(-525, 775f, 0f);
                _instance.transform.parent = transform;
                _instance.name = "Rain" + (i + 1);
                _instance.transform.Translate(new Vector3(i * 1050 / (bands - 1), 0f, 0f));
                _instance.transform.localScale = new Vector3(size, 10, 10);
                //_instance.transform.localPosition = new Vector3(_instance.transform.localPosition.x, _instance.transform.localPosition.y, AudioVisualization.m_bandBuffer[i] * maxScale / 4 + 8);
                _instance.AddComponent<Rigidbody>();
                _instance.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                _instance.GetComponent<Rigidbody>().useGravity = false;
                _instance.GetComponent<Rigidbody>().velocity = new Vector3(0f, -100f, 0f);
                _instance.GetComponent<Rigidbody>().constraints = (RigidbodyConstraints)122;
                float _emission = 0.1f + (AudioVisualization.m_frequencyBands[i] / Mathf.Max(AudioVisualization.m_maxBandValue[i], AudioVisualization.m_frequencyBands[i]));
                _instance.GetComponent<MeshRenderer>().materials[0].SetColor("_EmissionColor", new Color(_emission, _emission, _emission));
                _instance.gameObject.tag = "Rain";
            }
        }
        if (enableRain == 10) enableRain = 0;
    }
}
