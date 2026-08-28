using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] 
[ApiController]
[Route("api/[controller]")]
public class QuoteController : ControllerBase
{
    private readonly QuoteService service;

    public QuoteController(QuoteService service)
    {
        this.service = service;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> getAllQuotes([FromRoute] int id)
    {
        List<QuoteResponseDto> books = await service.getAllQuotes(id);
        return Ok(books);
    }

    [HttpPost]
    public async Task<IActionResult> addQuote([FromBody] QuoteCreateRequestDto dto)
    {
        QuoteResponseDto newBook = await service.createNewQuote(dto);
        return Ok(newBook);
    }

    [HttpPatch]
    public async Task<IActionResult> updateQuote([FromBody] QuoteAlterRequestDto dto)
    {
        QuoteResponseDto updatedBook = await service.AlterQuote(dto);
        return Ok(updatedBook);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> deleteQuote([FromRoute] int id)
    {
        QuoteResponseDto deletedBook = await service.deleteQuote(id);
        return Ok(deletedBook); 
    }
}