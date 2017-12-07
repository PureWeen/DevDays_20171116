using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Auth;
using System.Threading.Tasks;
using DevDays.Services;

namespace DevDays.iOS.Services
{
    public class SecureStorageService : ISecureStorageService
    {

        public void SavePassword(string userId, string password)
        {
            Set("DevDays", userId, "Password", password);
        }


        public bool CheckPassword(string userId, string password)
        {
            string current = Get("DevDays", userId, "Password");
            return current == password;
        }



        public string Get(string serviceName, string username, string key)
        {
            var myStore = MyAccountStore;
            if (myStore == null)
                return String.Empty;

            Xamarin.Auth.Account accounts = myStore.FindAccountsForService(serviceName)
                .Where(x => x.Username == username).FirstOrDefault();

            if (accounts == null || !accounts.Properties.ContainsKey(key))
                return String.Empty;

            return accounts.Properties[key];
        }


        public void Set(string serviceName, string userId, string key, string value)
        {
            var myStore = MyAccountStore;

            if (myStore == null)
                throw new InvalidOperationException();

            Xamarin.Auth.Account account = myStore.FindAccountsForService(serviceName)
                .Where(x => x.Username == userId)
                .FirstOrDefault();

            if (account == null)
            {
                account = new Xamarin.Auth.Account(userId);
            }

            account.Properties[key] = value;
            myStore.Save(account, serviceName);
        }




        AccountStore MyAccountStore
        {
            get
            {
                return AccountStore.Create();
            }
        }
    }
}