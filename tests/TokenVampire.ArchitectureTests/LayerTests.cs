using System.Linq;
using System.Reflection;
using Xunit;
public class LayerTests
{
    static string[] Refs(Assembly a) => a.GetReferencedAssemblies().Select(x => x.Name!).ToArray();
    [Fact] public void Domain_references_no_project() =>
        Assert.DoesNotContain(Refs(typeof(TokenVampire.Domain.AssemblyMarker).Assembly), n => n.StartsWith("TokenVampire."));
    [Fact] public void Desktop_not_server() =>
        Assert.DoesNotContain(Refs(Assembly.Load("TokenVampire.Desktop")), n => n == "TokenVampire.Server");
    [Fact] public void Cli_not_server() =>
        Assert.DoesNotContain(Refs(Assembly.Load("TokenVampire.Cli")), n => n == "TokenVampire.Server");
    [Fact] public void Presentation_not_billing_or_legal() =>
        Assert.DoesNotContain(Refs(typeof(TokenVampire.Presentation.AssemblyMarker).Assembly),
            n => n is "TokenVampire.Billing" or "TokenVampire.Remedies" or "TokenVampire.Jurisdictions" or "TokenVampire.Compliance");
}
