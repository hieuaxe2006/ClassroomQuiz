using UnityEngine;

namespace ClassroomQuiz.Gameplay
{
    public sealed class EndSceneUiReferences : MonoBehaviour
    {
        [SerializeField] private EndSceneUIController.EndUiRefs references;
        public EndSceneUIController.EndUiRefs References => references;
        public void SetReferences(EndSceneUIController.EndUiRefs value) => references = value;
    }
}
