using Firebase.Auth.Providers;
using Firebase.Auth.Repository;
using Firebase.Auth;
using Firebase.Storage;
using FireSharp.Interfaces;
using FireSharp;

using System;
using System.Threading.Tasks;

namespace SRLS_launcher
{
    public class InitConfigsSys
    {
        static FirebaseAuthClient client;
        static FireSharp.FirebaseClient firebaseClient;
        static UserCredential userCredential;
        static FirebaseStorage storage;
        public InitConfigsSys()
        {
            InitConfigs();
        }
        public FireSharp.FirebaseClient GetFirebaseClient()
        {
            return firebaseClient;
        }
        public UserCredential GetUserCredential()
        {
            return userCredential;
        }
        public FirebaseStorage GetStorage()
        {
            return storage;
        }
        private void InitConfigs()
        {
            FirebaseAuthConfig config = new FirebaseAuthConfig
            {
                ApiKey = "AIzaSyCgXCL5GHSGmVJgDC1SDooeDubU3yCFz6I",
                AuthDomain = "srls-launcher.firebaseapp.com",
                Providers = new FirebaseAuthProvider[]
                {
                    new EmailProvider()
                },
                UserRepository = new FileUserRepository("test")
            };
            client = new FirebaseAuthClient(config);
        }
        private void InitConfigFirebase()
        {
            string _authSecret = null;

            if (userCredential != null && userCredential.User != null && userCredential.User.Credential != null)
            {
                _authSecret = userCredential.User.Credential.IdToken;
            }

            IFirebaseConfig firebaseConfig = new FireSharp.Config.FirebaseConfig
            {
                RequestTimeout = TimeSpan.FromDays(1),
                BasePath = "https://srls-launcher-default-rtdb.firebaseio.com",
                AuthSecret = _authSecret
            };
            firebaseClient = new FirebaseClient(firebaseConfig);
        }
        private void InitConfigFirebaseStorage()
        {
            storage = new FirebaseStorage("srls-launcher.appspot.com", new FirebaseStorageOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(userCredential.User.Credential.IdToken)
            });
        }
        public async Task<bool> Login(string username, string password)
        {
            try
            {
                userCredential = await client.SignInWithEmailAndPasswordAsync(username, password);
                InitConfigFirebaseStorage();
                InitConfigFirebase();

                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> Create(string username, string password)
        {
            try
            {
                userCredential = await client.CreateUserWithEmailAndPasswordAsync(username, password);
                InitConfigFirebaseStorage();
                InitConfigFirebase();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}