// File: Calendar.Core/User.cs
namespace Calendar.Core;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    public ICollection<Event> Events { get; set; } = new List<Event>();
}