#pragma once

#include <wrl\implements.h>
#include <mmdeviceapi.h>

namespace TunepalLib
{
    enum State {UNSTARTED, RUNNING, FINISHED};

    class AudioRecorder :
        public Microsoft::WRL::RuntimeClass<Microsoft::WRL::RuntimeClassFlags<Microsoft::WRL::ClassicCom>, Microsoft::WRL::FtmBase, IActivateAudioInterfaceCompletionHandler>
    {
    public:
        AudioRecorder();

        void StartRecordingAsync();

        void CancelRecording() { _running = false; }
        bool Running() { return _running; }
        float Progress() { return _progress; }
        float Amplitude() { return _amplitude; }
        Platform::String^ Transcription() { return _transcription; }
        State TranscriptionState() { return _transcriptionState; }

        STDMETHOD(ActivateCompleted)(IActivateAudioInterfaceAsyncOperation *operation);

    private:
        bool _running;
        float _progress;
        float _amplitude;
        Platform::String^ _transcription;
        State _transcriptionState;
    };
}
