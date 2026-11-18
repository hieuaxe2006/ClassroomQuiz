using UnityEngine;

namespace ClassroomQuiz.UI
{
    public sealed class LoginSceneUiReferences : MonoBehaviour
    {
        [SerializeField] private LoginSceneUiRefs references;
        public LoginSceneUiRefs References => references;
        public void SetReferences(LoginSceneUiRefs value) => references = value;
    }
}
