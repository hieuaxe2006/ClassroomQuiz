using System;
using System.Net.Mail;
using ClassroomQuiz.Network;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ClassroomQuiz.UI
{
    [Serializable]
    public sealed class LoginSceneUiRefs
    {
        public Canvas Canvas;
        public GameObject LoginPanel;
        public GameObject RegisterPanel;
        public TMP_InputField ApiUrlInput;
        public TMP_InputField LoginEmail;
        public TMP_InputField LoginPassword;
        public TMP_InputField RegisterEmail;
        public TMP_InputField RegisterDisplayName;
        public TMP_InputField RegisterPassword;
        public TMP_InputField ConfirmPassword;
        public TMP_Text Status;
        public Button LoginButton;
        public Button RegisterButton;
        public Button RegisterTabButton;
        public Button BackToLoginButton;
    }

    public sealed class LoginSceneUIController : MonoBehaviour
    {
        private GameObject _loginPanel;
        private GameObject _registerPanel;
        private TMP_InputField _apiUrlInput;
        private TMP_InputField _loginEmail;
        private TMP_InputField _loginPassword;
        private TMP_InputField _registerEmail;
        private TMP_InputField _registerDisplayName;
        private TMP_InputField _registerPassword;
        private TMP_InputField _confirmPassword;
        private TMP_Text _status;
        private Button _loginButton;
        private Button _registerButton;
        private Button _registerTabButton;
        private Button _backToLoginButton;

        private void Awake()
        {
            var authoredUi = UnityEngine.Object.FindFirstObjectByType<LoginSceneUiReferences>(FindObjectsInactive.Include);
            if (authoredUi != null && authoredUi.References != null)
            {
                ApplyReferences(authoredUi.References);
            }
            else
            {
                BuildInterface();
            }
        }

        private void Start()
        {
            _loginButton.onClick.AddListener(OnLoginPressed);
            _registerButton.onClick.AddListener(OnRegisterPressed);
            _registerTabButton.onClick.AddListener(SetRegisterPanel);
            _backToLoginButton.onClick.AddListener(SetLoginPanel);
            SetLoginPanel();
            SetStatus("Đăng nhập bằng email và mật khẩu ClassroomQuiz của bạn.", RuntimeUiFactory.Muted);
        }

        public void BuildInterface()
        {
            var canvas = RuntimeUiFactory.CreateCanvas("ClassroomQuiz Login Canvas");
            var background = RuntimeUiFactory.Panel(canvas.transform, "Background", Vector2.zero, Vector2.zero, RuntimeUiFactory.Background);
            background.rectTransform.anchorMin = Vector2.zero;
            background.rectTransform.anchorMax = Vector2.one;
            background.rectTransform.sizeDelta = Vector2.zero;

            var card = RuntimeUiFactory.Panel(background.transform, "Login Card", new Vector2(840, 980), Vector2.zero, RuntimeUiFactory.Surface);
            RuntimeUiFactory.Label(card.transform, "Brand", "CLASSROOMQUIZ", new Vector2(740, 46), new Vector2(0, 424), 25, RuntimeUiFactory.Cyan, TextAlignmentOptions.Center, true);
            RuntimeUiFactory.Label(card.transform, "Title", "Chào mừng bạn đến lớp học", new Vector2(760, 64), new Vector2(0, 358), 38, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);
            RuntimeUiFactory.Label(card.transform, "Subtitle", "Tạo tài khoản để lưu tiến trình và kết quả học tập.", new Vector2(760, 40), new Vector2(0, 310), 20, RuntimeUiFactory.Muted, TextAlignmentOptions.Center);
            RuntimeUiFactory.Label(card.transform, "Api URL Label", "Địa chỉ API XAMPP", new Vector2(620, 28), new Vector2(0, 278), 16, RuntimeUiFactory.Muted, TextAlignmentOptions.MidlineLeft);
            _apiUrlInput = RuntimeUiFactory.Input(card.transform, "API URL", "http://127.0.0.1/ClassroomQuizApi/api.php", new Vector2(620, 54), new Vector2(0, 244));
            _apiUrlInput.text = ClassroomQuizPreferences.GetString(PhpApiClient.ApiUrlPreferenceKey, PhpApiClient.DefaultUrl);

            _loginPanel = new GameObject("Login Panel", typeof(RectTransform));
            _loginPanel.transform.SetParent(card.transform, false);
            _loginPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(680, 520);
            _loginPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -44);
            _loginEmail = RuntimeUiFactory.Input(_loginPanel.transform, "Login Email", "Gmail / email", new Vector2(620, 62), new Vector2(0, 166), TMP_InputField.ContentType.EmailAddress);
            _loginEmail.text = ClassroomQuizPreferences.GetString(PhpApiClient.AccountEmailPreferenceKey, string.Empty);
            _loginPassword = RuntimeUiFactory.Input(_loginPanel.transform, "Login Password", "Mật khẩu ClassroomQuiz", new Vector2(620, 62), new Vector2(0, 82), TMP_InputField.ContentType.Password);
            _loginButton = RuntimeUiFactory.Button(_loginPanel.transform, "Login Button", "Đăng nhập", new Vector2(620, 66), new Vector2(0, -8), RuntimeUiFactory.Blue, null);
            _registerTabButton = RuntimeUiFactory.Button(_loginPanel.transform, "Register Tab", "Tạo tài khoản", new Vector2(300, 58), new Vector2(-160, -94), RuntimeUiFactory.Purple, null, fontSize: 21);
            RuntimeUiFactory.Label(_loginPanel.transform, "Login Help", "Tài khoản cần được tạo trên XAMPP.", new Vector2(300, 48), new Vector2(160, -94), 17, RuntimeUiFactory.Muted, TextAlignmentOptions.Center);
            RuntimeUiFactory.Label(_loginPanel.transform, "Login Hint", "Mật khẩu được xác thực bởi máy chủ XAMPP.", new Vector2(620, 34), new Vector2(0, -160), 16, RuntimeUiFactory.Muted, TextAlignmentOptions.Center);

            _registerPanel = new GameObject("Register Panel", typeof(RectTransform));
            _registerPanel.transform.SetParent(card.transform, false);
            _registerPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(680, 570);
            _registerPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -42);
            _registerEmail = RuntimeUiFactory.Input(_registerPanel.transform, "Register Email", "Gmail / email", new Vector2(620, 56), new Vector2(0, 190), TMP_InputField.ContentType.EmailAddress);
            _registerDisplayName = RuntimeUiFactory.Input(_registerPanel.transform, "Register Display Name", "Tên hiển thị trong lớp", new Vector2(620, 56), new Vector2(0, 114));
            _registerPassword = RuntimeUiFactory.Input(_registerPanel.transform, "Register Password", "Mật khẩu (ít nhất 8 ký tự)", new Vector2(620, 56), new Vector2(0, 38), TMP_InputField.ContentType.Password);
            _confirmPassword = RuntimeUiFactory.Input(_registerPanel.transform, "Confirm Password", "Nhập lại mật khẩu", new Vector2(620, 56), new Vector2(0, -38), TMP_InputField.ContentType.Password);
            _registerButton = RuntimeUiFactory.Button(_registerPanel.transform, "Register Button", "Đăng ký", new Vector2(620, 62), new Vector2(0, -116), RuntimeUiFactory.Blue, null);
            _backToLoginButton = RuntimeUiFactory.Button(_registerPanel.transform, "Back To Login", "← Đăng nhập", new Vector2(210, 48), new Vector2(-220, 258), RuntimeUiFactory.Navy, null, fontSize: 18);

            var statusPlate = RuntimeUiFactory.Panel(card.transform, "Status Plate", new Vector2(720, 72), new Vector2(0, -430), RuntimeUiFactory.Hex("EAF3FA"));
            _status = RuntimeUiFactory.Text(statusPlate.transform, "Status", string.Empty, 18, RuntimeUiFactory.Muted, TextAlignmentOptions.Center);
            _status.rectTransform.offsetMin = new Vector2(16, 4);
            _status.rectTransform.offsetMax = new Vector2(-16, -4);

            canvas.gameObject.AddComponent<LoginSceneUiReferences>().SetReferences(new LoginSceneUiRefs
            {
                Canvas = canvas,
                LoginPanel = _loginPanel,
                RegisterPanel = _registerPanel,
                ApiUrlInput = _apiUrlInput,
                LoginEmail = _loginEmail,
                LoginPassword = _loginPassword,
                RegisterEmail = _registerEmail,
                RegisterDisplayName = _registerDisplayName,
                RegisterPassword = _registerPassword,
                ConfirmPassword = _confirmPassword,
                Status = _status,
                LoginButton = _loginButton,
                RegisterButton = _registerButton,
                RegisterTabButton = _registerTabButton,
                BackToLoginButton = _backToLoginButton
            });
            _loginPanel.SetActive(true);
            _registerPanel.SetActive(false);
        }

        private void ApplyReferences(LoginSceneUiRefs references)
        {
            _loginPanel = references.LoginPanel;
            _registerPanel = references.RegisterPanel;
            _apiUrlInput = references.ApiUrlInput;
            _loginEmail = references.LoginEmail;
            _loginPassword = references.LoginPassword;
            _registerEmail = references.RegisterEmail;
            _registerDisplayName = references.RegisterDisplayName;
            _registerPassword = references.RegisterPassword;
            _confirmPassword = references.ConfirmPassword;
            _status = references.Status;
            _loginButton = references.LoginButton;
            _registerButton = references.RegisterButton;
            _registerTabButton = references.RegisterTabButton;
            _backToLoginButton = references.BackToLoginButton;
        }

        private void SetLoginPanel()
        {
            RuntimeUiFactory.SetVisible(_loginPanel, true);
            RuntimeUiFactory.SetVisible(_registerPanel, false);
            SetStatus("Đăng nhập bằng email và mật khẩu ClassroomQuiz của bạn.", RuntimeUiFactory.Muted);
        }

        private void SetRegisterPanel()
        {
            RuntimeUiFactory.SetVisible(_loginPanel, false);
            RuntimeUiFactory.SetVisible(_registerPanel, true);
            _registerEmail.text = _loginEmail.text;
            SetStatus("Dùng email, tên hiển thị và mật khẩu riêng cho ClassroomQuiz.", RuntimeUiFactory.Muted);
        }

        private void OnLoginPressed()
        {
            var email = NormalizeAndValidateEmail(_loginEmail.text);
            if (email == null || string.IsNullOrEmpty(_loginPassword.text))
            {
                SetStatus("Nhập email hợp lệ và mật khẩu của tài khoản.", RuntimeUiFactory.Orange);
                return;
            }
            if (!TryGetApiUrl(out var apiUrl)) return;

            SetBusy(true, "Đang đăng nhập…");
            var request = new ClassroomQuizApiRequest
            {
                action = "login",
                email = email,
                password = _loginPassword.text
            };
            StartCoroutine(PhpApiClient.Post(apiUrl, request, null, HandleAuthenticationResponse));
        }

        private void OnRegisterPressed()
        {
            var email = NormalizeAndValidateEmail(_registerEmail.text);
            var displayName = _registerDisplayName.text.Trim();
            if (email == null)
            {
                SetStatus("Nhập Gmail/email hợp lệ.", RuntimeUiFactory.Orange);
                return;
            }
            if (string.IsNullOrWhiteSpace(displayName) || displayName.Length > 48)
            {
                SetStatus("Tên hiển thị cần từ 1 đến 48 ký tự.", RuntimeUiFactory.Orange);
                return;
            }
            if (_registerPassword.text.Length < 8)
            {
                SetStatus("Mật khẩu cần có ít nhất 8 ký tự.", RuntimeUiFactory.Orange);
                return;
            }
            if (_registerPassword.text != _confirmPassword.text)
            {
                SetStatus("Mật khẩu nhập lại chưa khớp.", RuntimeUiFactory.Orange);
                return;
            }
            if (!TryGetApiUrl(out var apiUrl)) return;

            SetBusy(true, "Đang tạo tài khoản…");
            var request = new ClassroomQuizApiRequest
            {
                action = "register",
                email = email,
                displayName = displayName,
                password = _registerPassword.text
            };
            StartCoroutine(PhpApiClient.Post(apiUrl, request, null, HandleAuthenticationResponse));
        }

        private void HandleAuthenticationResponse(ClassroomQuizApiResponse response)
        {
            SetBusy(false, string.Empty);
            if (response == null || !response.success || response.accountId <= 0 || string.IsNullOrWhiteSpace(response.token))
            {
                SetStatus(response == null || string.IsNullOrWhiteSpace(response.error) ? "Không nhận được phản hồi xác thực." : response.error, RuntimeUiFactory.Red);
                return;
            }

            ClassroomQuizPreferences.SetString(PhpApiClient.AccountIdPreferenceKey, response.accountId.ToString());
            ClassroomQuizPreferences.SetString(PhpApiClient.AccountTokenPreferenceKey, response.token);
            ClassroomQuizPreferences.SetString(PhpApiClient.AccountEmailPreferenceKey, response.email);
            ClassroomQuizPreferences.SetString("ClassroomQuiz.DisplayName", response.displayName);
            ClassroomQuizPreferences.SetString("ClassroomQuiz.Avatar", string.IsNullOrWhiteSpace(response.avatarKey) ? "A" : response.avatarKey);
            ClassroomQuizPreferences.SetString(PhpApiClient.ApiUrlPreferenceKey, _apiUrlInput.text.Trim());
            ClassroomQuizPreferences.Save();
            SceneManager.LoadScene("StartScene");
        }

        private bool TryGetApiUrl(out string apiUrl)
        {
            apiUrl = _apiUrlInput.text.Trim();
            if (!Uri.TryCreate(apiUrl, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                SetStatus("Địa chỉ API cần bắt đầu bằng http:// hoặc https://.", RuntimeUiFactory.Orange);
                return false;
            }
            return true;
        }

        private static string NormalizeAndValidateEmail(string value)
        {
            var email = (value ?? string.Empty).Trim().ToLowerInvariant();
            if (email.Length == 0 || email.Length > 254) return null;
            try
            {
                return new MailAddress(email).Address == email ? email : null;
            }
            catch (FormatException)
            {
                return null;
            }
        }

        private void SetBusy(bool busy, string message)
        {
            _loginButton.interactable = !busy;
            _registerButton.interactable = !busy;
            if (!string.IsNullOrEmpty(message)) SetStatus(message, RuntimeUiFactory.Muted);
        }

        private void SetStatus(string message, Color color)
        {
            if (_status != null)
            {
                _status.text = message;
                _status.color = color;
            }
        }
    }
}
