using Android.App;
using Android.Runtime;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Alcedo
{
#if DEBUG
    [Application(UsesCleartextTraffic = true)]
#else
    [Application]
#endif
    public class MainApplication : MauiApplication
    {
#pragma warning disable IDE0290 // Use primary constructor
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
