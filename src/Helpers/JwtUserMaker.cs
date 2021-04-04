using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

using DotNetXtensions;

using Microsoft.IdentityModel.Tokens;

using WebApi.Entities;

namespace WebApi.Helpers
{
	public class JwtUserMaker
	{
		public static bool ValidateUserToken(
			string token, string tokenSecret, out int userId, out JwtSecurityToken jwtToken)
		{
			userId = -1;
			jwtToken = null;
			try {
				if(token.IsNullOrWhiteSpace() || tokenSecret.IsNullOrWhiteSpace())
					return false;

				JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

				byte[] key = Encoding.ASCII.GetBytes(tokenSecret);

				var tokenValidationParams = new TokenValidationParameters {
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(key),
					ValidateIssuer = false,
					ValidateAudience = false,
					// set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
					ClockSkew = TimeSpan.Zero
				};

				ClaimsPrincipal claimPrin = tokenHandler.ValidateToken(
					token, tokenValidationParams, out SecurityToken validatedToken);

				jwtToken = (JwtSecurityToken)validatedToken;

				string claimId = jwtToken.Claims.First(x => x.Type == "id").Value;

				userId = claimId == null ? -1 : int.Parse(claimId);
				if(userId > 0)
					return true;
			}
			catch {
				// do nothing if jwt validation fails
				// user is not attached to context so request won't have access to secure routes
			}
			return false;
		}

		public static string GenerateJwtToken(User user, string tokenSecret, DateTime? expires = null)
		{
			// generate token that is valid for 7 days
			var tokenHandler = new JwtSecurityTokenHandler();

			byte[] key = Encoding.ASCII.GetBytes(tokenSecret);

			var extraClaims = new Dictionary<string, object>() {
				{ "orgid", user.OrgId },
				{ "username", user.Username },
				{ "userfullname", $"{user.FirstName} {user.LastName}" },
			};

			SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor {
				Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
				Claims = extraClaims,
				Expires = expires ?? DateTime.UtcNow.AddDays(1),
				SigningCredentials = new SigningCredentials(
					new SymmetricSecurityKey(key),
					SecurityAlgorithms.HmacSha256Signature)
			};

			SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

			string jwtToken = tokenHandler.WriteToken(token);
			return jwtToken;
		}

	}
}
