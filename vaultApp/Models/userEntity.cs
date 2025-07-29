
public class UserEntity
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public DateTime CreatedAt { get; set; }

    public UserEntity(string? id, string? name, string? email, string? password, DateTime createdAt)
    {
        Id = id;
        Name = name;
        Email = email;
        Password = password;
        CreatedAt = createdAt;
    }
}

