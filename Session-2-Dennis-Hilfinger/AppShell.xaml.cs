namespace Session_2_Dennis_Hilfinger
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(EditFlightPage), typeof(EditFlightPage));
            Routing.RegisterRoute(nameof(ImportPage), typeof(ImportPage));
        }
    }
}
