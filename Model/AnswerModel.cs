namespace LLM.Document.Response.Model
{
    public class AnswerModel
    {
        public string Answer { get; internal set; }
        public int InputTokens { get; internal set; }
        public int OutputTokens { get; internal set; }
        public DateTime CreationDate { get; internal set; }
        public AnswerModel(string answer, int inputTokens, int outputTokens)
        {
            Answer = answer;
            InputTokens = inputTokens;
            OutputTokens = outputTokens;
            CreationDate = DateTime.Now;
        }

        public string Info
        {
            get
            {

                return $"Input Tokens: {InputTokens};{Environment.NewLine}Output Tokens: {OutputTokens};";
            }
        }
    }
}
