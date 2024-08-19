using Firebase;
using Firebase.Auth;
using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;

public class AuthFireBaseManager : MonoBehaviour
{
    // FireBase 
    [Header("FireBase")]
    public DependencyStatus status;
    public FirebaseAuth auth;
    public FirebaseUser user;

    [Header("Login")]
    public TMP_InputField mailLogin;
    public TMP_InputField PasswordLogin;

    public TMP_Text textLogin;
 

    [Header("Registro")]
    public TMP_InputField UserName;
    public TMP_InputField mailRegistro;
    public TMP_InputField passwordRegistro;
    public TMP_InputField passwordConfirmationRegistro;
    public TMP_Text ErrorRegistro;

    private void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            status = task.Result;
            if (status == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.Log("No se pudieron resolver las dependencias " + status);
            }
        });
        
    }

    private void InitializeFirebase()
    {
        Debug.Log("setting FireBaseApp");
        auth = FirebaseAuth.DefaultInstance;
    }

    public void LoginBoton()
    {
        StartCoroutine(Login( mailLogin.text , PasswordLogin.text));
    }

    private IEnumerator Login(string _email, string _password)
    {
        //Call the Firebase auth signin function passing the email and password
        Task<AuthResult> LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not exist";
                    break;
            }
            textLogin.text = message;
        }
        else
        {
            //User is now logged in
            //Now get the result
            user = LoginTask.Result.User;
            Debug.LogFormat("User signed in successfully: {0} ({1})", user.DisplayName, user.Email);
            textLogin.text = "";
            textLogin.text = "Logged In";
        }
    }

    public void RegisterBoton()
    {
        StartCoroutine(Register(UserName.text, mailRegistro.text, passwordRegistro.text));
    }

    private IEnumerator Register(string _username , string  _email, string _password )
    {
        if (_username == "")
        {
            //If the username field is blank show a warning
            ErrorRegistro.text = "Missing Username";
        }
        else if (passwordRegistro.text != passwordConfirmationRegistro.text)
        {
            //If the password does not match show a warning
            ErrorRegistro.text = "Password Does Not Match!";
        }
        else
        {
            //Call the Firebase auth signin function passing the email and password
            Task<AuthResult> RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            //Wait until the task completes
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                //If there are errors handle them
                Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Register Failed!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WeakPassword:
                        message = "Weak Password";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Email Already In Use";
                        break;
                }
                ErrorRegistro.text = message;
            }
            else
            {
                //User has now been created
                //Now get the result
                user = RegisterTask.Result.User;

                if (user != null)
                {
                    //Create a user profile and set the username
                    UserProfile profile = new UserProfile { DisplayName = _username };

                    //Call the Firebase auth update user profile function passing the profile with the username
                    Task ProfileTask = user.UpdateUserProfileAsync(profile);
                    //Wait until the task completes
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        //If there are errors handle them
                        Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                        FirebaseException firebaseEx = ProfileTask.Exception.GetBaseException() as FirebaseException;
                        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                        ErrorRegistro.text = "Username Set Failed!";
                    }
                    else
                    {
                        //Username is now set
                        //Now return to login screen
                        UIManager.instance.LoginScreen();
                        ErrorRegistro.text = "";
                    }
                }
            }
        }
    }
}

