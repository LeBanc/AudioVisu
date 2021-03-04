using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PianoVisualization : MonoBehaviour
{
    public Light leftLight;
    public Light rightLight;
    public GameObject noteBlanche;
    public GameObject noteNoire;
    public GameObject band;
    public GameObject ball;
    [Range(0.01f,1f)]
    public float limitFromMax = 0.6f;
    [Range(0.01f, 100f)]
    public float limitBallCreation = 0.75f;

    GameObject[] piano = new GameObject[88];
    GameObject[] visu = new GameObject[88];
    float[] m_frequencyBands = new float[88];
    float maxValueTBF = 0f;
    float maxValueBF = 0f;
    float maxValueMF = 0f;
    float maxValueMHF = 0f;

    int numberOfChannels = 8192;
    float[] m_samplesLeft;
    float[] m_samplesRight;

    float bassLight = 0f;
    float treebleLight = 0f;

    int sendBalls = 0;

    float energy;
    Color[] noteColor = new Color[12];

    private AudioSource m_musicSource;

    // Start is called before the first frame update
    void Start()
    {
        // Initialization of the 3D piano model
        Vector3 position = Vector3.zero;
        for (int i = 0; i < piano.Length; i++)
        {
            if(i == 1 || i == 4 || i == 6 || i == 9 || i == 11 || i == 13 || i == 16 || i == 18 || i == 21 || i == 23 || i == 25 || i == 28 || i == 30 || i == 33 || i == 35 || i == 37 || i == 40 || i == 42 || i == 45 || i == 47 || i == 49 || i == 52 || i == 54 || i == 57 || i == 59 || i == 61 || i == 64 || i == 66 || i == 69 || i == 71 || i == 73 || i == 76 || i == 78 || i == 81 || i == 83 || i == 85)
            {
                GameObject _instance = Instantiate(noteNoire,position + new Vector3(-0.55f,0.5f,1f), Quaternion.identity);
                _instance.transform.parent = transform;
                piano[i] = _instance;
                GameObject _instanceBand = Instantiate(band, position + new Vector3(-0.55f, 1f, 2.5f), Quaternion.identity);
                _instanceBand.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                _instanceBand.transform.Rotate(new Vector3(80f, 0f, 0f));
                _instanceBand.transform.parent = transform;
                visu[i] = _instanceBand;
            }
            else
            {
                GameObject _instance = Instantiate(noteBlanche, position, Quaternion.identity);
                _instance.transform.parent = transform;
                piano[i] = _instance;
                GameObject _instanceBand = Instantiate(band, position + new Vector3(0f, 1f, 2.5f), Quaternion.identity);
                _instanceBand.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                _instanceBand.transform.Rotate(new Vector3(80f, 0f, 0f));
                _instanceBand.transform.parent = transform;
                visu[i] = _instanceBand;

                position += new Vector3(1.1f, 0f, 0f);
            }
        }

        // Initialization of the sound analysis
        m_musicSource = GetComponent<AudioSource>();
        m_samplesLeft = new float[numberOfChannels];
        m_samplesRight = new float[numberOfChannels];

        //Definition of color based on the note on an octave
        noteColor[0] = new Color(1f, 0f, 0f);
        noteColor[1] = new Color(1f, 0f, 0.5f);
        noteColor[2] = new Color(1f, 0f, 1f);
        noteColor[3] = new Color(0f, 0f, 1f);
        noteColor[4] = new Color(0f, 0.25f, 1f);
        noteColor[5] = new Color(0f, 0.5f, 1f);
        noteColor[6] = new Color(0f, 0.75f, 0.75f);
        noteColor[7] = new Color(0f, 1f, 0.5f);
        noteColor[8] = new Color(0f, 1f, 0f);
        noteColor[9] = new Color(0.5f, 1f, 0f);
        noteColor[10] = new Color(1f, 1f, 0f);
        noteColor[11] = new Color(1f, 0.5f, 0f);

    }

    void GetSpectrum()
    {
        m_musicSource.GetSpectrumData(m_samplesLeft, 0, FFTWindow.Blackman);
        m_musicSource.GetSpectrumData(m_samplesRight, 1, FFTWindow.Blackman);
    }

    void MakeFrequencyBand()
    {
        energy = 0f;
        float stereoCoeff = (2 - Mathf.Abs(m_musicSource.panStereo)); // stereoCoeff is used to normalize the amplitude whatever stereo pan is chosen

        bassLight = 0f;
        for(int i=0; i < 9; i++){
            bassLight += (m_samplesLeft[i] + m_samplesRight[i]) / stereoCoeff;
        }
        bassLight = bassLight / 9;
        energy += bassLight;

        maxValueTBF = 0.0075f;
        maxValueBF = 0.0075f;
        maxValueMF = 0.0075f;
        maxValueMHF = 0.0075f;

        m_frequencyBands[0] = (m_samplesLeft[9] + m_samplesRight[9]) / stereoCoeff;
        maxValueTBF = Mathf.Max(maxValueTBF, m_frequencyBands[0]);
        m_frequencyBands[1] = (m_samplesLeft[9] + m_samplesRight[9]) / stereoCoeff;
        maxValueTBF = Mathf.Max(maxValueTBF, m_frequencyBands[1]);
        m_frequencyBands[2] = (m_samplesLeft[10] + m_samplesRight[10]) / stereoCoeff;
        maxValueTBF = Mathf.Max(maxValueTBF, m_frequencyBands[2]);
        m_frequencyBands[3] = (m_samplesLeft[11] + m_samplesRight[11]) / stereoCoeff;
        maxValueTBF = Mathf.Max(maxValueTBF, m_frequencyBands[3]);
        m_frequencyBands[4] = (m_samplesLeft[11] + m_samplesRight[11]) / stereoCoeff;
        maxValueTBF = Mathf.Max(maxValueTBF, m_frequencyBands[4]);
        m_frequencyBands[5] = (m_samplesLeft[12] + m_samplesRight[12]) / stereoCoeff;
        maxValueTBF = Mathf.Max(maxValueTBF, m_frequencyBands[5]);
        m_frequencyBands[6] = (m_samplesLeft[13] + m_samplesRight[13]) / stereoCoeff;
        maxValueTBF = Mathf.Max(maxValueTBF, m_frequencyBands[6]);
        m_frequencyBands[7] = (m_samplesLeft[14] + m_samplesRight[14]) / stereoCoeff;
        maxValueTBF = Mathf.Max(maxValueTBF, m_frequencyBands[7]);
        m_frequencyBands[8] = (m_samplesLeft[14] + m_samplesRight[14]) / stereoCoeff;
        maxValueTBF = Mathf.Max(maxValueTBF, m_frequencyBands[8]);

        energy += (m_samplesLeft[9] + m_samplesRight[9]) / stereoCoeff;
        energy += (m_samplesLeft[10] + m_samplesRight[10]) / stereoCoeff;
        energy += (m_samplesLeft[11] + m_samplesRight[11]) / stereoCoeff;
        energy += (m_samplesLeft[12] + m_samplesRight[12]) / stereoCoeff;
        energy += (m_samplesLeft[13] + m_samplesRight[13]) / stereoCoeff;
        energy += (m_samplesLeft[14] + m_samplesRight[14]) / stereoCoeff;

        int n = 9;
        float average = 0;
        int count = 0;

        for (int i = 15; i < 1468; i++) // pas besoin des échantillons après 1467 (environ 4300Hz)
        {
            switch (i)
            {
                case 16:
                case 17:
                case 18:
                case 19:
                case 21:
                case 22:
                case 23:
                case 25:
                case 26:
                case 28:
                case 29:
                case 31:
                case 33:
                case 35:
                case 37:
                case 39:
                case 41:
                case 44:
                case 46:
                case 49:
                case 52:
                case 55:
                case 58:
                case 62:
                case 65:
                case 69:
                case 73:
                case 78:
                case 82:
                case 87:
                case 92:
                case 98:
                case 104:
                case 110:
                case 116:
                case 123:
                case 130:
                case 138:
                case 146:
                case 155:
                case 164:
                case 174:
                case 184:
                case 195:
                case 207:
                case 219:
                case 232:
                case 246:
                case 260:
                case 276:
                case 292:
                case 310:
                case 328:
                case 348:
                case 368:
                case 390:
                case 413:
                case 438:
                case 464:
                case 491:
                case 521:
                case 552:
                case 584:
                case 619:
                case 656:
                case 695:
                case 736:
                case 779:
                case 826:
                case 875:
                case 927:
                case 982:
                case 1041:
                case 1103:
                case 1168:
                case 1237:
                case 1311:
                case 1389:
                case 1467:
                    m_frequencyBands[n] = average / count;
                    energy += m_frequencyBands[n];
                    if (n < 12)
                    {
                        maxValueTBF = Mathf.Max(maxValueTBF, m_frequencyBands[n]);
                    }else if (n < 36)
                    {
                        maxValueBF = Mathf.Max(maxValueBF, m_frequencyBands[n]);
                    } else if (n < 72)
                    {
                        maxValueMF = Mathf.Max(maxValueMF, m_frequencyBands[n]);
                    }
                    else
                    {
                        maxValueMHF = Mathf.Max(maxValueMHF, m_frequencyBands[n]);
                    }                    
                    n++;
                    average = 0;
                    count = 0;
                    break;
                default:
                    break;
            }

            average += (m_samplesLeft[i] + m_samplesRight[i]) / stereoCoeff;
            count++;
        }

        treebleLight = 0f;
        for (int i = 1468; i < numberOfChannels; i++)
        {
            treebleLight += (m_samplesLeft[i] + m_samplesRight[i]) / stereoCoeff;
        }
        treebleLight = treebleLight / (numberOfChannels - 1468);
        energy += treebleLight;
    }

    // Update is called once per frame
    void Update()
    {
        GetSpectrum();
        MakeFrequencyBand();

        leftLight.intensity = 0.2f + 500 * bassLight;
        rightLight.intensity = 0.2f + 2000 * treebleLight;

        float max = Mathf.Max(maxValueTBF, maxValueBF, maxValueMF, maxValueMHF);
        float maxValue = 0;
        for (int i = 0; i< piano.Length; i++)
        {
            float _emission = 0f;
            _emission = piano[i].GetComponent<MeshRenderer>().materials[0].GetColor("_EmissionColor").b;
            _emission -= 0.05f;
            _emission = Mathf.Clamp(_emission,0f, 1f);

            if (i < 12)
            {
                maxValue = maxValueTBF;
            }
            else if (i < 36)
            {
                maxValue = maxValueBF;
            }
            else if (i < 72)
            {
                maxValue = maxValueMF;
            }
            else
            {
                maxValue = maxValueMHF;
            }


            if (m_frequencyBands[i] *10 > limitFromMax * maxValue)
            {
                _emission = Mathf.Pow(m_frequencyBands[i],2) / Mathf.Pow(Mathf.Max(maxValue, float.Epsilon),2);
                //_emission = m_frequencyBands[i] / maxValue;
            }
            piano[i].GetComponent<MeshRenderer>().materials[0].SetColor("_EmissionColor", new Color(_emission, _emission, _emission));

            visu[i].transform.localScale = new Vector3(0.5f , 0.1f + 2 * m_frequencyBands[i]/max , 0.5f);
            visu[i].transform.localPosition = new Vector3(visu[i].transform.localPosition.x, (m_frequencyBands[i] / max)/4, visu[i].transform.localPosition.z);
            visu[i].GetComponent<MeshRenderer>().materials[0].SetColor("_Color", noteColor[i % noteColor.Length]);

        }

    }

    private void FixedUpdate()
    {
        sendBalls += 1;
        if (sendBalls == 1)
        {

            float max = Mathf.Max(maxValueTBF, maxValueBF, maxValueMF, maxValueMHF);

            for (int i = 0; i < piano.Length; i++)
            {
                if(m_frequencyBands[i] > limitBallCreation * energy / (piano.Length+2))
                {
                    GameObject _instance = Instantiate(ball);
                    _instance.transform.position = transform.position + new Vector3(0f, 0.5f, 4f);
                    _instance.transform.parent = transform;
                    _instance.name = "Ball" + (i + 1);
                    _instance.transform.Translate(new Vector3(visu[i].transform.position.x, 0f, 0f));
                    _instance.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                    _instance.GetComponent<Rigidbody>().velocity = new Vector3(0f, 5f, 10f);
                    float _emission = 0.1f + (m_frequencyBands[i] / max);
                    _instance.GetComponent<MeshRenderer>().materials[0].SetColor("_EmissionColor", new Color(_emission, _emission, _emission));
                    _instance.gameObject.tag = "Rain";
                    Color color = noteColor[i % noteColor.Length];
                    color.a = 0.29f;
                    _instance.GetComponent<MeshRenderer>().materials[0].SetColor("_Color", color);
                }
            }
        }
        if (sendBalls == 3) sendBalls = 0;
    }
}
