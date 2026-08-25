using System.ComponentModel.DataAnnotations;

public class Person
{
    [Key]
    public int Id { get; set; }
    public String email{get; set;}
    public String password{get; set;}

    public Person()
    {
        
    }

    public Person(int id, String email, String password)
    {
        this.Id = id;
        this.email = email;
        this.password = password;
    }
}