using UIKit;

namespace DevDays.iOS
{
    public class Application
    {
        private static iOSCompositionRoot _compositionRoot;

        // This is the main entry point of the application.
        static void Main(string[] args)
        {

            _compositionRoot = new iOSCompositionRoot();
            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.
            UIApplication.Main(args, null, "AppDelegate");
        }


        public static iOSCompositionRoot CompositionRoot => _compositionRoot;
    }
}