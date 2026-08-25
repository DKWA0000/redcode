public class QuoteResponseDto
{
    public int bookId{ get; set; }
    public String quote{ get; set; }

    public QuoteResponseDto(int bookId, String quote)
    {
        this.bookId = bookId;
        this.quote = quote;
    }
}