/*
 * Developer : Bryan Duggan (bryan.duggan@dit.ie)
 * All code (c)2010 Dublin Institute of Technology. All rights reserved
 */

#include "transcriber.h"
#include "tunepalconstants.h"
#include "pitchspeller.h"
#include "FuzzyHistogram.h"
#include "fft.h"
#include "PitchDetector.h"

#include <vector>

#include <math.h>
#include <iostream>


using namespace std;

double round(double number)
{
    return number < 0.0 ? ceil(number - 0.5) : floor(number + 0.5);
}

void Transcriber::setSignal(float * signal)
{
	this->signal = signal;
}

string Transcriber::transcribe(float * progress, bool * interrupted, bool midi)
{
	PitchSpeller speller;
	string lastNote;
	speller.makeScale("major");
    speller.makeMidiNotes();
	float spectrum[(int) FRAME_SIZE / 2];
	float hopSize = FRAME_SIZE * (1.0f - OVERLAP);
	
	int numHops = numSamples / hopSize;
	
	int i;
	for (i = 0 ; i < numHops ; i ++)
	{
		int startAt = hopSize * i;
		* progress = (float) i / (float) numHops;
		if (* interrupted)
		{
			return "";
		}
		
		// can we get another frame out without going over?
		if (startAt + FRAME_SIZE >= numSamples)
		{
			break;
		}
		
		WindowFunc(HANNING, FRAME_SIZE, signal + startAt);
		PowerSpectrum(FRAME_SIZE, signal + startAt, spectrum);
    
		float frequency = mikelsFrequency(spectrum, FRAME_SIZE / 2, sampleRate, FRAME_SIZE);
        string currentNote;
		if (midi)
        {
             currentNote = speller.spellFrequencyAsMidi(frequency);
        }
        else {
            currentNote = speller.spellFrequency(frequency);
        }
        if (currentNote != lastNote)
        {
            TranscribedNote note;
            note.spelling = currentNote;
            note.frequency = frequency;					
            note.onset = ((float) startAt) / sampleRate;			
            lastNote = currentNote; 
            notes.push_back(note);
        }

	}
	numHops = i;
	printTranscription();
	postProcess(midi);
	printf("transcription: %s\n", transcription.c_str());
	return transcription;
}

void Transcriber::printTranscription()
{
	for (int i = 0 ; i < notes.size() ; i ++)
	{
		printf("%s %f %f %f %f\n", notes[i].spelling.c_str(), notes[i].frequency, notes[i].qq, notes[i].onset, notes[i].duration);
	}
		
}

void Transcriber::postProcess(bool midi)
{
	transcription = "";
	// put in the durations	
	for (int i = 0 ; i < (notes.size() - 1) ; i ++)
	{
		notes[i].duration = notes[i + 1].onset - notes[i].onset;
	}
	// Now do the last note
	notes[notes.size() - 1].duration = sampleTime - notes[notes.size() - 1].onset;

	float *durations = new float[notes.size()];
	for (int i = 0 ; i < notes.size() ; i ++)
	{
		durations[i] = notes[i].duration;
	}
	
	FuzzyHistogram fuzzyHistogram;
	float quaverLength = fuzzyHistogram.calculatePeek(durations, notes.size(), 0.33f, 0.1f);

	delete durations;

	cout << "quaverLength " << quaverLength << endl;
	// Now calculate the quaver quantisation for each note
	for (int i = 0 ; i < notes.size() ; i ++)
	{
		notes[i].qq = round((float)notes[i].duration / (float)quaverLength);	
	}	
	printTranscription();
	
	//Convert the sequence of notes to a string, removing the 0 durations and expanding the multiople durations
	bool pastSilence = false;
	for (int i = 0 ; i < notes.size() ; i ++)
	{
		// Skip Z's at the start
		cout << notes[i].spelling << endl;
		if (notes[i].spelling != "Z")
		{
			pastSilence = true;
		}
		for (int j = 0 ; j < notes[i].qq ; j ++)
		{				
			if (pastSilence)
			{
				if (notes[i].spelling != "Z")
				{
					transcription += notes[i].spelling;
				}
                if (midi && notes[i].spelling != "Z")
                {
                    transcription += ",";
                }            
			}
		}
	}	
	cout << transcription << endl;
	// Remove Z's at the end
	if (transcription.size() > 0)
	{
		int i = transcription.size() - 1;
		while (i > 0)
		{
			if (transcription[i] == 'Z')
			{
				transcription.erase(i, 1);
				i --;
			}
			else
			{
				break;
			}
		}		
	}
	cout << transcription << endl;
	// Count the Z's
	if (transcription.size() > 0)
	{
		int numZ = 0;
		for (int i = 0 ; i < transcription.size() ; i ++)
		{
			if (transcription[i] == 'Z')
			{
				numZ ++;
			}		
		}
		// If more than 50%, we heard mostly silence
		float normalZ = (float) numZ / (float) transcription.size();
		cout << "Normal z" << normalZ << endl;
		if (normalZ > 0.5f)
		{
			cout << "Transcription is mostly silence, so I'm removing all the notes" << endl;
			transcription = "";
		}
	}	
}

Transcriber::Transcriber()
{
    sampleRate = DEFAULT_SAMPLE_RATE;
    sampleTime = DEFAULT_SAMPLE_TIME;
    numSamples = sampleRate * sampleTime;
}

Transcriber::Transcriber(int sampleRate, int sampleTime)
{
    this->sampleRate = sampleRate;
    this->sampleTime = sampleTime;
    numSamples = sampleRate * sampleTime;
}

Transcriber::Transcriber(char * audioData, int numSamples)
{
	signal = new float[numSamples];
	for (int signalIndex = 0 ; signalIndex < numSamples; signalIndex ++)
	{
		signal[signalIndex] = ((audioData[(signalIndex * 2) + 1] << 8) + audioData[signalIndex * 2]);
	}  
	
	this->numSamples = numSamples;
}

Transcriber::~Transcriber()
{
	//delete signal;
}
