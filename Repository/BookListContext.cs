using Microsoft.EntityFrameworkCore;
public class BookListContext : DbContext
{
    public BookListContext(DbContextOptions<BookListContext> options) : base(options)
    {
    }
    public DbSet<Person> persons{ get; set; } = null!;
    public DbSet<Book> books{ get; set; } = null!;
    public DbSet<Quote> quotes{ get; set; } = null!;
    public DbSet<RefreshToken> refreshTokens{ get; set; } = null!;
}