namespace Feast.Models;

public class UserModel
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public TimeSpan BreakfastTime { get; set; } 
    public TimeSpan LunchTime { get; set; } 
    public TimeSpan DinnerTime { get; set; } 
}