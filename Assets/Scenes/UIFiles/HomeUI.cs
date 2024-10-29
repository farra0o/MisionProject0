using System.Collections;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;

using UnityEngine;
using UnityEngine.UIElements;

public class HomeUI : MonoBehaviour
{
    // Firebase
    [Header("Firebase")]
    public DependencyStatus status;
    public FirebaseAuth auth;
    public FirebaseUser user;

    // UI Toolkit elements
    private TextField mailLogin;
    private TextField passwordLogin;

    private TextField userName;
    private TextField mailRegistro;
    private TextField passwordRegistro;
    private TextField passwordConfirmationRegistro;

    // UI Panels for Login and Registration
    private VisualElement loginPanel;
    private VisualElement registroPanel;
    private VisualElement messagePannel;

    // Buttons
    private Button btnLogin;
    private Button btnRegister;
    private Button btnGoToRegistro;
    private Button btnGoToLogin;
    //
    private Button btncloseEmergentWindow;

    private TextElement mensaje; 

    private VisualElement root;

    private void Awake()
    {
        // Acceder a los elementos de UI Toolkit
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        mensaje= root.Q<TextElement>("mensaje");
        messagePannel = root.Q<VisualElement>("MessagePannel");
        btncloseEmergentWindow = root.Q<Button>("CloseWindow"); 

        // Asignar los elementos del UI Toolkit
        mailLogin = root.Q<TextField>("MailLogin");
        passwordLogin = root.Q<TextField>("PasswordLogin");

        userName = root.Q<TextField>("UserName");
        mailRegistro = root.Q<TextField>("MailRegistro");
        passwordRegistro = root.Q<TextField>("PasswordRegistro");
        passwordConfirmationRegistro = root.Q<TextField>("PasswordConfirmationRegistro");

        // Asignar los paneles de login y registro
        loginPanel = root.Q<VisualElement>("LoginPanel");
        registroPanel = root.Q<VisualElement>("RegistroPanel");

        // Asignar los botones
        btnLogin = root.Q<Button>("BtnLogin");
        btnRegister = root.Q<Button>("BtnRegister");
        btnGoToRegistro = root.Q<Button>("BtnGoToRegistro");
        btnGoToLogin = root.Q<Button>("BtnGoToLogin");


        // Asignar las acciones de los botones
        btnLogin.clicked += LoginBoton;
        btnRegister.clicked += RegisterBoton;
        btnGoToRegistro.clicked += ShowRegistroPanel;
        btnGoToLogin.clicked += ShowLoginPanel;
        btncloseEmergentWindow.clicked += CloseMessage;



        // Inicializar Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            status = task.Result;
            if (status == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("No se pudieron resolver las dependencias " + status);
            }
        });
    }

    private void InitializeFirebase()
    {
        Debug.Log("Setting FirebaseApp");
        auth = FirebaseAuth.DefaultInstance;
    }

    // Método para mostrar el panel de Login
    private void ShowLoginPanel()
    {
        loginPanel.style.display = DisplayStyle.Flex;
        registroPanel.style.display = DisplayStyle.None;
    }

    // Método para mostrar el panel de Registro
    private void ShowRegistroPanel()
    {
        registroPanel.style.display = DisplayStyle.Flex;
        loginPanel.style.display = DisplayStyle.None;
    }

    // Método para manejar el botón de Login
    public void LoginBoton()
    {
        StartCoroutine(Login(mailLogin.value, passwordLogin.value));
    }
    public void OpenMessage()
    {
        messagePannel.style.display = DisplayStyle.Flex;
    }
    public void CloseMessage()
    {
        messagePannel.style.display = DisplayStyle.None;
    }

    private IEnumerator Login(string _email, string _password)
    {
        Task<AuthResult> loginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            Debug.LogWarning($"Failed to login: {loginTask.Exception}");
            FirebaseException firebaseEx = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            mensaje.text = "Login Failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    mensaje.text = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    mensaje.text = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    mensaje.text = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    mensaje.text = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    mensaje.text = "Account does not exist";
                    break;
            }
            Debug.LogError(mensaje.text);
            OpenMessage();

        }
        else
        {
            user = loginTask.Result.User;
            mensaje.text = $"User signed in successfully: {user.DisplayName} ({user.Email})";
            Debug.Log(mensaje.text);
            OpenMessage();
        }
    }

    // Método para manejar el botón de Registro
    public void RegisterBoton()
    {
        Debug.Log(userName.value + "user name");
        StartCoroutine(Register(userName.value, mailRegistro.value, passwordRegistro.value));
        
    }

    private IEnumerator Register(string _username, string _email, string _password)
    {
        if (string.IsNullOrEmpty(_username))
        {
            mensaje.text = "Missing Username";
            Debug.LogError(mensaje.text);
            OpenMessage();
        }

        // Agregear comprobacion y aviso de como debe ser la contraseña 
        else if (passwordRegistro.value != passwordConfirmationRegistro.value)
        {
            mensaje.text = "Password Does Not Match!";
            Debug.LogError(mensaje.text);
            OpenMessage();
        }
        else
        {
            Task<AuthResult> registerTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            yield return new WaitUntil(() => registerTask.IsCompleted);

            if (registerTask.Exception != null)
            {
                Debug.LogWarning($"Failed to register: {registerTask.Exception}");
                FirebaseException firebaseEx = registerTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                mensaje.text = "Register Failed!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        mensaje.text = "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        mensaje.text = "Missing Password";
                        break;
                    case AuthError.WeakPassword:
                        mensaje.text = "Weak Password";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        mensaje.text = "Email Already In Use";
                        break;
                }
                OpenMessage();
                Debug.LogError(mensaje.text);
            }
            else
            {
                user = registerTask.Result.User;
                if (user != null)
                {
                    UserProfile profile = new UserProfile { DisplayName = _username };
                    Task profileTask = user.UpdateUserProfileAsync(profile);
                    yield return new WaitUntil(() => profileTask.IsCompleted);

                    if (profileTask.Exception != null)
                    {
                        Debug.LogWarning($"Failed to set username: {profileTask.Exception}");

                        Debug.LogError("Username Set Failed!");
                    }
                    else
                    {
                        UIManager.instance.LoginScreen();
                        Debug.Log("User registered and username set successfully.");
                        mensaje.text = "Registrado usuario: " + userName;
                        OpenMessage();
                    }
                }
            }
        }
    }

 

}
