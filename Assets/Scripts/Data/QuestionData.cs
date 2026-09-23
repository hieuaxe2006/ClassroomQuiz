// QuestionData.cs — CẢ 2 NGƯỜI DÙNG CHUNG
using System;

[Serializable]
public class QuestionData
{
    public int questionId;
    public string questionText;
    public string[] answers;        // 4 đáp án
    public int correctAnswerIndex;  // 0-3
    public string category;         // "Địa lý", "Lịch sử", ...
}

[Serializable]
public class QuestionBank
{
    public QuestionData[] questions;
}
