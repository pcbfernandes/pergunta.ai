using Aspose.Pdf.Operators;
using LangChain.Databases;
using LangChain.Docstore;
using LangChain.Providers;
using LangChain.Providers.Azure;
using LangChain.Providers.OpenAI;
using LangChain.Sources;
using LangChain.TextSplitters;
using LangChain.VectorStores;
using Microsoft.Data.Sqlite;
using LLM.Document.Response.Model;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using LLM.Document.Response.Core;

namespace LLM.Document.Response.Services
{
    public class LangChainService
    {
        private IWebHostEnvironment _environment { get; set; }

        private AzureOpenAIModel ModelEmbedding;
        private AzureOpenAIModel ModelGpt;
        readonly string ENDPOINT;
        readonly string OPEN_AI_KEY;
        readonly string DEPLOYMENT_NAME_GPT;
        readonly string DEPLOYMENT_NAME_EMBEDDING;
        readonly string DBNAME;
        readonly string INSTRUCTION;

        public LangChainService(IWebHostEnvironment environment, AppConfig appConfig)
        {
            _environment = environment;
            ENDPOINT = appConfig.ENDPOINT;
            OPEN_AI_KEY = appConfig.OPEN_AI_KEY;
            DEPLOYMENT_NAME_GPT = appConfig.DEPLOYMENT_NAME_GPT;
            DEPLOYMENT_NAME_EMBEDDING = appConfig.DEPLOYMENT_NAME_EMBEDDING;
            DBNAME = appConfig.DBNAME;
            INSTRUCTION = appConfig.INSTRUCTION;

            ModelEmbedding = new AzureOpenAIModel(OPEN_AI_KEY, ENDPOINT, DEPLOYMENT_NAME_EMBEDDING);
            ModelGpt = new AzureOpenAIModel(OPEN_AI_KEY, ENDPOINT, DEPLOYMENT_NAME_GPT);
        }


        private IReadOnlyCollection<LangChain.Docstore.Document>? Documents;

        public string GetDatabasePath()
        {
            var variaveis = Environment.GetEnvironmentVariables();
            if (variaveis.Contains("SQLITE_LOCATION") && variaveis["SQLITE_LOCATION"] is string location)
            {
                return Path.Combine(location, DBNAME);
            }

            List<string> lista = new List<string>()
            {
                _environment.ContentRootPath,
                _environment.WebRootPath,
            };

            foreach (var item in lista)
            {
                if (File.Exists(Path.Combine(item, DBNAME)))
                {
                    return Path.Combine(item, DBNAME);
                }
            }

            throw new ApplicationException($"Db não localizado nos caminhos [{string.Join("], [", lista)}].");
        }
        public async Task<AnswerModel> GetAnswer(QuestionModel questionModel)
        {
            //A indexação dos arquivos não acontecerá neste momento. Na parte web o db já deve existir;
            //await CreateIndex();            

            string dbPath = GetDatabasePath();

            IReadOnlyCollection<LangChain.Docstore.Document>? similarDocuments = null;

            try
            {
                var db = GetDatabase();
                if (db == null)
                {
                    throw new ApplicationException("Database está nula.");
                }
                similarDocuments = await db.GetSimilarDocuments(questionModel.Question, amount: 3);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Erro ao tentar obter o db no caminho [{dbPath}]: {ex.Message}");
            }

            string context = $"""
                 {INSTRUCTION}
                 {similarDocuments.AsString()}

                 Pergunta: {questionModel.Question}
                 Resposta útil:
                 """;

            ChatResponse answer = await ModelGpt.GenerateAsync(context, CancellationToken.None).ConfigureAwait(true);

            AnswerModel answerModel = new AnswerModel($"{answer}", answer.Usage.InputTokens, answer.Usage.OutputTokens);

            return answerModel;
        }

        private SQLiteVectorStore GetDatabase()
        {
            string dbConnectionString = $"{GetDatabasePath()};Mode=ReadOnly";
            string dbMemoryConnectionString = ":memory:;Mode=Memory;Cache=Shared";

            using (SqliteConnection source = new SqliteConnection($"Data Source = {dbConnectionString}"))
            {
                using (SqliteConnection destination = new SqliteConnection($"Data Source = {dbMemoryConnectionString}"))
                {
                    source.Open();
                    destination.Open();
                    source.BackupDatabase(destination);

                    return new SQLiteVectorStore(dbMemoryConnectionString, "vectors", ModelEmbedding);
                }
            }
        }

        public List<FileModel> FileModels { get; private set; } = new();

        public class FileModel
        {
            public FileModel(string fullName, string name, IReadOnlyCollection<LangChain.Docstore.Document> documents)
            {
                FullName = fullName;
                Name = name;
                Documents = documents;
            }
            public string FullName { get; internal set; }
            public string Name { get; internal set; }
            public IReadOnlyCollection<LangChain.Docstore.Document> Documents { get; internal set; }
        }
    }
}
