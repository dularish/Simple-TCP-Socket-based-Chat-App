using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServerApp.Authentication
{
    public interface IAuthenticationService
    {
        bool SignIn(string userEmail, string password, out SignInErrorCode signInErrorCode);

        bool SignUp(string userEmail, string password, out SignUpErrorCode signUpErrorCode);

        string Name { get; }
    }

    public enum SignInErrorCode { NoError, InvalidCredentials, UnknownError }

    public enum SignUpErrorCode { NoError, UserEmailAlreadyExists, UnknownError }
}
