using BookStore.Services;
using Infrastructure.Entities.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IdentityController : Controller
    {
        private readonly IIdentityService _identityService;
        public IdentityController(IIdentityService identityService)
        {
            _identityService = identityService;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterUser register)
        {
            var result = await _identityService.Register(register);
            if (result.Succeeded)
            {
                return Ok();
            }
            else { return BadRequest(result.ToString()); }
        }

        [HttpPost("Login")]
        public async Task<ActionResult<string>> Login([FromBody] LoginUser login)
        {
            var result = await _identityService.Login(login);
            return result;
        }
    }
}
