using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace IKSTranslateApp;

internal partial class MainPageViewModel : ObservableObject
{
    [ObservableProperty] private MainPageModel model;

    #region Model-independent fields

    // Selectable items with default values
    [ObservableProperty] private LanguageVoiceItem selectedSourceLanguageVoiceItem = new("English", "en", "en-US", null);
    [ObservableProperty] private LanguageVoiceItem selectedTargetLanguageVoiceItem = new("German", "de", "de-DE", "de-DE-KatjaNeural");

    // Free-text entry
    [ObservableProperty] private string inputText = string.Empty;

    // Flag for readiness state
    [ObservableProperty] private bool isReady = true;
    [ObservableProperty] private bool isBusy = false;

    #endregion

    #region Model-dependent fields

    // Text output
    [ObservableProperty] private ObservableCollection<OutputMessage> outputMessages = [];

    #endregion

    public MainPageViewModel()
    {
        if (App.ErrorMessage is not null)
        {
            IsReady = false;
            OutputMessages.Add(App.ErrorMessage);
        }
        else
        {
            Model = new MainPageModel();
            OutputMessages = Model.OutputMessages;

            Load();
        }
    }

    private void Load()
    {
        var sourceLanguageLocale = Util.LoadTextPropertySync("translate_source_language_locale", false);
        if (sourceLanguageLocale == string.Empty)
        {
            sourceLanguageLocale = SelectedSourceLanguageVoiceItem.LanguageLocale;
        }
        SelectedSourceLanguageVoiceItem = Model.SourceLanguageVoiceItems.ToList().Find(x => x.LanguageLocale == sourceLanguageLocale);

        var targetLanguageLocale = Util.LoadTextPropertySync("translate_target_language_locale", false);
        var voiceName = Util.LoadTextPropertySync("translate_voice_name", false);
        if (targetLanguageLocale == string.Empty || voiceName == string.Empty)
        {
            targetLanguageLocale = SelectedTargetLanguageVoiceItem.LanguageLocale;
            voiceName = SelectedTargetLanguageVoiceItem.VoiceName;
        }
        SelectedTargetLanguageVoiceItem = Model.TargetLanguageVoiceItems.ToList().Find(x => x.LanguageLocale == targetLanguageLocale && x.VoiceName == voiceName);
    }

    [RelayCommand]
    public void Save(string property)
    {
        switch (property)
        {
            case "SourceLanguageVoiceItem":
                Util.SaveTextProperty("translate_source_language_locale", SelectedSourceLanguageVoiceItem.LanguageLocale, false);
                break;
            case "TargetLanguageVoiceItem":
                Util.SaveTextProperty("translate_target_language_locale", SelectedTargetLanguageVoiceItem.LanguageLocale, false);
                Util.SaveTextProperty("translate_voice_name", SelectedTargetLanguageVoiceItem.VoiceName, false);
                break;
            default:
                throw new ArgumentException($"Property '{property}' unknown.");
        }
    }

    [RelayCommand]
    public async Task Speak()
    {
        IsReady = false;
        IsBusy = true;
        await Model.Speak(SelectedSourceLanguageVoiceItem, SelectedTargetLanguageVoiceItem);
        InputText = string.Empty;
        IsReady = true;
        IsBusy = false;
    }

    [RelayCommand]
    public async Task Send()
    {
        IsReady = false;
        IsBusy = true;
        await Model.Send(InputText, SelectedSourceLanguageVoiceItem, SelectedTargetLanguageVoiceItem);
        InputText = string.Empty;
        IsReady = true;
        IsBusy = false;
    }
}