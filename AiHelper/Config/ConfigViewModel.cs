using System.IO.Pipes;
using System.Windows.Input;
using System.Windows.Media.Animation;
using AiHelper.Config.Models;
using NAudio.Wave;

namespace AiHelper.Config
{
    public class ConfigViewModel : ViewModelBase
    {
        private readonly Action<bool> close;

        public ConfigViewModel(AiHelperConfig? config, Action<bool> close)
        {
            this.close = close;
            this.OkCommand = new RelayCommand(() => this.Close(true));
            this.CancelCommand = new RelayCommand(() => this.Close(false));
            if (config != null)
            {
                this.Config = config.Clone();
            }
            else
            {
                this.Config = new AiHelperConfig { EMailConfig = new() };
            }

            this.VolumeLimit = ConfigProvider.Config.SoundConfig.SilenceVolumeLimit;

            Task.Run(MonitorNoise);
        }

        private bool isListening = true;


        public ICommand OkCommand { get; }

        public ICommand CancelCommand { get; }

        public AiHelperConfig Config { get; private set; }

        private async void MonitorNoise()
        {
            WaveInEvent waveIn;
            waveIn = new WaveInEvent();
            waveIn.DeviceNumber = 0;
            waveIn.WaveFormat = new WaveFormat(16000, 1);
            waveIn.BufferMilliseconds = 100;


            waveIn.DataAvailable += (object? sender, WaveInEventArgs e) =>
            {
                double rawMaxVolume = AudioTools.GetMaxVolume(e);

                double logValue = Math.Log10(rawMaxVolume);
                this.MaxVolume = (logValue + 5.0) / 5.0;

                this.IsAboveLimit = rawMaxVolume > this.VolumeLimit;
            };

            waveIn.StartRecording();

            while(isListening)
            {
                await Task.Delay(100);
            }
        }

        internal void Close(bool result)
        {
            isListening = false;
            this.Config.SoundConfig.SilenceVolumeLimit = this.VolumeLimit;
            this.close(result);
        }

        internal void StopListening()
        {
            this.isListening = false;
        }

        private double maxVolume = 0;

        public double MaxVolume
        {
            get => this.maxVolume;
            set
            {
                this.maxVolume = value;
                OnPropertyChanged();
            }
        }

        private bool isAboveLimit = false;
        public bool IsAboveLimit
        {
            get => this.isAboveLimit;
            set
            {
                isAboveLimit = value;
                OnPropertyChanged();
            }
        }

        private double volumeLimit = 0.05;

        public double VolumeLimit
        {
            get => this.volumeLimit;
            set
            {
                this.volumeLimit = value;
                OnPropertyChanged();
            }
        }


    }
}
