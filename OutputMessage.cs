using CommunityToolkit.Mvvm.ComponentModel;

namespace IKSTranslateApp;

internal partial class OutputMessage : ObservableObject
{
    [ObservableProperty] private OutputMessageType type;
    [ObservableProperty] private string text;
}
