#include "AudioRecorder.h"

#include <AudioClient.h>
#include <ppltasks.h>

#include "common.h"
#include "tunepal/transcriber.h"
#include "libsndfile/sndfile.h"

using namespace Platform;
using namespace Windows::Media::Devices;
using namespace Windows::Storage;
using namespace TunepalLib;

inline int utf16_to_utf8(const wchar_t *src, char *dest, int destSize)
{
    return WideCharToMultiByte(CP_UTF8, 0, src, -1, dest, destSize, NULL, NULL);
}

char *platform_string_to_c_string(String^ pstr)
{
    int max_utf8_len = pstr->Length() * 3;
    char *cstr = new char[max_utf8_len];
    utf16_to_utf8(pstr->Data(), cstr, max_utf8_len);

    return cstr;
}

AudioRecorder::AudioRecorder()
{
    _running = false;
    _progress = 0;
    _amplitude = 0;
    _transcription = nullptr;
    _transcriptionState = UNSTARTED;
}

void AudioRecorder::StartRecordingAsync()
{
    if (!_running)
    {
        // init variables
        _running = true;
        _progress = 0;
        _amplitude = 0;
        _transcription = nullptr;

        IActivateAudioInterfaceAsyncOperation *asyncOp;

        // Get a string representing the Default Audio Capture Device
        Platform::String^ deviceIdString = MediaDevice::GetDefaultAudioCaptureId(Windows::Media::Devices::AudioDeviceRole::Default);

        // This call must be made on the main UI thread.  Async operation will call back to
        // IActivateAudioInterfaceCompletionHandler::ActivateCompleted, which must be an agile interface implementation
        HRESULT hr = ActivateAudioInterfaceAsync(deviceIdString->Data(), __uuidof(IAudioClient2), nullptr, this, &asyncOp);
        THROW_IF_FAILED(hr);
        SAFE_RELEASE(asyncOp);
    }
}

//
//  ActivateCompleted()
//
//  Callback implementation of ActivateAudioInterfaceAsync function.  This will be called on MTA thread
//  when results of the activation are available.
//
HRESULT AudioRecorder::ActivateCompleted( IActivateAudioInterfaceAsyncOperation *operation )
{
    IUnknown *punkAudioInterface = nullptr;
    IAudioClient *audioClient = nullptr;
    IAudioCaptureClient *audioCaptureClient = nullptr;
    WAVEFORMATEX *mixFormat = nullptr;

    float *signal = nullptr;
    int signalIndex = 0;

    HRESULT hr = S_OK;
    HRESULT hrActivateResult = S_OK;

    int sampleTime = 12;

    // Check for a successful activation result
    hr = operation->GetActivateResult( &hrActivateResult, &punkAudioInterface );
    THROW_IF_FAILED(hr);
    THROW_IF_FAILED(hrActivateResult);

    // Get the pointer for the Audio Client
    hr = punkAudioInterface->QueryInterface( IID_PPV_ARGS(&audioClient) );
    THROW_IF_FAILED(hr);

    // Get the Mix Format
    hr = audioClient->GetMixFormat(&mixFormat);
    THROW_IF_FAILED(hr);

    // Know whether it's PCM or float audio
    WAVEFORMATEXTENSIBLE *mixFormatEx = (WAVEFORMATEXTENSIBLE*)mixFormat;

    bool pcmAudio = (mixFormat->wFormatTag == WAVE_FORMAT_PCM
        || mixFormat->wFormatTag == WAVE_FORMAT_EXTENSIBLE
        && mixFormatEx->SubFormat == KSDATAFORMAT_SUBTYPE_PCM);
    bool floatAudio = (mixFormat->wFormatTag == WAVE_FORMAT_IEEE_FLOAT
        || mixFormat->wFormatTag == WAVE_FORMAT_EXTENSIBLE
        && mixFormatEx->SubFormat == KSDATAFORMAT_SUBTYPE_IEEE_FLOAT);

    if (!(pcmAudio || floatAudio)) throw Platform::Exception::CreateException(AUDCLNT_E_UNSUPPORTED_FORMAT);
    // Initialize the AudioClient in Shared Mode with the user specified buffer


    hr = audioClient->Initialize(AUDCLNT_SHAREMODE_SHARED,
        0,
        250000,  // Ideally, set the buffer size to be able to contain audio of 250000e-7 secs (0.025 secs, 40fps)
		0,
		mixFormat,
        nullptr);
    THROW_IF_FAILED(hr);

    // Get the maximum size of the AudioClient Buffer
    UINT32 bufferFrameCount;
    hr = audioClient->GetBufferSize(&bufferFrameCount);
    THROW_IF_FAILED(hr);

    // Get the capture client
    hr = audioClient->GetService( __uuidof(IAudioCaptureClient), (void**) &audioCaptureClient );
    THROW_IF_FAILED(hr);

    // Calculate the actual duration (in millisecs) of the allocated buffer
    UINT32 actualDuration = 1000 * bufferFrameCount / mixFormat->nSamplesPerSec;

    // Calculate the size of all signals
    int signalLength = mixFormat->nSamplesPerSec * sampleTime;

    // Initialize the array of signals
    signal = new float[signalLength];
    short *pcmSignal = new short[signalLength];

    // Start recording
    hr = audioClient->Start();
    THROW_IF_FAILED(hr);

    _running = true;

    // Each loop fills about half of the shared buffer
    while (_running)
    {
        // yield cpu time
        concurrency::wait(0);

        UINT32 numFramesAvailable = 0;
        hr = audioCaptureClient->GetNextPacketSize(&numFramesAvailable);
        THROW_IF_FAILED(hr);

        while (numFramesAvailable > 0 && _running)
        {
            DWORD recordingFlag;
            BYTE *byteBuffer;

            // Get the available data in the shared buffer
            hr = audioCaptureClient->GetBuffer(
                &byteBuffer,
                &numFramesAvailable,
                &recordingFlag, NULL, NULL);
            THROW_IF_FAILED(hr);

            if (byteBuffer == nullptr) break;

            if (recordingFlag & AUDCLNT_BUFFERFLAGS_SILENT)
            {
                memset(byteBuffer, 0, numFramesAvailable * mixFormat->nBlockAlign);
            }

            float amplitudeSum = 0;

            for (int i = 0; i < (int)numFramesAvailable; i++)
            {
                const int AMPLITUDE_RANGE = 32768; // 2^16 / 2
                float amplitude = 0;

                if (floatAudio) {
                    amplitude = *((float*)byteBuffer) * AMPLITUDE_RANGE;
                }
                else if (mixFormat->wBitsPerSample == 8) {
                    // 8-bit PCM is unsigned
                    amplitude = *byteBuffer;
                }
                else {
                    // WAVEFORMATEX supports only 8-bit or 16-bit PCM
                    // 16-bit PCM is signed
                    amplitude = *((short*)byteBuffer);
                }

                // pointer increment in loop
                byteBuffer += mixFormat->nBlockAlign;

                amplitudeSum += amplitude;

                // save the amplitude
                signal[signalIndex] = amplitude;
                pcmSignal[signalIndex] = (short)amplitude;
                ++signalIndex;

                if (signalIndex >= signalLength)
                {
                    hr = audioClient->Stop();  // Stop recording
                    THROW_IF_FAILED(hr);

                    _running = false;
                    _transcriptionState = RUNNING;

                    //TEST: save the signal to a .wav file
                    SF_INFO stinfo;
                    stinfo.frames = signalLength;
                    stinfo.samplerate = mixFormat->nSamplesPerSec;
                    stinfo.channels = 1;
                    stinfo.format = SF_FORMAT_WAV | SF_FORMAT_PCM_16;
                    stinfo.sections = 1;
                    stinfo.seekable = 1;

					/*
                    String^ wavName = "\\signal.wav";
                    String^ wavPath = String::Concat(ApplicationData::Current->LocalFolder->Path, wavName);
                    const char* outfilename = platform_string_to_c_string(wavPath);

                    SNDFILE *sndfile = sf_open(outfilename, SFM_WRITE, &stinfo);

                    sf_count_t numWritten = sf_write_short(sndfile, pcmSignal, (sf_count_t)signalLength);
                    if (numWritten != signalLength) {
                        int test = 1;
                    }

                    sf_close(sndfile);
                    delete outfilename;
					*/
                    Transcriber transcriber(mixFormat->nSamplesPerSec, sampleTime);
                    float f = 0.0f;
                    bool b = false;

                    transcriber.setSignal(signal);
                    string transcription = transcriber.transcribe(&f, &b, false);

                    wchar_t tmp[1024];
					mbstowcs(tmp, transcription.c_str(), transcription.length());
                    tmp[transcription.length()] = 0;

                    _transcription = ref new Platform::String(tmp);

                    _transcriptionState = FINISHED;

                    break;
                }
            }

            // get the representative amplitude of a short period of time
            _amplitude = amplitudeSum / numFramesAvailable;

            _progress = (float)signalIndex / signalLength;

            hr = audioCaptureClient->ReleaseBuffer(numFramesAvailable);
            THROW_IF_FAILED(hr);

            hr = audioCaptureClient->GetNextPacketSize(&numFramesAvailable);
            THROW_IF_FAILED(hr);
        }
    }

    hr = audioClient->Stop();  // Stop recording
    THROW_IF_FAILED(hr);

    delete[] signal;
    SAFE_RELEASE(punkAudioInterface);
    SAFE_RELEASE(audioClient);
    SAFE_RELEASE(audioCaptureClient);

    // Need to return S_OK
    return S_OK;
}
