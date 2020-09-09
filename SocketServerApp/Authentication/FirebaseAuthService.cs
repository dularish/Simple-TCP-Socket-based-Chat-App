using Firebase.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServerApp.Authentication
{
    public class FirebaseAuthService : IAuthenticationService
    {
        private FirebaseAuthProvider _firebaseAuthProvider;

        public FirebaseAuthService()
        {
            _firebaseAuthProvider =  new FirebaseAuthProvider(new FirebaseConfig(Environment.GetEnvironmentVariable("ChatServerFirebaseApiKey", EnvironmentVariableTarget.Machine)));
        }

        public string Name => "Firebase Authentication service";

        public bool SignIn(string userEmail, string password, out SignInErrorCode signInErrorCode)
        {
            bool retCode = true;
            try
            {
                FirebaseAuthLink firebaseAuthLink = _firebaseAuthProvider.SignInWithEmailAndPasswordAsync(userEmail, password).Result;

                signInErrorCode = SignInErrorCode.NoError;
            }
            catch (FirebaseAuthException ex)
            {
                signInErrorCode = SignInErrorCode.InvalidCredentials;
                retCode = false;
            }
            catch(Exception)
            {
                signInErrorCode = SignInErrorCode.UnknownError;
                retCode = false;
            }
            return retCode;

        }

        public bool SignUp(string userEmail, string password, out SignUpErrorCode signUpErrorCode)
        {
            bool retCode = true;
            try
            {
                FirebaseAuthLink firebaseAuthLink = _firebaseAuthProvider.CreateUserWithEmailAndPasswordAsync(userEmail, password).Result;

                signUpErrorCode = SignUpErrorCode.NoError;
            }
            catch (FirebaseAuthException)
            {
                signUpErrorCode = SignUpErrorCode.UserEmailAlreadyExists;
                retCode = false;
            }
            catch (Exception)
            {
                signUpErrorCode = SignUpErrorCode.UnknownError;
                retCode = false;
            }
            return retCode;
        }
    }
}
