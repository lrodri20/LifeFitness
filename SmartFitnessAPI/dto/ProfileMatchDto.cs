// Dtos/ProfileDto.cs
using System;
using System.Collections.Generic;

namespace SmartFitnessApi.Data.Dtos
{
    /// <summary>
    /// Data Transfer Object for exposing a user's profile.
    /// </summary>
    public class ProfileMatchDto
    {
        public int UserId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Bio { get; set; }

        public string FitnessLevelName { get; set; } = string.Empty;
        public IEnumerable<string> Activities { get; set; } = Array.Empty<string>();
    }
}
