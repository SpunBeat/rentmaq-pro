namespace RentMaq.Domain.Entities;

public class EquipmentCertification
{
    public Guid CertificationId { get; set; }
    public Guid EquipmentId { get; set; }
    public string CertificationName { get; set; } = null!;
    public string CertificationType { get; set; } = null!;
    public string? IssuedBy { get; set; }
    public DateOnly IssueDate { get; set; }
    public DateOnly ExpirationDate { get; set; }
    public bool BlocksRental { get; set; } = true;
    public string? DocumentUrl { get; set; }
    public string Status { get; set; } = null!; // VALID, EXPIRED, REVOKED

    public DateTimeOffset CreatedAt { get; set; }

    // Navigation
    public Equipment Equipment { get; set; } = null!;
}
