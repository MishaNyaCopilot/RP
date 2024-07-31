using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

namespace Valuator.Pages
{
    public class SummaryModel : PageModel
    {
        private readonly ILogger<SummaryModel> _logger;
        private readonly IDatabase _db;

        public SummaryModel(ILogger<SummaryModel> logger)
        {
            _logger = logger;
            var redis = ConnectionMultiplexer.Connect("localhost:6379"); // Подключение к Redis
            _db = redis.GetDatabase();
        }

        public double Rank { get; set; }
        public double Similarity { get; set; }

        public void OnGet(string id)
        {
            _logger.LogDebug(id);

            string rankKey = "RANK-" + id;
            Rank = (double)_db.StringGet(rankKey); // Инициализация свойства Rank значением из Redis по ключу rankKey

            string similarityKey = "SIMILARITY-" + id;
            Similarity = (double)_db.StringGet(similarityKey); // Инициализация свойства Similarity значением из Redis по ключу similarityKey
        }
    }
}
