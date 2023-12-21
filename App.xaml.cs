using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Translation;
using Plugin.Maui.Audio;

namespace IKSTranslateApp;

public partial class App : Application
{
    internal static SettingsModel SettingsModel;

    internal static OutputMessage ErrorMessage;

    internal static SpeechConfig SpeechConfig;
    internal static SpeechRecognizer SpeechRecognizer;
    internal static SpeechSynthesizer SpeechSynthesizer;

    internal static SpeechTranslationConfig SpeechTranslationConfig;
    internal static TranslationRecognizer TranslationRecognizer;

    internal static AudioConfig AudioConfig;
    internal static MemoryStream AudioStream;
    internal static IAudioPlayer AudioPlayer;

    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();

        SettingsModel = new SettingsModel();

        if (ErrorMessage is null)
        {
            try
            {
                SpeechConfig = SpeechConfig.FromSubscription(SettingsModel.SpeechSubscriptionKey, SettingsModel.SpeechServiceRegion);
                SpeechConfig.SetProperty(PropertyId.SpeechServiceConnection_InitialSilenceTimeoutMs, SettingsModel.InitialSilenceTimeoutMs.ToString());
                SpeechConfig.SetProperty(PropertyId.SpeechServiceConnection_EndSilenceTimeoutMs, SettingsModel.EndSilenceTimeoutMs.ToString());
                SpeechConfig.SetProfanity(ProfanityOption.Raw);

                AudioConfig = AudioConfig.FromDefaultMicrophoneInput();

                SpeechRecognizer = new SpeechRecognizer(SpeechConfig, AudioConfig);
                SpeechSynthesizer = new SpeechSynthesizer(SpeechConfig);
            }
            catch (Exception ex)
            {
                ErrorMessage = Util.GetErrorText(ex);
            }
        }
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        Window window = base.CreateWindow(activationState);

        window.Destroying += (s, e) =>
        {
            try
            {
                AudioConfig?.Dispose();

                if (SpeechRecognizer is not null)
                {
                    SpeechRecognizer.StopContinuousRecognitionAsync();
                    Task.Delay(1000);
                    Connection.FromRecognizer(SpeechRecognizer).Close();
                    SpeechRecognizer.Dispose();
                }
            }
            catch (Exception) { }
        };

        return window;
    }
}
