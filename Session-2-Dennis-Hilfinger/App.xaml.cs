using Windows.Graphics.Display;

namespace Session_2_Dennis_Hilfinger
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new MyWindow(new AppShell());
        }

        
    }
}