using CommunityToolkit.Mvvm.ComponentModel;

namespace IKSTranslateApp;

internal partial class SettingsModel : ObservableObject
{
    [ObservableProperty] internal string translatorSubscriptionKey;
    [ObservableProperty] internal string translatorServiceEndpoint;
    [ObservableProperty] internal string translatorServiceRegion;
    [ObservableProperty] internal string speechSubscriptionKey;
    [ObservableProperty] internal string speechServiceRegion;
    [ObservableProperty] internal int? initialSilenceTimeoutMs;
    [ObservableProperty] internal int? endSilenceTimeoutMs;

    internal SettingsModel()
    {
        Load();
    }

    public void Load()
    {
        TranslatorSubscriptionKey = Util.LoadTextPropertySync("translator_subscription_key", true);
        TranslatorServiceEndpoint = Util.LoadTextPropertySync("translator_service_endpoint", true);
        TranslatorServiceRegion = Util.LoadTextPropertySync("translator_service_region", true);
        SpeechSubscriptionKey = Util.LoadTextPropertySync("speech_subscription_key", true);
        SpeechServiceRegion = Util.LoadTextPropertySync("speech_service_region", true);
        InitialSilenceTimeoutMs = Util.GetInt(Util.LoadTextPropertySync("initial_silence_timeout_ms", true));
        EndSilenceTimeoutMs = Util.GetInt(Util.LoadTextPropertySync("end_silence_timeout_ms", true));

        if (TranslatorSubscriptionKey is null
            || TranslatorServiceEndpoint is null
            || TranslatorServiceRegion is null
            || SpeechSubscriptionKey is null
            || SpeechServiceRegion is null
            || InitialSilenceTimeoutMs is null
            || EndSilenceTimeoutMs is null)
        {
            App.ErrorMessage = Util.GetMissingSettingsText();
        }
    }

    public void Save(string property)
    {
        switch (property)
        {
            case "TranslatorSubscriptionKey":
                Util.SaveTextProperty("translator_subscription_key", TranslatorSubscriptionKey, true);
                break;
            case "TranslatorServiceEndpoint":
                Util.SaveTextProperty("translator_service_endpoint", TranslatorServiceEndpoint, true);
                break;
            case "TranslatorServiceRegion":
                Util.SaveTextProperty("translator_service_region", TranslatorServiceRegion, true);
                break;
            case "SpeechSubscriptionKey":
                Util.SaveTextProperty("speech_subscription_key", SpeechSubscriptionKey, true);
                break;
            case "SpeechServiceRegion":
                Util.SaveTextProperty("speech_service_region", SpeechServiceRegion, true);
                break;
            case "InitialSilenceTimeoutMs":
                Util.SaveTextProperty("initial_silence_timeout_ms", InitialSilenceTimeoutMs.ToString(), true);
                break;
            case "EndSilenceTimeoutMs":
                Util.SaveTextProperty("end_silence_timeout_ms", EndSilenceTimeoutMs.ToString(), true);
                break;
            default:
                throw new ArgumentException($"Property '{property}' unknown.");
        }
    }
}
