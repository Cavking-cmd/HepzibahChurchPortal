namespace ChurchPortal.Core.Entities
{
    public class User : BaseEntity
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? DisplayName { get; set; }
        public byte[]? AvatarFile { get; set; }
        public string? AvatarMimeType { get; set; }

        public bool EmailVerified { get; set; }

        /// <summary>True for accounts created through the public sign-up page.</summary>
        public bool IsSelfRegistered { get; set; }
        public string? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationTokenExpiry { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<UserCredential> Credentials { get; set; } = new List<UserCredential>();

        /// <summary>Reimbursement requests this user submitted (ReimbursementRequest.PayeeUserId).</summary>
        public ICollection<ReimbursementRequest> Reimbursements { get; set; } = new List<ReimbursementRequest>();
    }
}
