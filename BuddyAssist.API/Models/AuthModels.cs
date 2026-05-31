namespace BuddyAssist.API.Models
{
    public class RegisterRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Role { get; set; } = "user"; // default är user
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Role { get; set; } = string.Empty;
    }


    public class ProfileImageRequest
    {
        public string ImageBase64 { get; set; } = string.Empty;
    }

    public class CompleteRequest
    {
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }




    //for chat 

    public class ChatMessageRequest
    {
        public int ReceiverId { get; set; }
        public string Message { get; set; } = string.Empty;
    }



    public class SponsoredMissionRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime ScheduledAt { get; set; }
        public int Points { get; set; }
        public string SponsorName { get; set; } = string.Empty;
        public string? SponsorLogo { get; set; }
        public string? SponsorUrl { get; set; }
        public decimal SponsorBudget { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }




    public class EditUserRequest
    {
        public string? Name { get; set; }
        public string? City { get; set; }
        public string? Role { get; set; }
        public bool ClearProfileImage { get; set; } = false;
    }
}