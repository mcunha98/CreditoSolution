using System.Text.Json.Serialization;


namespace Shared.Domain
{
    public class PagedResult<T>
    {
        [JsonPropertyName("pagina")]
        public int Pagina { get; set; }

        [JsonPropertyName("tamanho")]
        public int Tamanho { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("items")]
        public List<T> Items { get; set; } = new();
    }
}
