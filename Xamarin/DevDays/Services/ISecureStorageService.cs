namespace DevDays.Services
{
    public interface ISecureStorageService
    {
        void SavePassword(string userId, string password);
        bool CheckPassword(string userId, string password);
    }
}