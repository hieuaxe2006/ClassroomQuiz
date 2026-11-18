using UnityEngine;

namespace ClassroomQuiz.UI
{
    public sealed class GameplayUiReferences : MonoBehaviour
    {
        [SerializeField] private GameplayUiRefs references;
        public GameplayUiRefs References => references;
        public void SetReferences(GameplayUiRefs value) => references = value;
    }
}
