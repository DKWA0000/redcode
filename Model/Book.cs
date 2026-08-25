using System.ComponentModel.DataAnnotations;

public class Book
{
    [Key]
    public int Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public DateTime Date { get; set; }

    // En tom konstruktor krävs av EF Core
    public Book() {}

    // Din egna konstruktor (valfri, men smidig)
    public Book(int id, string title, string author, DateTime date)
    {
        Id = id;
        Title = title;
        Author = author;
        Date = date;
    }
}