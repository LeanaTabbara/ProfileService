using Microsoft.AspNetCore.Mvc;
using ProfileService.Web.Dtos;
using ProfileService.Web.Storage;

namespace ProfileService.Web.Controllers;//i added the brackets there was an error check again

[ApiController]
[Route("[controller]")]
public class ProfileController : ControllerBase
{
    private readonly IProfileStore _profileStore;

    public ProfileController(IProfileStore profileStore)
    {
        _profileStore = profileStore;
    }
        
    [HttpGet("{username}")]
    public async Task<ActionResult<Profile>> GetProfile(string username)
    {
        var profile = await _profileStore.GetProfile(username);
        if (profile == null)
        {
            return NotFound($"A User with username {username} was not found");
        }
            
        return Ok(profile);
    }

    [HttpPost]
    public async Task<ActionResult<Profile>> AddProfile(Profile profile)
    {
        var existingProfile = await _profileStore.GetProfile(profile.Username);
        if (existingProfile != null)
        {
            return Conflict($"A user with username {profile.Username} already exists");
        }

        await _profileStore.UpsertProfile(profile);
        return CreatedAtAction(nameof(GetProfile), new {username = profile.Username},
            profile);
    }

    // TODO: the Put request body should not contain the username because it's already in the URI.
    // Add a new DTO that contains only the First and Last name and use it instead (name it PutProfileRequest)
    [HttpPut("{username}")]
    public async Task<ActionResult<Profile>> UpdateProfile(String username, PutProfileRequest putProfileRequest)
    {
        var existingProfile = await _profileStore.GetProfile(username);
        if (existingProfile == null)
        {
            return NotFound($"A User with username {username} was not found");
        }

        Profile profile = new Profile(username, putProfileRequest.FirstName, putProfileRequest.LastName);
        await _profileStore.UpsertProfile(profile);
        return Ok(profile);
    }
}