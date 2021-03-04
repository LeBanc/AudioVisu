using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]

public class AudioVisualization : MonoBehaviour
{
    public  int  numberOfChannels = 512;
    
    public enum ChannelsNumber { _4, _8, _10, _64 }
    public ChannelsNumber bands;
    public float[] coeffs4bands = new[] { 1.45f, 1.5f, 1.8f, 2.4f};
    public float[] coeffs8bands = new[] { 0.8f, 1.05f, 0.8f, 1f, 1f, 1.05f, 0.75f, 1.95f};
    public float[] coeffs10bands = new[] { 0.45f, 0.4f, 0.75f, 0.85f, 1.05f , 0.75f, 1.1f, 1.15f, 1.5f, 0.85f};
    public float[] coeffs64bands = new[] { 0.42f, 0.39f, 0.43f, 0.37f, 0.55f, 0.24f, 0.22f, 0.2f, 0.19f, 0.2f, 0.21f, 0.2f, 0.21f, 0.19f, 0.2f, 0.17f, 0.17f, 0.34f, 0.24f, 0.26f, 0.26f, 0.25f, 0.22f, 0.22f, 0.23f, 0.25f, 0.1f, 0.27f, 0.1f, 0.18f, 0.17f, 0.14f, 0.23f, 0.24f, 0.18f, 0.16f, 0.15f, 0.14f, 0.13f, 0.17f, 0.21f, 0.18f, 0.18f, 0.15f, 0.14f, 0.13f, 0.13f, 0.19f, 0.3f, 0.3f, 0.29f, 0.28f, 0.24f, 0.19f, 0.27f, 0.28f, 0.17f, 0.27f, 0.16f, 0.14f, 0.12f, 0.05f, 0.055f, 0.04f };

    public static float[] m_samplesLeft; // Multiple de 64
    public static float[] m_samplesRight; // Multiple de 64

    public static float[] m_frequencyBands; // Puissance de 2
    public static float[] m_bandBuffer;
    private float[] m_bufferDecrease;

    public float initialDecrease = 0.005f;
    public float decreaseRatio = 1.1f;
    
    

    private AudioSource m_musicSource;

    private int numberOfBands;
    private float[] bandsWidth;
    public static float[] m_maxBandValue;
    public static float[] m_coeff;
    private bool first;

    // Start is called before the first frame update
    void Start()
    {

        first = true;
        m_musicSource = GetComponent<AudioSource>();
        m_samplesLeft =  new float[numberOfChannels];
        m_samplesRight = new float[numberOfChannels];

        switch (bands)
        {
            case ChannelsNumber._4:
                numberOfBands = 4;
                bandsWidth = new[] { 250f, 1000f, 4000f, 23000f };
                m_coeff = coeffs4bands;
                break;
            case ChannelsNumber._8:
                numberOfBands = 8;
                bandsWidth = new[] { 60f, 250f, 500f, 1000f, 2000f, 4000f, 6000f, 23000f };
                m_coeff = coeffs8bands;
                break;
            case ChannelsNumber._10:
                numberOfBands = 10;
                bandsWidth = new[] { 40f, 80f, 160f, 315f, 750f, 1200f, 2500f, 5000f, 10000f, 23000f };
                m_coeff = coeffs10bands;
                break;
            case ChannelsNumber._64:
                numberOfBands = 64;
                bandsWidth = new[] { 40f, 80f, 120f, 170f, 215f, 250f, 301f, 344f, 380f, 420f, 470f, 516f, 550f, 600f, 640f, 680f, 770f, 850f, 940f, 1030f, 1110f, 1200f, 1280f, 1370f, 1460f, 1548f, 1630f, 1720f, 1800f, 1890f, 1970f, 2060f, 2230f, 2400f, 2570f, 2750f, 2920f, 3090f, 3260f, 3430f, 3690f, 3950f, 4210f, 4470f, 4720f, 4980f, 5240f, 5500f, 6190f, 6870f, 7560f, 8250f, 8940f, 9360f, 10310f, 11000f, 11690f, 13070f, 14440f, 15820f, 17190f, 18570f, 19952f, 23000 };
                m_coeff = coeffs64bands;
                break;
            default:
                numberOfBands = 8;
                break;
        }

        m_frequencyBands = new float[numberOfBands];
        m_bandBuffer = new float[numberOfBands];
        m_bufferDecrease = new float[numberOfBands];
        m_maxBandValue = new float[numberOfBands];
        GetComponent<InstantiateCubes>().SetPrefabs(numberOfChannels, numberOfBands);

        for(int i = 0; i < m_maxBandValue.Length; i++)
        {
            m_maxBandValue[i] = 0.3f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        GetSpectrum();
        MakeFrequencyBands();
        BandBuffer();

        if (!m_musicSource.isPlaying && first)
        {
            first = false;
            for(int i = 0; i< m_maxBandValue.Length; i++)
            {
                Debug.Log("Max value of band " + i + " is: " + m_maxBandValue[i]);
            }
        }

    }

    void GetSpectrum()
    {
        m_musicSource.GetSpectrumData(m_samplesLeft, 0, FFTWindow.Blackman);
        m_musicSource.GetSpectrumData(m_samplesRight, 1, FFTWindow.Blackman);
    }

    void BandBuffer()
    {
        for(int i = 0; i < m_bandBuffer.Length; i++)
        {
            if (m_frequencyBands[i] >= m_bandBuffer[i])
            {
                m_bandBuffer[i] = m_frequencyBands[i];
                m_bufferDecrease[i] = initialDecrease;
            }
            else
            {
                m_bandBuffer[i] -= m_bufferDecrease[i];
                m_bufferDecrease[i] *= decreaseRatio;
            }
        }
    }

    void MakeFrequencyBands()
    {
        /*int maxCount = 0;
        for(int i = 0; i< numberOfBands; i++)
        {
            float average = 0;
            int count = 0;
            // NOT OK mais soirée KARAOKE!!!
            int max = (InversePowerOf2(numberOfBands) + 1) * (numberOfChannels / numberOfBands) / ((int)Mathf.Pow(2, (numberOfBands - 1 - i)));
            Debug.Log("max :" + max + " = " + (InversePowerOf2(numberOfBands) + 1) + " * " + (numberOfChannels / numberOfBands) + " / " + ((int)Mathf.Pow(2, (numberOfBands - 1 - i))));
            for (int j = 0; j < max; j++)
            {
                average += m_samples[j + maxCount];
                count++;
               // Debug.Log(j+maxCount);
            }
            maxCount += max;
            if (i == numberOfBands - 1)
            {
                for(int j = maxCount; j < numberOfChannels;j++)
                {
                    average += m_samples[j];
                    count++;
                 //   Debug.Log(j);
                }
            }
            m_frequencyBands[i] = average / count;
        }*/

        int n = 0;
        float average = 0;
        int count = 0;
        float stereoCoeff = (2 - Mathf.Abs(m_musicSource.panStereo)); // stereoCoeff is used to normalize the amplitude whatever stereo pan is chosen

        for (int i = 0; i < numberOfChannels; i++)
        {
            if (22050 / numberOfChannels * i > bandsWidth[n])
            {
                m_frequencyBands[n] = average / m_coeff[n];
                m_maxBandValue[n] = Mathf.Max(m_maxBandValue[n], average);
                n++;
                average = 0;
                count = 0;
            }

            average += (m_samplesLeft[i] + m_samplesRight[i]) / stereoCoeff;
            count++;
        }
        
        m_frequencyBands[n] = average / m_coeff[n];
        m_maxBandValue[n] = Mathf.Max(m_maxBandValue[n], average);
    }

    int InversePowerOf2(int val)
    {
        int count = 0;
        if (Mathf.IsPowerOfTwo(val))
        {
            int tempVal = val;
            while (tempVal != 1)
            {
                tempVal /= 2;
                count++;
            }
        }
        return count;
    }

}
