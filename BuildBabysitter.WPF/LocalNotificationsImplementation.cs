using Xamarin.Forms;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;

[assembly: Dependency(typeof(BuildBabysitter.WPF.LocalNotificationsImplementation))]
namespace BuildBabysitter.WPF
{
    public class LocalNotificationsImplementation : INotification
    {
        public void Show(string title, string body)
        {
            NotifyIcon notifyIcon;
            notifyIcon = new NotifyIcon();
            notifyIcon.Visible = true;
            notifyIcon.Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            notifyIcon.BalloonTipClosed += (s, e) => notifyIcon.Visible = false;

            notifyIcon.ShowBalloonTip(3000, title, body, ToolTipIcon.Info);
        }
    }
}
