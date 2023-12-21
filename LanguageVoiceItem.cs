using CommunityToolkit.Mvvm.ComponentModel;

namespace IKSTranslateApp;

internal partial class LanguageVoiceItem : ObservableObject
{
    [ObservableProperty] internal string languageName; // Reader-friendly language name
    [ObservableProperty] internal string languageCode; // Language code (for speech translation)
    [ObservableProperty] internal string languageLocale; // Language-Locale (for speech-to-text and text-to-speech)
    [ObservableProperty] internal string voiceName; // Voice (for text to speech)

    [ObservableProperty] internal string languageNameAndLocale;
    [ObservableProperty] internal string languageNameAndVoice;

    internal LanguageVoiceItem(string languageName, string languageCode, string languageLocale, string voiceName)
    {
        LanguageName = languageName;
        LanguageCode = languageCode;
        LanguageLocale = languageLocale;
        VoiceName = voiceName;

        LanguageNameAndLocale = $"{LanguageName} [{languageLocale}]";
        LanguageNameAndVoice = $"{LanguageName} [{voiceName}]";
    }
}
