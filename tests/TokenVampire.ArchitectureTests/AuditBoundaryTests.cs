using System.Reflection;
using Xunit;

public sealed class AuditBoundaryTests
{
    [Fact]
    public void Domain_remains_dependency_free()
    {
        var references = typeof(TokenVampire.Domain.AuditObservation).Assembly
            .GetReferencedAssemblies().Select(a => a.Name).ToArray();
        Assert.DoesNotContain(references, name => name is not null && name.StartsWith("TokenVampire.", StringComparison.Ordinal));
    }

    [Fact]
    public void Parsing_has_no_infrastructure_or_vendor_sdk_reference()
    {
        var references = typeof(TokenVampire.Parsing.LocalTokenEstimator).Assembly
            .GetReferencedAssemblies().Select(a => a.Name).ToArray();
        Assert.DoesNotContain(references, name => name is "TokenVampire.Infrastructure" or "TokenVampire.Server");
    }
}
