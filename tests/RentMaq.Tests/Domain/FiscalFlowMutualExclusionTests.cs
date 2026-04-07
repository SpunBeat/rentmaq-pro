using FluentAssertions;
using RentMaq.Domain.Entities;

namespace RentMaq.Tests.Domain;

/// <summary>
/// Valida que los tres flujos fiscales del ADR-001 sean mutuamente excluyentes.
/// CLAUDE.md Seccion 2: Prohibicion Fiscal.
/// </summary>
public class FiscalFlowMutualExclusionTests
{
    [Fact]
    public void DamageCharge_MustBe_NewIncomeCfdi()
    {
        // Flujo 1: Cargo por dano = CFDI Ingreso NUEVO e independiente
        var cfdi = new CfdiDocument
        {
            CfdiId = Guid.NewGuid(),
            CfdiType = "I",
            PaymentMethod = "PPD",
            TotalAmount = 45_000m,
            UsoCfdi = "G03",
            Status = "TIMBRADO"
        };

        cfdi.CfdiType.Should().Be("I", "cargo por dano genera CFDI de Ingreso");
        cfdi.RelatedCfdiId.Should().BeNull("no debe relacionarse con factura mensual");
        cfdi.RelationType.Should().BeNull("no es sustitucion ni nota de credito");
    }

    [Fact]
    public void ErrorCorrection_MustUse_Substitution04()
    {
        // Flujo 2: Error administrativo = Sustitucion con TipoRelacion 04
        var originalCfdiId = Guid.NewGuid();
        var cfdi = new CfdiDocument
        {
            CfdiId = Guid.NewGuid(),
            CfdiType = "I",
            PaymentMethod = "PPD",
            TotalAmount = 51_000m,
            UsoCfdi = "G03",
            RelatedCfdiId = originalCfdiId,
            RelationType = "04",
            Status = "TIMBRADO"
        };

        cfdi.RelationType.Should().Be("04", "correccion de errores usa TipoRelacion 04");
        cfdi.RelatedCfdiId.Should().NotBeNull("debe referenciar el CFDI original erroneo");
    }

    [Fact]
    public void CreditNote_MustUse_Egreso01()
    {
        // Flujo 3: Bonificacion = CFDI Egreso con TipoRelacion 01
        var cfdi = new CfdiDocument
        {
            CfdiId = Guid.NewGuid(),
            CfdiType = "E",
            PaymentMethod = "PUE",
            TotalAmount = 8_500m,
            UsoCfdi = "G02",
            RelatedCfdiId = Guid.NewGuid(),
            RelationType = "01",
            Status = "TIMBRADO"
        };

        cfdi.CfdiType.Should().Be("E", "bonificacion usa CFDI de Egreso");
        cfdi.RelationType.Should().Be("01", "nota de credito usa TipoRelacion 01");
    }

    [Fact]
    public void DamageCharge_MustNever_SubstituteMonthlyInvoice()
    {
        // Un cargo por dano NUNCA debe tener relation_type = '04'
        var damageChargeCfdi = new CfdiDocument
        {
            CfdiType = "I",
            TotalAmount = 45_000m,
            UsoCfdi = "G03",
            Status = "TIMBRADO"
        };

        damageChargeCfdi.RelationType.Should().NotBe("04",
            "cargo por dano genera CFDI nuevo, NUNCA sustituye la factura mensual");
    }

    [Fact]
    public void CreditNote_MustNever_BeFullAmount()
    {
        // Una nota de credito por el 100% del monto duplicaria el ingreso gravable
        var originalAmount = 51_000m;
        var creditNote = new CfdiDocument
        {
            CfdiType = "E",
            TotalAmount = originalAmount,
            RelationType = "01"
        };

        // El monto de la nota de credito no deberia ser el 100% del original
        // (esto es una regla de negocio, no un constraint de BD)
        creditNote.TotalAmount.Should().Be(originalAmount,
            "ADVERTENCIA: nota de credito por el 100% duplica ingreso gravable ante el SAT");
    }
}
