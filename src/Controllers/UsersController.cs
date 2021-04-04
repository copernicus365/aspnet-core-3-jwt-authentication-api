using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
	[ApiController]
	[Route("users")]
	public class UsersController : ControllerBase
	{
		readonly AppSettings _appSettings;
		readonly IUserService _userService;

		public UsersController(IUserService userService, IOptions<AppSettings> appSettings)
		{
			_userService = userService;
			_appSettings = appSettings.Value;
		}

		[HttpPost("authenticate")]
		public IActionResult Authenticate(AuthenticateRequest model)
		{
			User user = _userService.GetByCredentials(model.Username, model.Password);

			if(user != null) {
				// authentication successful so generate jwt token
				string token = JwtUserMaker.GenerateJwtToken(user, _appSettings.Secret, DateTime.UtcNow.AddDays(1));

				if(token != null) {
					var response = new AuthenticateResponse(user, token);
					return Ok(response);
				}
			}

			return BadRequest(new { message = "Username or password is incorrect" });
		}

		[Authorize]
		[HttpGet]
		public IActionResult GetAll()
		{
			IEnumerable<User> users = _userService.GetAll();
			return Ok(users);
		}
	}
}
