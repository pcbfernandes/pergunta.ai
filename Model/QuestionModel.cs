namespace LLM.Document.Response.Model
{
    public class QuestionModel
    {
        public string Question { get; internal set; }
        public DateTime CreationDate { get; internal set; }
        public AnswerModel AnswerModel { get; internal set; }
        public QuestionModel(string question)
        {
            Question = question;
            CreationDate = DateTime.Now;
        }
    }
}
