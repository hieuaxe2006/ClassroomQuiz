using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ClassroomQuiz.Data
{
    public enum LessonStatus
    {
        Draft,
        Ready
    }

    [Serializable]
    public sealed class LessonQuestionData
    {
        public int lessonId;
        public string lessonTitle = string.Empty;
        public string questionId = string.Empty;
        public int sequenceIndex;
        public string prompt = string.Empty;
        public List<string> options = new List<string> { string.Empty, string.Empty, string.Empty, string.Empty };
        public int correctIndex;
        public string explanation = string.Empty;

        public bool IsValid(out string error)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                error = "Nội dung câu hỏi không được để trống.";
                return false;
            }

            if (options == null || options.Count != 4)
            {
                error = "Mỗi câu hỏi phải có đúng 4 đáp án.";
                return false;
            }

            for (var i = 0; i < options.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(options[i]))
                {
                    error = $"Đáp án {i + 1} không được để trống.";
                    return false;
                }
            }

            if (correctIndex < 0 || correctIndex > 3)
            {
                error = "Đáp án đúng phải nằm trong khoảng 0–3.";
                return false;
            }

            error = string.Empty;
            return true;
        }
    }

    [Serializable]
    public sealed class LessonDefinition
    {
        public int lessonId;
        public string title = string.Empty;
        public LessonStatus status;
        public bool isDemo;
        public string summary = string.Empty;
        public int defaultQuestionDurationSeconds = 15;
        public List<LessonQuestionData> questions = new List<LessonQuestionData>();

        public bool IsReadyForRoom => status == LessonStatus.Ready && ValidateQuestions(out _);

        public bool ValidateQuestions()
        {
            return ValidateQuestions(out _);
        }

        public bool ValidateQuestions(out string error)
        {
            if (questions == null || questions.Count < 2 || questions.Count > 5)
            {
                error = "Mỗi bài cần từ 2 đến 5 câu hỏi.";
                return false;
            }

            for (var i = 0; i < questions.Count; i++)
            {
                if (questions[i] == null)
                {
                    error = $"Câu hỏi {i + 1} không hợp lệ.";
                    return false;
                }

                if (!questions[i].IsValid(out var questionError))
                {
                    error = $"Câu hỏi {i + 1}: {questionError}";
                    return false;
                }
            }

            error = string.Empty;
            return true;
        }
    }

    [Serializable]
    public sealed class LessonRepositoryData
    {
        public int schemaVersion = 1;
        public List<LessonDefinition> lessons = new List<LessonDefinition>();
    }

    [Serializable]
    public sealed class QuizRoomPayload
    {
        public int schemaVersion = 1;
        public string lessonTitle = string.Empty;
        public string summary = string.Empty;
        public List<LessonQuestionData> questions = new List<LessonQuestionData>();
    }

    public interface IQuestionRepository
    {
        List<LessonDefinition> LoadLessons();
        LessonDefinition GetLesson(int lessonId);
        bool SaveLessons(List<LessonDefinition> lessons);
    }

    public sealed class JsonLessonRepository : IQuestionRepository
    {
        private readonly string _persistentPath;

        public JsonLessonRepository()
        {
            var baseDirectory = Path.Combine(Application.persistentDataPath, "ClassroomQuiz");
            Directory.CreateDirectory(baseDirectory);
            _persistentPath = Path.Combine(baseDirectory, "ClassroomQuizLessons.json");
            EnsurePersistentDefaultExists();
        }

        public List<LessonDefinition> LoadLessons()
        {
            if (!File.Exists(_persistentPath))
            {
                return BuildDefaultLessons();
            }

            try
            {
                var data = JsonUtility.FromJson<LessonRepositoryData>(File.ReadAllText(_persistentPath));
                if (data != null && IsCompleteLessonSet(data.lessons))
                {
                    if (MigrateDefaultDemoLesson(data.lessons))
                    {
                        SaveLessons(data.lessons);
                    }
                    return data.lessons;
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Không thể đọc dữ liệu bài học; dùng dữ liệu mặc định. {exception.Message}");
            }

            return BuildDefaultLessons();
        }

        private static bool MigrateDefaultDemoLesson(List<LessonDefinition> lessons)
        {
            var demoLesson = lessons.Find(lesson => lesson.lessonId == 1 && lesson.isDemo);
            if (demoLesson == null || demoLesson.questions == null || demoLesson.questions.Count != 3
                || demoLesson.questions.Exists(question => question == null))
            {
                return false;
            }

            var isOriginalDemo = demoLesson.questions[0].questionId == "demo-triangle-area-1"
                && demoLesson.questions[1].questionId == "demo-triangle-area-2"
                && demoLesson.questions[2].questionId == "demo-triangle-area-3";
            if (!isOriginalDemo)
            {
                return false;
            }

            demoLesson.questions = BuildDemoQuestions(demoLesson.lessonId, demoLesson.title);
            demoLesson.summary = "DEMO — 5 câu hỏi đi từ công thức đến nhận diện dữ kiện và vận dụng tính diện tích tam giác.";
            return true;
        }

        public LessonDefinition GetLesson(int lessonId)
        {
            var lessons = LoadLessons();
            foreach (var lesson in lessons)
            {
                if (lesson.lessonId == lessonId)
                {
                    return lesson;
                }
            }

            return null;
        }

        public bool SaveLessons(List<LessonDefinition> lessons)
        {
            if (!IsCompleteLessonSet(lessons))
            {
                return false;
            }

            try
            {
                var json = JsonUtility.ToJson(new LessonRepositoryData { lessons = lessons }, true);
                var temporaryPath = _persistentPath + ".tmp";
                File.WriteAllText(temporaryPath, json);
                if (File.Exists(_persistentPath))
                {
                    File.Copy(temporaryPath, _persistentPath, true);
                    File.Delete(temporaryPath);
                }
                else
                {
                    File.Move(temporaryPath, _persistentPath);
                }
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"Không thể lưu dữ liệu bài học: {exception.Message}");
                return false;
            }
        }

        public string PersistentDataPath => _persistentPath;

        public static List<LessonDefinition> BuildDefaultLessons()
        {
            var lessons = new List<LessonDefinition>(42);
            for (var i = 1; i <= 42; i++)
            {
                var lesson = new LessonDefinition
                {
                    lessonId = i,
                    title = $"Bài {i}",
                    status = LessonStatus.Draft,
                    isDemo = false,
                    summary = "Bài nháp — giáo viên cần thêm câu hỏi trước khi tạo phòng.",
                    defaultQuestionDurationSeconds = 15,
                    questions = new List<LessonQuestionData>()
                };

                if (i == 1)
                {
                    lesson.title = "Bài 1 — Diện tích tam giác";
                    lesson.status = LessonStatus.Ready;
                    lesson.isDemo = true;
                    lesson.summary = "DEMO — câu hỏi minh họa, giáo viên nên rà soát trước khi dùng chính thức.";
                    lesson.questions = BuildDemoQuestions(lesson.lessonId, lesson.title);
                }

                lessons.Add(lesson);
            }

            return lessons;
        }

        private void EnsurePersistentDefaultExists()
        {
            if (File.Exists(_persistentPath))
            {
                return;
            }

            var defaultData = new LessonRepositoryData { lessons = BuildDefaultLessons() };
            File.WriteAllText(_persistentPath, JsonUtility.ToJson(defaultData, true));
        }

        private static bool IsCompleteLessonSet(List<LessonDefinition> lessons)
        {
            if (lessons == null || lessons.Count != 42)
            {
                return false;
            }

            var seen = new HashSet<int>();
            foreach (var lesson in lessons)
            {
                if (lesson == null || lesson.lessonId < 1 || lesson.lessonId > 42 || !seen.Add(lesson.lessonId) || string.IsNullOrWhiteSpace(lesson.title))
                {
                    return false;
                }

                if (lesson.questions == null || lesson.questions.Count > 5)
                {
                    return false;
                }

                if (lesson.status == LessonStatus.Ready && !lesson.ValidateQuestions())
                {
                    return false;
                }
            }

            return seen.Count == 42;
        }

        private static List<LessonQuestionData> BuildDemoQuestions(int lessonId, string lessonTitle)
        {
            return new List<LessonQuestionData>
            {
                new LessonQuestionData
                {
                    lessonId = lessonId,
                    lessonTitle = lessonTitle,
                    questionId = "demo-triangle-area-1",
                    sequenceIndex = 1,
                    prompt = "Công thức tính diện tích của một tam giác bất kỳ là gì?",
                    options = new List<string> { "Đáy × chiều cao", "(Đáy × chiều cao) ÷ 2", "(Đáy + chiều cao) ÷ 2", "Đáy + chiều cao" },
                    correctIndex = 1,
                    explanation = "Ghép hai tam giác bằng nhau sẽ tạo thành một hình bình hành có diện tích đáy × chiều cao. Vì một tam giác bằng một nửa hình đó nên S = (đáy × chiều cao) ÷ 2."
                },
                new LessonQuestionData
                {
                    lessonId = lessonId,
                    lessonTitle = lessonTitle,
                    questionId = "demo-triangle-area-2",
                    sequenceIndex = 2,
                    prompt = "Tam giác vuông có hai cạnh góc vuông dài 6 cm và 8 cm. Có thể dùng cạnh nào làm đáy và chiều cao để tính diện tích?",
                    options = new List<string> { "6 cm và 8 cm", "6 cm và cạnh huyền", "8 cm và cạnh huyền", "Chỉ dùng cạnh huyền" },
                    correctIndex = 0,
                    explanation = "Hai cạnh góc vuông vuông góc với nhau nên một cạnh làm đáy thì cạnh còn lại là chiều cao tương ứng. Ở đây có thể chọn đáy 8 cm và chiều cao 6 cm (hoặc đổi lại)."
                },
                new LessonQuestionData
                {
                    lessonId = lessonId,
                    lessonTitle = lessonTitle,
                    questionId = "demo-triangle-area-3",
                    sequenceIndex = 3,
                    prompt = "Một tam giác có đáy 10 cm và chiều cao tương ứng 7 cm. Khi thay vào công thức, biểu thức nào đúng?",
                    options = new List<string> { "10 × 7", "(10 + 7) ÷ 2", "(10 × 7) ÷ 2", "10 + 7" },
                    correctIndex = 2,
                    explanation = "Đáy là 10 cm, chiều cao tương ứng là 7 cm. Công thức yêu cầu lấy tích đáy và chiều cao rồi chia 2: (10 × 7) ÷ 2."
                },
                new LessonQuestionData
                {
                    lessonId = lessonId,
                    lessonTitle = lessonTitle,
                    questionId = "demo-triangle-area-4",
                    sequenceIndex = 4,
                    prompt = "Tính diện tích tam giác có đáy 10 cm và chiều cao tương ứng 7 cm.",
                    options = new List<string> { "17 cm²", "35 cm²", "70 cm²", "140 cm²" },
                    correctIndex = 1,
                    explanation = "Tính theo từng bước: 10 × 7 = 70; 70 ÷ 2 = 35. Vậy diện tích là 35 cm²."
                },
                new LessonQuestionData
                {
                    lessonId = lessonId,
                    lessonTitle = lessonTitle,
                    questionId = "demo-triangle-area-5",
                    sequenceIndex = 5,
                    prompt = "Tam giác vuông có hai cạnh góc vuông dài 6 cm và 8 cm. Diện tích tam giác là bao nhiêu?",
                    options = new List<string> { "14 cm²", "24 cm²", "48 cm²", "28 cm²" },
                    correctIndex = 1,
                    explanation = "Hai cạnh góc vuông có thể làm đáy và chiều cao. S = (6 × 8) ÷ 2 = 48 ÷ 2 = 24 cm²."
                }
            };
        }
    }
}
