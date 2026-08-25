using System;

public class BookCreateRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime Date { get; set; }

    public BookCreateRequestDto(string title, string author, DateTime date)
    {
        Title = title;
        Author = author;
        Date = date;
    }
}