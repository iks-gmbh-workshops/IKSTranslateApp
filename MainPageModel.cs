using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.CognitiveServices.Speech;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Text;

namespace IKSTranslateApp;

internal partial class MainPageModel : ObservableObject
{
    [ObservableProperty] internal ObservableCollection<LanguageVoiceItem> sourceLanguageVoiceItems;
    [ObservableProperty] internal ObservableCollection<LanguageVoiceItem> targetLanguageVoiceItems;

    [ObservableProperty] internal ObservableCollection<OutputMessage> outputMessages = [];

    internal MainPageModel()
    {
        // Initialize lists of selectable values

        SourceLanguageVoiceItems =
        [
            new("Chinese", "zh-Hant", "zh-TW", null),
            new("English", "en", "en-US", null),
            new("French", "fr", "fr-FR", null),
            new ("German", "de", "de-DE", null),
            new ("Italian", "it", "it-IT", null),
            new ("Spanish", "es", "es-ES", null)
        ];

        TargetLanguageVoiceItems =
        [
            new ("Chinese", "zh-Hant", "zh-TW", "zh-TW-HsiaoChenNeural"),
            new ("English", "en", "en-US", "en-US-JennyNeural"),
            new ("English", "en", "en-US", "en-US-TonyNeural"),
            new ("French", "fr", "fr-FR", "fr-FR-BrigitteNeural"),
            new ("German", "de", "de-DE", "de-DE-KatjaNeural"),
            new ("Italian", "it", "it-IT", "it-IT-IsabellaNeural"),
            new ("Spanish", "es", "es-ES", "es-ES-ElviraNeural")
        ];
    }

    public async Task Speak(LanguageVoiceItem sourceLanguageVoiceItem, LanguageVoiceItem targetLanguageVoiceItem)
    {
        // Ensure microphone access
        if (!await Util.EnsureMicrophonePermission()) { return; }

        OutputMessages.Add(Util.GetInfoText("Listening..."));

        try
        {
            var statusMessage = Util.EnsureTranslationRecognizer(sourceLanguageVoiceItem.LanguageLocale,
                targetLanguageVoiceItem.LanguageCode,
                targetLanguageVoiceItem.LanguageLocale,
                targetLanguageVoiceItem.VoiceName);

            if (statusMessage is not null)
            {
                OutputMessages.RemoveAt(OutputMessages.Count - 1);
                OutputMessages.Add(statusMessage);
            }
            else
            {
                var result = await App.TranslationRecognizer.RecognizeOnceAsync();

                OutputMessages.RemoveAt(OutputMessages.Count - 1);

                // Output results
                switch (result.Reason)
                {
                    case ResultReason.TranslatedSpeech:
                        var translatedText = result.Translations[targetLanguageVoiceItem.LanguageCode];
                        OutputMessages.Add(new OutputMessage { Type = OutputMessageType.RecognizedText, Text = result.Text });
                        OutputMessages.Add(new OutputMessage { Type = OutputMessageType.TranslatedText, Text = translatedText });
                        break;

                    case ResultReason.NoMatch:
                        OutputMessages.Add(Util.GetNoMatchText());
                        break;

                    case ResultReason.Canceled:
                        OutputMessages.Add(Util.GetCanceledText(result));
                        break;

                    default:
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            OutputMessages.Add(Util.GetErrorText(ex));
        }
        finally
        {
        }
    }

    internal async Task Send(string text, LanguageVoiceItem sourceLanguageVoiceItem, LanguageVoiceItem targetLanguageVoiceItem)
    {
        OutputMessages.Add(new OutputMessage { Type = OutputMessageType.RecognizedText, Text = text });

        try
        {
            var route = $"/translate?api-version=3.0&from={sourceLanguageVoiceItem.LanguageCode}&to={targetLanguageVoiceItem.LanguageCode}";
            var translatedText = string.Empty;

            var body = new object[] { new { Text = text } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(App.SettingsModel.TranslatorServiceEndpoint + route);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", App.SettingsModel.TranslatorSubscriptionKey);
                request.Headers.Add("Ocp-Apim-Subscription-Region", App.SettingsModel.TranslatorServiceRegion); // required when using a multi-service or regional (not global) resource

                var response = await client.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();

                var data = (JArray)JsonConvert.DeserializeObject(result);
                translatedText = data.First.SelectToken("translations").First.SelectToken("text").ToString();
            }

            OutputMessages.Add(new OutputMessage { Type = OutputMessageType.TranslatedText, Text = translatedText });

            await Synthesize(translatedText, targetLanguageVoiceItem);

        }
        catch (Exception ex)
        {
            OutputMessages.Add(Util.GetErrorText(ex));
        }
        finally
        {
        }
    }

    private async Task Synthesize(string text, LanguageVoiceItem targetLanguageVoiceItem)
    {
        var statusMessage = Util.EnsureSpeechSynthesizer(targetLanguageVoiceItem.LanguageLocale, targetLanguageVoiceItem.LanguageLocale, targetLanguageVoiceItem.VoiceName);

        if (statusMessage is not null)
        {
            OutputMessages.Add(statusMessage);
        }
        else
        {
            var outputMessage = await Util.Synthesize(text, targetLanguageVoiceItem.LanguageLocale, targetLanguageVoiceItem.VoiceName, "Default");

            if (outputMessage is not null)
            {
                OutputMessages.Add(outputMessage);
            }
        }
    }
}
