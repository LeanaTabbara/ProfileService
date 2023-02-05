using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using ProfileService.Web.Dtos;
using ProfileService.Web.Storage;

namespace ProfileService.Web.Tests.Controllers;

public class ProfileControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProfileControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }
    
    [Fact]
    public async Task GetProfile()
    {
        var profileStoreMock = new Mock<IProfileStore>();

        var profile = new Profile("foobar", "Foo", "Bar");
        profileStoreMock.Setup(m => m.GetProfile(profile.Username))
            .ReturnsAsync(profile);
                    
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton(profileStoreMock.Object);
            });
        }).CreateClient();

        var response = await client.GetAsync($"/Profile/{profile.Username}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.Equal(profile, JsonConvert.DeserializeObject<Profile>(responseBody));
    }
    
    // TODO: Add a test for NotFound response
    [Fact]
    public async Task GetProfileNotFound()
    {
        var profileStoreMock = new Mock<IProfileStore>();

        var profile = new Profile("foobar", "Foo", "Bar");
        profileStoreMock.Setup(m => m.GetProfile(profile.Username))
            .ReturnsAsync(profile);
                    
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton(profileStoreMock.Object);
            });
        }).CreateClient();

        var response = await client.GetAsync($"/Profile/foo");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    //TODO: Add tests for remaining APIs 
    
    [Fact]
    public async Task AddProfile()
    {
        var profileStoreMock = new Mock<IProfileStore>();

        var profile = new Profile("foobar", "Foo", "Bar");
        profileStoreMock.Setup(m => m.UpsertProfile(profile))
            .Returns(Task.CompletedTask);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton(profileStoreMock.Object);
            });
        }).CreateClient();

        var myContent = JsonConvert.SerializeObject(profile);
        var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
        var byteContent = new ByteArrayContent(buffer);
        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        
        var response = await client.PostAsync("/Profile", byteContent);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.Equal(profile, JsonConvert.DeserializeObject<Profile>(responseBody));
    }
    
    [Fact]
    public async Task AddProfileConflict()
    {
        var profileStoreMock = new Mock<IProfileStore>();

        var profile = new Profile("foobar", "Foo", "Bar");
        profileStoreMock.Setup(m => m.UpsertProfile(profile))
            .Returns(Task.CompletedTask);
        profileStoreMock.Setup(m => m.GetProfile(profile.Username))
            .ReturnsAsync(profile);
                    
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton(profileStoreMock.Object);
            });
        }).CreateClient();

        var myContent = JsonConvert.SerializeObject(profile);
        var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
        var byteContent = new ByteArrayContent(buffer);
        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        
        
        var response = await client.PostAsync("/Profile", byteContent);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        // var responseBody = await response.Content.ReadAsStringAsync();
        // Assert.Equal(profile, JsonConvert.DeserializeObject<Profile>(responseBody));
    }
    
    [Fact]
    public async Task UpdateProfile()
    {
        var profileStoreMock = new Mock<IProfileStore>();

        var profile = new Profile("foobar", "Foo", "Bar");
        var profile2 = new Profile("foobar", "Foo1", "Bar1");
        profileStoreMock.Setup(m => m.UpsertProfile(profile2))
            .Returns(Task.CompletedTask);
        profileStoreMock.Setup(m => m.GetProfile(profile.Username))
            .ReturnsAsync(profile);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton(profileStoreMock.Object);
            });
        }).CreateClient();

        var myContent = JsonConvert.SerializeObject(profile2);
        var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
        var byteContent = new ByteArrayContent(buffer);
        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        
        var response = await client.PutAsync($"/Profile/{profile2.Username}", byteContent);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.Equal(profile2, JsonConvert.DeserializeObject<Profile>(responseBody));
    }
    
    [Fact]
    public async Task UpdateProfileNotFound()
    {
        var profileStoreMock = new Mock<IProfileStore>();

        var profile2 = new Profile("foobar", "Foo1", "Bar1");
        profileStoreMock.Setup(m => m.UpsertProfile(profile2))
            .Returns(Task.CompletedTask);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton(profileStoreMock.Object);
            });
        }).CreateClient();

        var myContent = JsonConvert.SerializeObject(profile2);
        var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
        var byteContent = new ByteArrayContent(buffer);
        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        
        var response = await client.PutAsync($"/Profile/{profile2.Username}", byteContent);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
}