public class QuoteCreateRequestDto
{
    public String quote{ get; set; }

    public QuoteCreateRequestDto(String quote)
    {
        this.quote = quote;
    }
}