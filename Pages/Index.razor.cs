using Microsoft.AspNetCore.Components;
using LLM.Document.Response.Model;
using LLM.Document.Response.Services;
using Telerik.Blazor.Components;
using Telerik.SvgIcons;
using Aspose.Pdf.Operators;
using OpenAI.Chat;
using static LangChain.Chains.StackableChains.Agents.Tools.BuiltIn.Classes.GoogleResult;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using System.Runtime.ConstrainedExecution;
using UglyToad.PdfPig.Graphics.Operations.SpecialGraphicsState;

namespace LLM.Document.Response.Pages
{
    public class IndexBase : ComponentBase
    {
        [Inject] LangChainService LangChainService { get; set; }
        public bool IsBusy { get; set; }
        public string ErrorMessage { get; set; }
        public string Prompt { get; set; }
        public TelerikAIPrompt AIPromptRef { get; set; }
        public List<QuestionModel> QuestionList { get; set; } = new();
        public void SetPromptSuggestion(string suggestion)
        {
            Prompt = suggestion;
        }

        public List<string> PromptSuggestions { get; set; } = new List<string>()
        {
            "Qual a população do DF?",
            "Como faço para realizar o teste rápido de Covid-19?",
            "Onde posso ter atendimento com médico urologista?",
            "Estou com febre e dor de cabeça onde devo procurar o atendimento?",
            "Quais as unidades de saúde da Região Sudoeste?",
            "Quantos pacientes tem na lista de espera da especialidade Oftalmologia?",
            "Como posso fazer o cadastro na UBS?",
            "Quantos pacientes tem na lista de espera da especialidade de neuropediatria?",
            "Posso ter atendimento negado por falta de cadastro na atenção básica, me explica se isso é possível?",
            "Onde posso fazer a inserção de DIU no DF?",
            "Como funcionam os encaminhamentos para médicos especialistas no DF?",

        };

        public async Task HandleCustomPromptRequest()
        {
            try
            {
                ErrorMessage = string.Empty;
                IsBusy = true;

                if (string.IsNullOrWhiteSpace(Prompt))
                {
                    throw new ApplicationException("Favor informar a pergunta.");
                }

                QuestionModel question = new QuestionModel(Prompt);
                QuestionList.Insert(0, question);

                AnswerModel answerModel = await LangChainService.GetAnswer(question);
                question.AnswerModel = answerModel;
                AIPromptRef.AddOutput(answerModel.Answer, "", answerModel.Info, Prompt, null, true);
            }
            catch (Exception erro)
            {
                ErrorMessage = erro.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

    }
}
