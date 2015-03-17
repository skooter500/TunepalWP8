#include "AudioController.h"

#include <ppltasks.h>

#include "AudioRecorder.h"
#include "tunepal/config.h"

using namespace TunepalLib;

AudioController::AudioController()
{
}

Windows::Foundation::IAsyncActionWithProgress<RecordingState^>^ AudioController::Record(int fundamental)
{
    Config::setFundamental(fundamental);

    _audioRecorder = Microsoft::WRL::Make<AudioRecorder>();

    _audioRecorder->StartRecordingAsync();

    return Concurrency::create_async(
        [this](Concurrency::progress_reporter<RecordingState^> reporter) {
            RecordingState^ state = ref new RecordingState();

            while (_audioRecorder->Running()) {
                state->Progress = _audioRecorder->Progress();
                state->Amplitude = _audioRecorder->Amplitude();
                reporter.report(state);
                Concurrency::wait(25);
            }
    });
}

Windows::Foundation::IAsyncOperation<Platform::String^>^ AudioController::Transcribe()
{
    return Concurrency::create_async(
        [this]() -> Platform::String^ {
            while (_audioRecorder->TranscriptionState() != FINISHED) {
                Concurrency::wait(0);
            }

            Platform::String^ transcription = _audioRecorder->Transcription();
            return transcription;
    });
}

void AudioController::Cancel()
{
    _audioRecorder->CancelRecording();
}
