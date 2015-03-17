#pragma once

#include <wrl\implements.h>

namespace TunepalLib
{
    class AudioRecorder;

    public ref struct RecordingState sealed
    {
        property float Progress;
        property float Amplitude;
    };

	public ref class AudioController sealed
	{
	public:
        AudioController();
		Windows::Foundation::IAsyncActionWithProgress<RecordingState^>^ Record(int fundamental);
        Windows::Foundation::IAsyncOperation<Platform::String^>^ Transcribe();
        void Cancel();

    private:
        Microsoft::WRL::ComPtr<AudioRecorder> _audioRecorder;
	};
}
