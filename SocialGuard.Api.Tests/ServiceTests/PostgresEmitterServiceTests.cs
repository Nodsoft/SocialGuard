using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using SocialGuard.Api.Data;
using SocialGuard.Api.Services;
using SocialGuard.Api.Tests.Fixtures;
using SocialGuard.Api.Tests.TestData;
using SocialGuard.Common.Data.Models;

namespace SocialGuard.Api.Tests.ServiceTests;

public class PostgresEmitterServiceTests : IClassFixture<ApiDbContextFixture>
{
	private ApiDbContextFixture _databaseFixture = new();

	[Fact]
	public async Task GetEmitterAsync_String_ReturnsExisting()
	{
		// Arrange
		await _databaseFixture.Context.Database.EnsureCreatedAsync();
		PostgresEmitterService postgresEmitterService = new(_databaseFixture.Context);
		
		_databaseFixture.Context.Emitters.Add(EmitterTestData.UserEmitter);
		await _databaseFixture.Context.SaveChangesAsync();
		
		// Act
		Emitter result = postgresEmitterService.GetEmitterAsync(EmitterTestData.UserEmitter.Login).Result;
		
		// Assert
		result.Should().Be(EmitterTestData.UserEmitter);
		
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
		
		_databaseFixture.Context.Emitters.Add(EmitterTestData.UserEmitter);
		await _databaseFixture.Context.SaveChangesAsync();

		Mock<HttpContext> mockHttpContext = new();
		mockHttpContext.SetupGet(c => c.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, EmitterTestData.UserEmitter.Login) })));

		// Act
		Emitter result = await postgresEmitterService.GetEmitterAsync(mockHttpContext.Object);
		
		// Assert
		result.Should().Be(EmitterTestData.UserEmitter);
		
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
		Func<Task> operation = () => postgresEmitterService.GetEmitterAsync(mockHttpContext.Object);
		
		// Assert
		await operation.Should().ThrowAsync<Exception>();
	}
	
	[Fact]
	public async Task CreateOrUpdateEmitterSelfAsync_HttpContextEmitter_Nominal()
	{
		// Arrange
		await _databaseFixture.Context.Database.EnsureCreatedAsync();
		PostgresEmitterService postgresEmitterService = new(_databaseFixture.Context);
		
		Mock<HttpContext> mockHttpContext = new();
		mockHttpContext.SetupGet(c => c.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, EmitterTestData.UserEmitter.Login) })));
		
		// Act
		Func<Task> operation = () => postgresEmitterService.CreateOrUpdateEmitterSelfAsync(EmitterTestData.UserEmitter, mockHttpContext.Object);
		
		// Assert
		await operation.Should().NotThrowAsync();
		_databaseFixture.Context.Emitters.Should().Contain(EmitterTestData.UserEmitter);
		
		// Cleanup
		_databaseFixture.Context.Emitters.RemoveRange(_databaseFixture.Context.Emitters);
	}
	
	[Fact]
	public async Task DeleteEmitterAsync_String_Nominal()
	{
		// Arrange
		await _databaseFixture.Context.Database.EnsureCreatedAsync();
		PostgresEmitterService postgresEmitterService = new(_databaseFixture.Context);
		
		_databaseFixture.Context.Emitters.Add(EmitterTestData.UserEmitter);
		await _databaseFixture.Context.SaveChangesAsync();
		
		// Act
		Func<Task> operation = () => postgresEmitterService.DeleteEmitterAsync(EmitterTestData.UserEmitter.Login);
		
		// Assert
		await operation.Should().NotThrowAsync();
		_databaseFixture.Context.Emitters.Should().NotContain(EmitterTestData.UserEmitter);
		
		// Cleanup
		_databaseFixture.Context.Emitters.RemoveRange(_databaseFixture.Context.Emitters);
	}
}