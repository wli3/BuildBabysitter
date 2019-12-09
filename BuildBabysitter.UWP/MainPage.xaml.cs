using Windows.Foundation;
using Windows.UI.ViewManagement;

namespace BuildBabysitter.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            LoadApplication(new BuildBabysitter.App());
        }
    }
}
