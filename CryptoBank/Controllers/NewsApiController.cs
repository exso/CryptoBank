using CryptoBank.Handlers.News.Queries;

using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CryptoBank.Controllers
{
    [ApiController]
    [Route("{api}/{controller}")]
    public class NewsApiController : ControllerBase 
    {
        private readonly IMediator _mediator;

        public NewsApiController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("{action}")]
        public async Task<IActionResult> Get()
        {
            var result = await _mediator.Send(new NewsList.Query());

            return new JsonResult(result);
        }
    }
}
