using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class RefreshToken
{
    [Key]
    public int id{get; set;}
    public String token{get; set;} = String.Empty;
    public DateTime expires{get; set;}
    public int personId{get; set;}
    [ForeignKey(nameof(personId))]

    public Person person{get; set;} = null!;

     public RefreshToken() { }

    public RefreshToken(int id, string token, DateTime expires, int personId)
    {
        this.id = id;
        this.token = token;
        this.expires = expires;
        this.personId = personId;
    }
}