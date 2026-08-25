using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PersonController : ControllerBase
{
    private readonly PersonService service;

    public PersonController(PersonService service)
    {
        this.service = service;
    }

    [HttpPost("createuser")]
    public async Task<IActionResult> createUser([FromBody] CreateUserDto dto)
    {
        await service.createUser(dto);
        return Ok();
    }

    [HttpPost("loginuser")]
    public async Task<IActionResult> loginUser([FromBody] LoginUserDto dto)
    {
        service.loginUser(dto);
        return Ok();
    }

    [HttpPost("refreshtoken")]
    public async Task<IActionResult> refreshToken()
    {
        await service.refreshTokens();
        return Ok();
    }
}