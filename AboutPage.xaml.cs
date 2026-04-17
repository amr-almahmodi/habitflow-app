namespace HabitFlow;

public partial class AboutPage : ContentPage
{
    public AboutPage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

    }

    private void BackTapped(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }

    private void GoHome(object sender, EventArgs e)
    {
        Navigation.PopToRootAsync();
    }
    private async void OnMenuTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MenuListPage());
    }

}
