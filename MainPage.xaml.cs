namespace IKSTranslateApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        stlMain.SizeChanged += (s, e) =>
        {
            if (s is StackLayout stl)
            {
                // Scroll to end
                Util.ScrollToEnd(scvMain, stl);
            }
        };
    }

    private static async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
    {
        if (sender is Label label)
        {
            // Copy text to clipboard
            await Util.CopyToClipboard(label.Text);
        }
    }

    private void BtnClose_Clicked(object sender, EventArgs e)
    {
        Application.Current.Quit();
    }
}