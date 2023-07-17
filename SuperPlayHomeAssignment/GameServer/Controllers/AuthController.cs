using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Common.Models.Api;
using GameServer.ConnectionManagement;
using GameServer.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace GameServer.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IPlayerRepository _playerRepository;
    private readonly IPlayerConnectionManager _playerConnectionManager;

    public AuthController(ILogger<AuthController> logger, IConfiguration configuration, IPlayerRepository playerRepository, IPlayerConnectionManager playerConnectionManager)
    {
        _logger = logger;
        _configuration = configuration;
        _playerRepository = playerRepository;
        _playerConnectionManager = playerConnectionManager;
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Post(DeviceInfoRequestModel deviceInfoRequestModel)
    {
        //todo: use some secret that device app and the server knows only. Use with https only.
        
        //todo: validate device UDID
        var player = _playerRepository.GetOrCreate(deviceInfoRequestModel.Udid);
        
        //create claims details based on the user information
        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()), //todo: use DateTimeProvider
            new Claim("UserId", player.Id.ToString()),
            new Claim("Udid", player.UdId),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            _configuration["Jwt:Issuer"],
            _configuration["Jwt:Audience"],
            claims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: signIn);

        return Ok(new LoginResponseModel
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ConnectionEsteblished = _playerConnectionManager.HasValidConnection(deviceInfoRequestModel.Udid)
        });
    }
}