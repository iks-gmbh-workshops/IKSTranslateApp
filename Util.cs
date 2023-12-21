using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Translation;
using Plugin.Maui.Audio;

namespace IKSTranslateApp;

internal static class Util
{
    internal static void ScrollToEnd(ScrollView scv, StackLayout s)
    {
        new Timer((object obj) =>
        {
            MainThread.BeginInvokeOnMainThread(async () => await scv.ScrollToAsync(0, s.Height, true));
        }, null, 1, Timeout.Infinite);
    }

    internal static async Task<bool> EnsureMicrophonePermission()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Microphone>();
        }
        return status == PermissionStatus.Granted;
    }

    internal static OutputMessage GetInfoText(string text)
    {
        return new OutputMessage { Type = OutputMessageType.Information, Text = text };
    }

    internal static OutputMessage GetNoMatchText()
    {
        return new OutputMessage { Type = OutputMessageType.Information, Text = "SPEECH NOT RECOGNIZED" };
    }

    internal static OutputMessage GetMissingSettingsText()
    {
        return new OutputMessage { Type = OutputMessageType.Information, Text = "SETTINGS MISSING. Fill in the 'Settings' page and restart the app." };
    }

    internal static OutputMessage GetCanceledText(RecognitionResult result)
    {
        var cancellation = CancellationDetails.FromResult(result);
        return GetCancellationDetailsText(cancellation.Reason, cancellation.ErrorCode, cancellation.ErrorDetails);
    }

    internal static OutputMessage GetCanceledText(SpeechSynthesisResult result)
    {
        var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
        return GetCancellationDetailsText(cancellation.Reason, cancellation.ErrorCode, cancellation.ErrorDetails);
    }

    private static OutputMessage GetCancellationDetailsText(CancellationReason reason, CancellationErrorCode errorCode, string errorDetails)
    {
        var text = string.Empty;

        text += $"CANCELED: Reason = {reason}";

        if (reason == CancellationReason.Error)
        {
            text += $", ErrorCode = {errorCode}";
            text += $", ErrorDetails = {errorDetails}";
        }

        return new OutputMessage { Type = OutputMessageType.Error, Text = text };
    }

    internal static OutputMessage GetErrorText(Exception ex)
    {
        return new OutputMessage { Type = OutputMessageType.Error, Text = ex.Message };
    }

    internal static OutputMessage GetNoResponseText()
    {
        return new OutputMessage { Type = OutputMessageType.Error, Text = "NO RESPONSE" };
    }

    internal static async Task<OutputMessage> Synthesize(string text, string languageLocale, string voiceName, string style)
    {
        if (text == null)
        {
            return Util.GetNoResponseText();
        }

        var ssmlText = $"<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xmlns:mstts=\"https://www.w3.org/2001/mstts\" xml:lang=\"{languageLocale}\">";
        ssmlText += $"<voice name=\"{voiceName}\">";
        ssmlText += $"<mstts:express-as style=\"{style}\" styledegree=\"1\">{text}</mstts:express-as>";
        ssmlText += $"</voice>";
        ssmlText += $"</speak>";

        var result = await App.SpeechSynthesizer.SpeakSsmlAsync(ssmlText);

        return result.Reason switch
        {
            ResultReason.Canceled => GetCanceledText(result),
            _ => null
        };
    }

    internal static string LoadTextPropertySync(string key, bool isSecure)
    {
       return Task<string>.Run(() => LoadTextProperty(key, isSecure)).Result;
    }

    internal static async Task<string> LoadTextProperty(string key, bool isSecure)
    {
        return isSecure ? await SecureStorage.Default.GetAsync(key) : Preferences.Default.Get(key, string.Empty);
    }

    internal static void SaveTextProperty(string key, string value, bool isSecure)
    {
        if (isSecure)
        {
            if (string.IsNullOrEmpty(value))
            {
                SecureStorage.Default.Remove(key);
            }
            else
            {
                SecureStorage.Default.SetAsync(key, value);
            }
        }
        else
        {
            Preferences.Default.Set(key, value);
        }
    }

    internal static int? GetInt(string value)
    {
        bool success = int.TryParse(value, out int number);
        return success ? number : null;
    }

    internal static OutputMessage EnsureSpeechSynthesizer(string speechRecognitionLanguage, string speechSynthesisLanguage, string speechSynthesisVoiceName)
    {
        OutputMessage statusMessage = null;

        if (App.SpeechSynthesizer is null
            || App.SpeechConfig.SpeechRecognitionLanguage != speechRecognitionLanguage
            || App.SpeechConfig.SpeechSynthesisLanguage != speechSynthesisLanguage
            || App.SpeechConfig.SpeechSynthesisVoiceName != speechSynthesisVoiceName)
        {
            App.SpeechConfig.SpeechRecognitionLanguage = speechRecognitionLanguage;
            App.SpeechConfig.SpeechSynthesisLanguage = speechSynthesisLanguage;
            App.SpeechConfig.SpeechSynthesisVoiceName = speechSynthesisVoiceName;
            
            try
            {
                App.SpeechSynthesizer = new SpeechSynthesizer(App.SpeechConfig);
            }
            catch (Exception ex)
            {
                statusMessage = GetErrorText(ex);
            }
        }

        return statusMessage;
    }

    internal static OutputMessage EnsureTranslationRecognizer(string speechRecognitionLanguageLocale, string targetLanguageCode, string speechSynthesisLanguageLocale, string speechSynthesisVoiceName)
    {
        OutputMessage statusMessage = null;

        if (App.TranslationRecognizer is null
            || App.SpeechTranslationConfig.SpeechRecognitionLanguage != speechRecognitionLanguageLocale
            || App.SpeechTranslationConfig.SpeechSynthesisLanguage != speechSynthesisLanguageLocale
            || App.SpeechTranslationConfig.SpeechSynthesisVoiceName != speechSynthesisVoiceName
            || !App.SpeechTranslationConfig.TargetLanguages.Contains(targetLanguageCode))
        {
            App.SpeechTranslationConfig = SpeechTranslationConfig.FromSubscription(App.SettingsModel.SpeechSubscriptionKey, App.SettingsModel.SpeechServiceRegion);

            App.SpeechTranslationConfig.SpeechRecognitionLanguage = speechRecognitionLanguageLocale;
            App.SpeechTranslationConfig.SpeechSynthesisLanguage = speechSynthesisLanguageLocale;
            App.SpeechTranslationConfig.SpeechSynthesisVoiceName = speechSynthesisVoiceName;
            App.SpeechTranslationConfig.VoiceName = speechSynthesisVoiceName;

            App.SpeechTranslationConfig.AddTargetLanguage(targetLanguageCode);

            App.SpeechTranslationConfig.SetProperty(PropertyId.SpeechServiceConnection_InitialSilenceTimeoutMs, App.SettingsModel.InitialSilenceTimeoutMs.ToString());
            App.SpeechTranslationConfig.SetProperty(PropertyId.SpeechServiceConnection_EndSilenceTimeoutMs, App.SettingsModel.EndSilenceTimeoutMs.ToString());
            App.SpeechTranslationConfig.SetProfanity(ProfanityOption.Raw);

            try
            {
                if (App.TranslationRecognizer is not null)
                {
                    App.TranslationRecognizer.Synthesizing -= PlayVoice;
                }
                App.TranslationRecognizer = new TranslationRecognizer(App.SpeechTranslationConfig, App.AudioConfig);
                App.TranslationRecognizer.Synthesizing += PlayVoice;
            }
            catch (Exception ex)
            {
                statusMessage = Util.GetErrorText(ex);
            }
        }

        return statusMessage;
    }

    private static void PlayVoice(object sender, TranslationSynthesisEventArgs e)
    {
        if (e.Result.Reason == ResultReason.SynthesizingAudio)
        {
            PlayAudio(e.Result.GetAudio());
        }
    }

    private static void PlayAudio(byte[] audioData)
    {
        if (audioData is not null && audioData.Length > 44) // Workaround for corrupt audio
        {
            // Play audio
            App.AudioStream = new MemoryStream(audioData); // Can't use USING or dispose in this scope (doesn't speak on Windows)
            App.AudioPlayer = AudioManager.Current.CreatePlayer(App.AudioStream);  // Can't use USING or dispose in this scope (doesn't speak on Windows)
            App.AudioPlayer.Play();
        }
    }

    internal async static Task CopyToClipboard(string text)
    {
        await Clipboard.Default.SetTextAsync(text);

        var toast = Toast.Make("Copied", ToastDuration.Short, 14);
        await toast.Show(new CancellationTokenSource().Token);
    }
}