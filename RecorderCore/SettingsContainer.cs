using System;
using System.Collections.Generic;
using System.Text;

namespace RecorderCore
{
    public class SettingsContainer
    {
        public enum RecordingType
        {
            Camera,
            Hilbert,
            Step
        }

        public enum ProcessingStep
        {
            Interferogramm,
            WrappedPhaseImage,
            UnwrappedPhaseImage,
            ProcessedImage

        }


        public RecordingType recordingType = RecordingType.Camera;
        public ProcessingStep maxProcessingStep = ProcessingStep.Interferogramm;
        public int MaximumSteps = 3;
        public int Camera = 0;
        public int FramePause = 0;
        public bool model = false;

        public bool AreEqual(SettingsContainer settings)
        {
            return this.maxProcessingStep == settings.maxProcessingStep || this.recordingType == settings.recordingType || this.MaximumSteps == settings.MaximumSteps|| (this.model == settings.model);
        }
    }

}
