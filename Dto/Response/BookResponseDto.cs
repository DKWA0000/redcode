using System;

public class BookResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime Date { get; set; }

    public BookResponseDto(int id, string title, string author, DateTime date)
    {
        Id = id;
        Title = title;
        Author = author;
        Date = date;
    }
}