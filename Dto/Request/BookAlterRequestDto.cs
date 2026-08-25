using Microsoft.AspNetCore.Authorization;
using System;

public class BookAlterRequestDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Author { get; set; }
    public DateTime? Date { get; set; }

    public BookAlterRequestDto(int id, string? title = null, string? author = null, DateTime? date = null)
    {
        Id = id;
        Title = title;
        Author = author;
        Date = date;
    }
}