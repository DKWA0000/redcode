using Microsoft.EntityFrameworkCore;

public class BookService{
    private readonly BookListContext context;

    public BookService(BookListContext context)
    {
        this.context = context;
    }

    public Task<List<BookResponseDto>> getAllBooks()
    {
        return context.books
            .Select(book => convertToBookDto(book))
            .ToListAsync();
    }

    public async Task<BookResponseDto> addBook(BookCreateRequestDto dto)
    {
        Book tmp = addBookToDb(dto);
        await context.SaveChangesAsync();
        return convertToBookDto(tmp);
    }

    public async Task<BookResponseDto> alterBook(BookAlterRequestDto dto)
    {
        Book updatedBook = await updateBook(dto);
        return convertToBookDto(updatedBook);
    }

    public async Task<BookResponseDto> deleteBook(int id)
    {
        Book? bookToDelete = await context.books.FindAsync(id);
        if(bookToDelete == null)
        {
            throw new BookNotFoundException("Cannot delete a book that does not exist");
        }
        context.books.Remove(bookToDelete);
        await context.SaveChangesAsync();
        return convertToBookDto(bookToDelete);
    }

    private BookResponseDto convertToBookDto(Book book)
    {
        return new BookResponseDto(
            book.Id,
            book.Title,
            book.Author,
            book.Date
        );
    }

    private Book addBookToDb(BookCreateRequestDto book)
    {
        Book tmp = new Book();
        tmp.Title = book.Title;
        tmp.Author = book.Author;
        tmp.Date = book.Date;
        return context.books.Add(tmp).Entity;
    }

    private async Task<Book> updateBook(BookAlterRequestDto dto)
    {
        Book? tmp = await context.books.FindAsync(dto.Id);
        if(tmp == null)
        {
            throw new BookNotFoundException("Cannot update a book that does not exist");
        }
        tmp.Title = dto.Title ?? tmp.Title;
        tmp.Author = dto.Author ?? tmp.Author;
        tmp.Date = dto.Date ?? tmp.Date;

        await context.SaveChangesAsync();
        return tmp;
    }
}