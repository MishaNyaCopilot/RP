using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

namespace Valuator.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IDatabase _db;
        private readonly IConnectionMultiplexer _redis;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
            _redis = ConnectionMultiplexer.Connect("localhost:6379"); // Подключение к Redis
            _db = _redis.GetDatabase();
        }

        public void OnGet() { }

        public IActionResult OnPost(string text)
        {
            _logger.LogDebug(text);
            string id = Guid.NewGuid().ToString();
            string textKey = "TEXT-" + id;
            _db.StringSet(textKey, text); // Сохранение текста в Redis по ключу textKey

            string rankKey = "RANK-" + id;
            double rank = CalculateRank(text);
            _db.StringSet(rankKey, rank); // Сохранение оценки rank в Redis по ключу rankKey

            string similarityKey = "SIMILARITY-" + id;
            double similarity = CalculateSimilarity(text);
            _db.StringSet(similarityKey, similarity); // Сохранение значения similarity в Redis по ключу similarityKey

            return Redirect($"summary?id={id}"); // Перенаправление на страницу summary с параметром id
        }

        private double CalculateRank(string text)
        {
            int nonAlphabeticCount = text.Count(c => !char.IsLetter(c));
            return (double)nonAlphabeticCount / text.Length; // Оценка содержания (доля неалфавитных символов в тексте)
        }

        private double CalculateSimilarity(string text)
        {
            var server = _redis.GetServer("localhost:6379");
            var keys = server.Keys(pattern: "TEXT-*"); // Получение всех ключей, соответствующих шаблону "TEXT-*"
            foreach (var key in keys)
            {
                if (_db.StringGet(key) == text)
                {
                    return 1; // Если текст совпадает с сохраненным текстом, возвращаем similarity = 1
                }
            }
            return 0; // Если совпадений нет, возвращаем similarity = 0
        }
    }
}
