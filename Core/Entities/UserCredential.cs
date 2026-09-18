namespace ChurchPortal.Core.Entities
{
    public class UserCredential : BaseEntity
    {
        public Guid UserId { get; set; }
        public User? User { get; set; }
        public required byte[] CredentialId { get; set; }
        public required byte[] PublicKey { get; set; }
        public long SignatureCounter { get; set; }
        public string? Nickname { get; set; } // oba come back and check  this  code !!
        public Guid? AaGuid { get; set; }
    }
}
