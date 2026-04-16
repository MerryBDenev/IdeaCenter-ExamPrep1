using System.Text.Json.Serialization;


namespace ExamPrep1.Models
{
    internal class IdeaDTO
    {
        [JsonPropertyName("title")]

        public string? Title { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

    }
}

