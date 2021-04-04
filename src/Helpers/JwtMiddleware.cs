using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

using DotNetXtensions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using WebApi.Entities;
using WebApi.Services;

namespace WebApi.Helpers
{
	public class JwtMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly AppSettings _appSettings;
		public int Num;

		public JwtMiddleware(RequestDelegate next, IOptions<AppSettings> appSettings)
		{
			_next = next;
			_appSettings = appSettings.Value;
			if(Num == 0)
				Num = new Random().Next(1, 1000); // to show this is performant, only hit at startup
		}

		public async Task Invoke(HttpContext context, IUserService userService)
		{
			string token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

			if(!token.IsNullOrWhiteSpace())
				_TryAttachUserToContextByValidToken(context, userService, token);

			await _next(context);
		}

		private bool _TryAttachUserToContextByValidToken(HttpContext context, IUserService userService, string token)
		{
			if(!JwtUserMaker.ValidateUserToken(
				token,
				_appSettings.Secret,
				out int userId,
				out JwtSecurityToken jwtToken))
				return false;

			// attach user to context on successful jwt validation
			User user = userService.GetById(userId);
			if(user != null) {
				context.Items["User"] = user;
				return true;
			}

			return false;
		}
	}
}