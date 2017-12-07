
using DevDays.iOS.Services;
using DevDays.Services;

namespace DevDays.iOS
{
    public class iOSCompositionRoot : CompositionRoot
    {
        protected override INavigationService CreateNavigationService()
        {
            return new iOSNavigationService();
        }

        protected override ISecureStorageService CreateSecureStorageService()
        {
            return new SecureStorageService();
        }
    }
}