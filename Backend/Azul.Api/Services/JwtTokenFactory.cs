using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Azul.Api.Services.Contracts;
using Azul.Core.UserAggregate;
using Microsoft.IdentityModel.Tokens;

namespace Azul.Api.Services;

//DO NOT TOUCH THIS FILE!!
public class JwtTokenFactory(TokenSettings settings) : ITokenFactory
{
    public string CreateToken(User user, IList<string> roleNames)
    {
         
        var allClaims = new[]
        {
            new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
        }.ToList();

        foreach (string roleName in roleNames)
        {
            allClaims.Add(new Claim(ClaimTypes.Role, roleName));
        }

        var keyBytes = Encoding.UTF8.GetBytes(settings.Key);
        var symmetricSecurityKey = new SymmetricSecurityKey(keyBytes);
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        var jwtSecurityToken = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: allClaims,
            expires: DateTime.UtcNow.AddMinutes(settings.ExpirationTimeInMinutes),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
    }
}