using System.Collections.Generic;

namespace ClassroomQuiz.Data
{
    public sealed class StudyMaterial
    {
        public int LessonId;
        public string Title;
        public string Concept;
        public string WorkedExample;
        public string Challenge;
    }

    public static class StudyMaterialCatalog
    {
        private sealed class Topic
        {
            public string Title;
            public string Concept;
            public string Example;
            public string Challenge;

            public Topic(string title, string concept, string example, string challenge)
            {
                Title = title;
                Concept = concept;
                Example = example;
                Challenge = challenge;
            }
        }

        private static readonly Topic[] Topics =
        {
            new Topic("Diện tích tam giác", "Diện tích bằng một nửa tích của đáy và chiều cao tương ứng. Đường cao phải vuông góc với đáy đã chọn.", "Ví dụ mới: đáy 14 cm, cao 9 cm. S = 14 × 9 ÷ 2 = 63 cm².", "Thử tìm chiều cao của tam giác có diện tích 84 cm² và đáy 14 cm. Gợi ý: đảo công thức h = 2S ÷ a."),
            new Topic("Tam giác và cạnh", "Tổng độ dài hai cạnh bất kỳ phải lớn hơn cạnh còn lại. Chu vi là tổng ba cạnh.", "Ba cạnh 7 cm, 8 cm, 11 cm tạo thành tam giác vì 7 + 8 > 11. Chu vi bằng 26 cm.", "Nếu hai cạnh là 5 cm và 9 cm, cạnh thứ ba phải nằm trong khoảng nào để tạo thành tam giác?"),
            new Topic("Hình chữ nhật và hình vuông", "Hình chữ nhật có S = dài × rộng; hình vuông có S = cạnh × cạnh. Chu vi hình chữ nhật là 2 × (dài + rộng).", "Mảnh vườn dài 18 m, rộng 7 m: diện tích 126 m², chu vi 50 m.", "Một hình chữ nhật có chu vi 54 m và dài 17 m. Hãy tìm chiều rộng và diện tích."),
            new Topic("Ghép hình", "Chia hình phức hợp thành các hình quen thuộc, tính từng phần rồi cộng hoặc trừ diện tích.", "Một hình chữ nhật 12 × 9 cm bị khoét góc vuông 3 × 4 cm: phần còn lại có diện tích 108 − 12 = 96 cm².", "Hãy tự chia một hình chữ L thành hai hình chữ nhật và so sánh hai cách chia khác nhau."),
            new Topic("Đổi đơn vị độ dài và diện tích", "Độ dài đổi theo bậc 10; diện tích đổi theo bậc 100 vì mỗi đơn vị có hai chiều.", "2,4 m = 240 cm; 2,4 m² = 24.000 cm².", "Đổi 0,35 ha sang m². Nhớ rằng 1 ha = 10.000 m²."),
            new Topic("Thứ tự thực hiện phép tính", "Làm trong ngoặc trước, rồi lũy thừa, nhân/chia, cuối cùng cộng/trừ; các phép cùng mức làm từ trái sang phải.", "72 − 3 × (8 + 4) = 72 − 36 = 36.", "Tính 150 ÷ 5 × 2 và giải thích vì sao không nhân 5 × 2 trước."),
            new Topic("Tính chia hết", "Một số chia hết cho 2 nếu chữ số cuối chẵn; cho 5 nếu chữ số cuối là 0 hoặc 5; cho 3 nếu tổng chữ số chia hết cho 3.", "4.572 chia hết cho 3 vì 4 + 5 + 7 + 2 = 18; số này cũng chia hết cho 2.", "Tìm chữ số x để 7x2 chia hết cho cả 3 và 5. Có thể có bao nhiêu đáp án?"),
            new Topic("Số nguyên tố và phân tích", "Số nguyên tố lớn hơn 1 chỉ có hai ước dương. Phân tích thành thừa số nguyên tố giúp tìm ước và bội.", "84 = 2² × 3 × 7.", "Phân tích 180 thành thừa số nguyên tố rồi tìm số mũ lớn nhất của 2 và 3."),
            new Topic("Ước chung lớn nhất", "ƯCLN là số lớn nhất chia hết các số đã cho. Có thể tìm bằng cách phân tích các số thành thừa số nguyên tố.", "36 = 2² × 3² và 60 = 2² × 3 × 5, nên ƯCLN(36,60) = 2² × 3 = 12.", "Chia 48 quyển vở và 72 cây bút thành nhiều phần quà giống nhau nhất. Mỗi phần có bao nhiêu món mỗi loại?"),
            new Topic("Bội chung nhỏ nhất", "BCNN là bội dương nhỏ nhất chung của các số. Dùng các lũy thừa lớn nhất trong phân tích nguyên tố.", "8 = 2³, 12 = 2² × 3 nên BCNN(8,12) = 2³ × 3 = 24.", "Hai đèn nháy mỗi 14 giây và 20 giây. Sau bao lâu chúng nháy cùng lúc lần tiếp theo?"),
            new Topic("Số nguyên trên trục số", "Số bên phải lớn hơn số bên trái. Số đối cách 0 cùng khoảng cách nhưng ở phía ngược lại.", "−6 < −2 vì −6 nằm bên trái −2; số đối của −9 là 9.", "Sắp xếp −12, 5, −3, 0, 8 theo thứ tự tăng dần."),
            new Topic("Cộng và trừ số nguyên", "Cộng hai số cùng dấu: cộng độ lớn và giữ dấu. Khác dấu: lấy độ lớn lớn trừ nhỏ và giữ dấu của số có độ lớn lớn hơn.", "−17 + 25 = 8; 13 − 21 = −8.", "Một thang máy ở tầng −4 đi lên 11 tầng rồi xuống 6 tầng. Thang máy dừng ở tầng nào?"),
            new Topic("Phân số tương đương", "Nhân hoặc chia cả tử và mẫu cho cùng một số khác 0 thì giá trị phân số không đổi.", "18/30 = 3/5 sau khi chia cả tử và mẫu cho 6.", "Rút gọn 42/63 rồi giải thích vì sao kết quả đã tối giản."),
            new Topic("Cộng trừ phân số", "Quy đồng mẫu trước khi cộng hoặc trừ; chỉ cộng/trừ tử số, giữ nguyên mẫu chung.", "5/12 + 7/18 = 15/36 + 14/36 = 29/36.", "Tính 7/10 − 1/4 bằng hai cách quy đồng khác nhau."),
            new Topic("Nhân và chia phân số", "Nhân: tử với tử, mẫu với mẫu. Chia cho một phân số là nhân với phân số nghịch đảo.", "3/8 ÷ 9/16 = 3/8 × 16/9 = 2/3.", "Một bình có 5/6 lít nước, rót mỗi cốc 1/12 lít. Rót đầy được bao nhiêu cốc?"),
            new Topic("Số thập phân", "Khi cộng trừ, đặt thẳng hàng dấu phẩy. Khi nhân, đếm tổng chữ số thập phân của hai thừa số.", "3,75 × 1,2 = 4,500 = 4,5.", "Một cuộn dây dài 12,5 m cắt thành đoạn 0,8 m. Cắt được nhiều nhất bao nhiêu đoạn nguyên vẹn?"),
            new Topic("Tỉ số và tỉ lệ", "Tỉ số so sánh hai đại lượng cùng loại. Trong bài toán tỉ lệ, giữ nguyên quan hệ giữa các đại lượng.", "Tỉ số 18 : 30 rút gọn thành 3 : 5.", "Nước ép pha theo tỉ lệ si-rô : nước = 2 : 7. Dùng 350 ml nước thì cần bao nhiêu ml si-rô?"),
            new Topic("Phần trăm", "x% nghĩa là x/100. Muốn tìm p% của một số, nhân số đó với p/100.", "15% của 240 = 0,15 × 240 = 36.", "Một món hàng 480.000 đồng giảm 12%. Tính số tiền phải trả và số tiền tiết kiệm."),
            new Topic("Biểu thức có biến", "Biến đại diện cho số chưa biết. Thay giá trị vào đúng vị trí, rồi làm theo thứ tự phép tính.", "Với P = 4x + 3, khi x = 7 thì P = 4 × 7 + 3 = 31.", "Viết biểu thức cho chu vi hình chữ nhật có chiều dài x + 4 và chiều rộng x."),
            new Topic("Giải phương trình đơn giản", "Giữ cân bằng hai vế: làm phép toán ngược ở cả hai vế để cô lập ẩn.", "3x + 5 = 26 ⇒ 3x = 21 ⇒ x = 7.", "Giải 5(y − 2) = 35 rồi thử thay kết quả vào phương trình ban đầu."),
            new Topic("Bảng số liệu và trung bình", "Trung bình cộng bằng tổng các giá trị chia cho số lượng. Luôn kiểm tra đơn vị và số phần tử.", "Điểm 6, 8, 9, 9 có trung bình (6 + 8 + 9 + 9) ÷ 4 = 8.", "Bốn ngày bán 12, 15, 9, 20 chiếc bánh. Tính trung bình mỗi ngày và khoảng biến thiên."),
        };

        public static List<StudyMaterial> Build(List<LessonDefinition> lessons)
        {
            var result = new List<StudyMaterial>(42);
            for (var lessonId = 1; lessonId <= 42; lessonId++)
            {
                var topicIndex = (lessonId - 1) % Topics.Length;
                var level = (lessonId - 1) / Topics.Length + 1;
                var topic = Topics[topicIndex];
                var lessonTitle = lessons != null && lessonId <= lessons.Count && lessons[lessonId - 1] != null
                    ? lessons[lessonId - 1].title
                    : $"Bài {lessonId}";
                var extra = level == 1
                    ? string.Empty
                    : "\n\nMở rộng: thử giải bằng một cách khác, kiểm tra ngược kết quả và nêu điều kiện để cách làm luôn đúng.";
                result.Add(new StudyMaterial
                {
                    LessonId = lessonId,
                    Title = $"{lessonId:00}. {lessonTitle} · Mở rộng: {topic.Title}",
                    Concept = topic.Concept,
                    WorkedExample = topic.Example,
                    Challenge = topic.Challenge + extra
                });
            }
            return result;
        }
    }
}
