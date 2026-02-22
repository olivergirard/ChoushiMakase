using FftSharp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using UtauPlugin;

namespace ChoushiMakase
{
    internal class Program
    {
        /* Used for reading in frequency data from audio samples. A smaller buffer uses and outputs more samples. */
        static int bufferSize = 2048;

        /* Determines how much of the note has control points attached to it. The default value is 0.5, meaning half of the note. */
        static double amountToBend = 0.5;

        /* Determines how much the frequency data in Hz is scaled by. The greater the value, the less dramatic the pitchbends. */
        static double scaleFactor = 100.0;

        /* Determines how much the frequency data in Hz is shifted downwards by. Strongly impacted by changes in scaleFactor. */
        static double shiftFactor = 30.0;

        static void Main(string[] args)
        {
            UtauPlugin.UtauPlugin utauPlugin = new UtauPlugin.UtauPlugin(args[0]);
            utauPlugin.Input();

            Console.WriteLine("Please provide the path where your audio file (.wav or .mp3) is located.");
            string audioFilePath = Console.ReadLine();
            audioFilePath = audioFilePath.Replace("\"", string.Empty);

            /* Using NAudio and FftSharp to read and parse audio data. */
            double[] sampleFrequencies = GetPitchData(audioFilePath);
            sampleFrequencies = NormalizeFrequencies(sampleFrequencies);

            /* Using UtauPlugin to convert audio data to pitchbends. */
            CreatePitchbends(sampleFrequencies, audioFilePath, utauPlugin);

            utauPlugin.Output();
        }

        /* Divides and decrements the frequency values for each sample so that they are compatible with UTAU. */
        static double[] NormalizeFrequencies(double[] sampleFrequencies)
        {
            for (int i = 0; i < sampleFrequencies.Length; i++)
            {

                sampleFrequencies[i] /= scaleFactor;
                sampleFrequencies[i] -= shiftFactor;
            }

            return sampleFrequencies;
        }

        /* Determines the more prevalent frequency in a sample. */
        static double[] GetPitchData(string audioFilePath)
        {
            var audioFile = new AudioFileReader(audioFilePath);
            int sampleRate = audioFile.WaveFormat.SampleRate;

            var amplitudeValues = new float[bufferSize];
            var window = new FftSharp.Windows.Hanning();

            List<double> peakFrequencies = new List<double>();

            int samplesRead = audioFile.Read(amplitudeValues, 0, bufferSize);
            while (samplesRead > 0)
            {

                double[] input = new double[bufferSize];
                for (int i = 0; i < bufferSize; i++)
                {
                    if (i < samplesRead)
                    {
                        input[i] = amplitudeValues[i];
                    }
                    else
                    {
                        input[i] = 0;
                    }
                }

                /* Tapering the edges of each sample to reduce spectral leakage. */
                window.ApplyInPlace(input);

                /* Determining the most prevalent frequency amongst all the noise. */
                System.Numerics.Complex[] spectrum = input.Select(v => new System.Numerics.Complex(v, 0)).ToArray();
                FFT.Forward(spectrum);
                double[] magnitude = FFT.Magnitude(spectrum);

                int maxIndex = 0;
                double maxValue = double.MinValue;
                for (int i = 0; i < magnitude.Length; i++)
                {
                    if (magnitude[i] > maxValue)
                    {
                        maxValue = magnitude[i];
                        maxIndex = i;
                    }
                }

                double peakFrequency = (double)maxIndex * sampleRate / bufferSize;
                peakFrequencies.Add(peakFrequency);

                samplesRead = audioFile.Read(amplitudeValues, 0, bufferSize);
            }

            double[] result = peakFrequencies.ToArray();

            return result;
        }

        /* Makes a control point for each most prevalent frequency in each sample. */
        static void CreatePitchbends(double[] sampleFrequencies, string audioFilePath, UtauPlugin.UtauPlugin utauPlugin)
        {
            var audioFile = new AudioFileReader(audioFilePath);
            int sampleRate = audioFile.WaveFormat.SampleRate;
            int pitchbendIndex = 0;

            int ustLength = 0;
            foreach (Note note in utauPlugin.note)
            {
                ustLength += note.GetLength();
            }

            foreach (Note note in utauPlugin.note)
            {
                /* Calculating the number of samples that span each individual note. */
                int samplesForNote = (note.GetLength() * sampleFrequencies.Length) / ustLength;

                /* Determining how much each sample is spaced across the note, based on UTAU's default BPM of 120. */
                double tempoFactor = 120 / utauPlugin.Tempo;
                double spacing = (note.GetLength() / samplesForNote) * tempoFactor;

                string pbw = spacing.ToString();
                string pby = "";


                for (int i = 1; i < samplesForNote - 1; i++)
                {
                    if (i < (samplesForNote - 1) * amountToBend)
                    {
                        /* Adjusting the horizontal spacing value per control point. At a uniform interval for each note. */
                        pbw += "," + spacing.ToString();

                        /* Adjusting the vertical pitch value per control point. */
                        pby += sampleFrequencies[pitchbendIndex].ToString() + ",";
                    }

                    pitchbendIndex++;
                }

                if (pby.Length > 0) 
                {
                    pby = pby.Substring(0, pby.Length - 1);
                }
                
                note.SetPbw(pbw);
                note.SetPby(pby);
            }
        }
    }
}
