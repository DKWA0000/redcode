using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;


[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] 
[ApiController]
[Route("api/[controller]")]
public class BookController : ControllerBase
{
    private readonly BookService service;

    public BookController(BookService service)
    {
        this.service = service;
    }

    [HttpGet]
    public async Task<IActionResult> getAllBooks()
    {
        List<BookResponseDto> books = await service.getAllBooks();
        return Ok(books);
    }

    [HttpPost]
    public async Task<IActionResult> addBook([FromBody] BookCreateRequestDto dto)
    {
        BookResponseDto newBook = await service.addBook(dto);
        return Ok(newBook);
    }

    [HttpPatch]
    public async Task<IActionResult> updateBook([FromBody] BookAlterRequestDto dto)
    {
        BookResponseDto updatedBook = await service.alterBook(dto);
        return Ok(updatedBook);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> deleteBook([FromRoute] int id)
    {
        BookResponseDto deletedBook = await service.deleteBook(id);
        return Ok(deletedBook); 
    }
}