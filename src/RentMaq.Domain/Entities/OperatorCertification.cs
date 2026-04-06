using RentMaq.Domain.Enums;

namespace RentMaq.Domain.Entities;

public class OperatorCertification
{
    public Guid OperatorCertId { get; set; }
    public Guid? ContractId { get; set; }
    public string OperatorName { get; set; } = null!;
    public string? Dc3CertificateNumber { get; set; }
    public EquipmentTypeEnum EquipmentTypeCertified { get; set; }
    public DateOnly IssuedAt { get; set; }
    public DateOnly? ExpiresAt { get; set; }
    public string? DocumentUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    // Navigation
    public RentalContract? Contract { get; set; }
}
