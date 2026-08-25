public class QuoteCreateRequestDto
{
    public int bookId{ get; set; }
    public String quote{ get; set; }

    public QuoteCreateRequestDto(int bookId, String quote)
    {
        this.bookId = bookId;
        this.quote = quote;
    }
}