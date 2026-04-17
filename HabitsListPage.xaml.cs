namespace HabitFlow;

public partial class HabitsListPage : ContentPage
{
    public HabitsListPage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnHomeTapped(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }

    private async void OnMenuTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MenuListPage());
    }

    private async void OnAboutTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AboutPage());
    }

    // ?? When user taps any habit
    private async void OnHabitTapped(object sender, TappedEventArgs e)
    {
        var habitName = e.Parameter as string ?? string.Empty;
        await Navigation.PushAsync(new AddEditHabitPage(habitName));
    }
}
