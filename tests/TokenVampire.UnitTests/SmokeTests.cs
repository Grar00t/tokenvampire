using Xunit;
public class SmokeTests { [Fact] public void MarkerExists() => Assert.NotNull(typeof(TokenVampire.Domain.AssemblyMarker)); }
