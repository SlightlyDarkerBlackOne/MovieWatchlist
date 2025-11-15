using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using MovieWatchlist.Tests.Shared.Infrastructure;

namespace MovieWatchlist.Infrastructure.IntegrationTests;

/// <summary>
/// Integration tests for infrastructure services (external APIs, email, etc.)
/// </summary>
public class InfrastructureIntegrationTests : EnhancedIntegrationTestBase
{
    public InfrastructureIntegrationTests(WebApplicationFactory<Program> factory) 
        : base(factory)
    {
    }

    [Fact]
    public async Task HttpClient_CanMakeRequests_Successfully()
    {
        var response = await Client.GetAsync("/api/Movies/popular");
        
        response.Should().NotBeNull();
    }
}
