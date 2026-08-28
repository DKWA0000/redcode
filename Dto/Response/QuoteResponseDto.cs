public class QuoteResponseDto
{
    public int quoteId{ get; set; }
    public String quote{ get; set; }

    public QuoteResponseDto(int quoteId, String quote)
    {
        this.quoteId = quoteId;
        this.quote = quote;
    }
}