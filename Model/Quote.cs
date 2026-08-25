using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Quote
{
    [Key]
    public int id{get; set;}
    public String quote{ get; set;}

    public int bookId{get; set;}
    [ForeignKey(nameof(bookId))]

    public Book book{get; set;} = null!;

    public int personId{get; set;}
    [ForeignKey(nameof(personId))]

    public Person person{get; set;} = null!;

    public Quote(){}
    public Quote(int id, String quote, int bookId, int personId)
    {
        this.id = id;
        this.quote = quote;
        this.bookId = bookId;
        this.personId = personId;
    } 
}