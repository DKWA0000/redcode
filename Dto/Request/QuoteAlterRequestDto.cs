public class QuoteAlterRequestDto
{
    public int quoteId{ get; set; }
    public String? quote{ get; set; }

    public QuoteAlterRequestDto(int quoteId, String? quote)
    {
        this.quoteId = quoteId;
        this.quote = quote;
    }
}