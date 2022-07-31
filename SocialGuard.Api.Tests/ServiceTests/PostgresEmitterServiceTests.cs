using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using SocialGuard.Api.Data;
using SocialGuard.Api.Services;
using SocialGuard.Api.Tests.Fixtures;
using SocialGuard.Common.Data.Models;

namespace SocialGuard.Api.Tests.ServiceTests;

public class PostgresEmitterServiceTests : IClassFixture<ApiDbContextFixture>
{
	private ApiDbContextFixture _databaseFixture = new();
	
	public static Emitter GetTestUserEmitter() => new()
	{
		Snowflake = 1, 
		Login = "computerman", 
		EmitterType = EmitterType.User,
		DisplayName = "The Computer Man"
	};

	[Fact]
	public async Task GetEmitterAsync_String_ReturnsExisting()
	{
		// Arrange
		await _databaseFixture.Context.Database.EnsureCreatedAsync();
		PostgresEmitterService postgresEmitterService = new(_databaseFixture.Context);

		Emitter testUserEmitter = GetTestUserEmitter();
		_databaseFixture.Context.Emitters.Add(testUserEmitter);
		await _databaseFixture.Context.SaveChangesAsync();
		
		// Act
		Emitter result = postgresEmitterService.GetEmitterAsync(testUserEmitter.Login).Result;
		
		// Assert
		result.Should().Be(testUserEmitter);
		
		// Cleanup
		_databaseFixture.Context.Emitters.RemoveRange(_databaseFixture.Context.Emitters);
	}
	
	[Fact]
	public async Task GetEmitterAsync_String_ReturnsNull()
	{
		// Arrange
		await _databaseFixture.Context.Database.EnsureCreatedAsync();
		PostgresEmitterService postgresEmitterService = new(_databaseFixture.Context);
		
		// Act
		Emitter result = await postgresEmitterService.GetEmitterAsync("nothing");
		
		// Assert
		result.Should().BeNull();
	}
	
	[Fact]
	public async Task GetEmitterAsync_HttpContext_ReturnsExisting()
	{
		// Arrange
		await _databaseFixture.Context.Database.EnsureCreatedAsync();
		PostgresEmitterService postgresEmitterService = new(_databaseFixture.Context);
		
		Emitter testUserEmitter = GetTestUserEmitter();
		_databaseFixture.Context.Emitters.Add(testUserEmitter);
		await _databaseFixture.Context.SaveChangesAsync();

		Mock<HttpContext> mockHttpContext = new();
		mockHttpContext.SetupGet(c => c.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, testUserEmitter.Login) })));

		// Act
		Emitter result = await postgresEmitterService.GetEmitterAsync(mockHttpContext.Object);
		
		// Assert
		result.Should().Be(testUserEmitter);
		
		// Cleanup
		_databaseFixture.Context.Emitters.RemoveRange(_databaseFixture.Context.Emitters);
	}
	
	[Fact]
	public async Task GetEmitterAsync_HttpContext_ReturnsNull()
	{
		// Arrange
		await _databaseFixture.Context.Database.EnsureCreatedAsync();
		PostgresEmitterService postgresEmitterService = new(_databaseFixture.Context);
		
		Mock<HttpContext> mockHttpContext = new();
		mockHttpContext.SetupGet(c => c.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "nothing") })));
		
		// Act
		Emitter result = postgresEmitterService.GetEmitterAsync(mockHttpContext.Object).Result;
		
		// Assert
		Assert.Null(result);
	}
	
	[Fact]
	public async Task GetEmitterAsync_HttpContext_Throws()
	{
		// Arrange
		await _databaseFixture.Context.Database.EnsureCreatedAsync();
		PostgresEmitterService postgresEmitterService = new(_databaseFixture.Context);
		
		Mock<HttpContext> mockHttpContext = new();

		// Act
		Func<Task> operation = async () => await postgresEmitterService.GetEmitterAsync(mockHttpContext.Object);
		
		// Assert
		await operation.Should().ThrowAsync<Exception>();
	}
}