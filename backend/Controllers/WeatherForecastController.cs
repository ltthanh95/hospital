using backend.Models;
using backend.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild",
            "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        [HttpGet]
        public ActionResult<ApiResponse<IEnumerable<WeatherForecast>>> Get()
        {
            var forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            });

            return Ok(ApiResponse<IEnumerable<WeatherForecast>>.Success(forecasts));
        }

        [HttpGet("admin-only")]
        [Authorize(Roles = nameof(Role.ADMIN))]
        public ActionResult<ApiResponse> AdminOnly()
        {
            return Ok(ApiResponse.Success("You are an admin."));
        }
    }
}
