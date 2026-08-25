using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public class QuoteService
{
    private readonly BookListContext context;
    private readonly JwtUtil util;
    private readonly IHttpContextAccessor httpContext;

    public QuoteService(BookListContext context, IHttpContextAccessor httpContextAccessor)
    {
        this.context = context;
        this.util = new JwtUtil();
        this.httpContext = httpContextAccessor;
    }

    public async Task<List<QuoteResponseDto>> getAllQuotes(int bookId)
    {
        int personId = getPersonId();
        List<Quote> quotes = await context.quotes.Where(quote => (quote.bookId == bookId) && quote.personId == personId) 
                    .ToListAsync();

        return quotes.Select(quote => convertFromQuote(quote)).ToList();            
    }

    public async Task<QuoteResponseDto> createNewQuote(QuoteCreateRequestDto dto)
    {
        int personId = getPersonId();
        if((await getAllQuotes(dto.bookId)).Count < 5)
        {
            Quote quoteToAdd = addQuoteToDb(dto, personId);
            await context.SaveChangesAsync();
            return convertFromQuote(quoteToAdd);
        }
        throw new ToManyQuotesException("Quotes limit has been reached, cannot add another quote");
    }

    public async Task<QuoteResponseDto> AlterQuote(QuoteAlterRequestDto dto)
    {
        Quote alteredquote = await updateQuote(dto, getPersonId());
        return convertFromQuote(alteredquote); 
    }

    public async Task<QuoteResponseDto> deleteQuote(int quoteId)
    {
        Quote deletedQuote = await removeQuoteFromDb(quoteId, getPersonId());
        return convertFromQuote(deletedQuote); 
    }

    private QuoteResponseDto convertFromQuote(Quote quote)
    {
        return new QuoteResponseDto(
            quote.bookId,
            quote.quote
        );
    }

    private int getPersonId()
    {
        
        HttpContext currentContext = httpContext.HttpContext;

        if(currentContext != null)
        {
            String? accessToken = currentContext.Request.Cookies["Access-token"];

            if (!string.IsNullOrEmpty(accessToken))
            {
                ClaimsPrincipal? principal = util.getClaimsFromToken(accessToken);

                if (principal != null)
                {
                    string? personIdStr = principal.FindFirstValue(ClaimTypes.NameIdentifier);

                    if (!string.IsNullOrEmpty(personIdStr) && int.TryParse(personIdStr, out int personId))
                    {
                        return personId;
                    }
                    throw new PersonNotFoundException("No corresponding user found, cannot continue");
                }
            }
            throw new NoValidTokenException("No valid token found");
        }
        return -1;
    }

    private Quote addQuoteToDb(QuoteCreateRequestDto dto, int personId)
    {
        Quote tmp = new Quote();
        tmp.bookId = dto.bookId;
        tmp.personId = personId;
        tmp.quote = dto.quote;
        return context.quotes.Add(tmp).Entity;
    }

    private async Task<Quote> updateQuote(QuoteAlterRequestDto dto, int personId)
    {
        Quote? quoteToUpdate = await context.quotes.FindAsync(dto.quoteId);
        if(quoteToUpdate == null)
        {
            throw new QuoteNotFoundException("Cannot update a quote that does not exist");
        }
        if(quoteToUpdate.personId != personId)
        {
            throw new InvalidQuoteOperationException("Cannot update another users quotes");
        }
        quoteToUpdate.quote = dto.quote ?? quoteToUpdate.quote;
        await context.SaveChangesAsync();
        return quoteToUpdate;
    }

    private async Task<Quote> removeQuoteFromDb(int id, int personId)
    {
         Quote? quoteToDelete = await context.quotes.FindAsync(id);
        if(quoteToDelete == null)
        {
            throw new QuoteNotFoundException("Cannot delete a quote that does not exist");
        }
        if(quoteToDelete.personId != personId)
        {
            throw new InvalidQuoteOperationException("Cannot delete another users quotes");
        }
        context.quotes.Remove(quoteToDelete);
        await context.SaveChangesAsync();
        return quoteToDelete;
    }
}