using HabitFlow.Data;

namespace HabitFlow;

public partial class App : Application
{
    // Global database instance
    public static HabitDatabase Database { get; private set; }

    public App()
    {
        InitializeComponent();

        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "habits.db3");
        Database = new HabitDatabase(dbPath);

        MainPage = new NavigationPage(new MainPage());
    }
}
