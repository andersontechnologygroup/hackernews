using System.ComponentModel.DataAnnotations;

namespace HackerNews.Api.Models
{
    public record LoginRequest([Required] string Username, [Required] string Password);
}

