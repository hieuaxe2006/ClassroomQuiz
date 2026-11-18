using UnityEngine;

namespace ClassroomQuiz.UI
{
    public sealed class StartSceneUiReferences : MonoBehaviour
    {
        [SerializeField] private StartSceneUiRefs references;
        public StartSceneUiRefs References => references;
        public void SetReferences(StartSceneUiRefs value) => references = value;
    }
}
