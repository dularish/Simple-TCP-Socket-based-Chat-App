using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServerApp.Authentication
{
    public class SimplestAuthService : IAuthenticationService
    {
        private Dictionary<string, string> _creds = new Dictionary<string, string>();

        public string Name => "Simple to test service session based authentication service";

        public bool SignIn(string userEmail, string password, out SignInErrorCode signInErrorCode)
        {
            if(_creds.ContainsKey(userEmail) && _creds[userEmail] == password)
            {
                signInErrorCode = SignInErrorCode.NoError;
                return true;
            }
            else
            {
                signInErrorCode = SignInErrorCode.InvalidCredentials;
                return false;
            }
        }

        public bool SignUp(string userEmail, string password, out SignUpErrorCode signUpErrorCode)
        {
            if (_creds.ContainsKey(userEmail))
            {
                signUpErrorCode = SignUpErrorCode.UserEmailAlreadyExists;
                return false;
            }
            else
            {
                _creds[userEmail] = password;
                signUpErrorCode = SignUpErrorCode.NoError;
                return true;
            }
        }
    }
}
