using System.Collections.Generic;
using System.Linq;

using WebApi.Entities;

namespace WebApi.Services
{
	public interface IUserService
	{
		IEnumerable<User> GetAll();
		User GetByCredentials(string userName, string password);
		User GetById(int id);
	}

	public class UserService : IUserService
	{
		// users hardcoded for simplicity, store in a db with hashed passwords in production applications
		private List<User> _users = new List<User>
		{
			new User { Id = 1, OrgId = 301, FirstName = "Test", LastName = "User", Username = "test", Password = "test" },
			new User { Id = 2, OrgId = 403, FirstName = "Charlie", LastName = "Brown", Username = "charlie.brown", Password = "snoops!" },
		};

		public IEnumerable<User> GetAll()
			=> _users;

		public User GetById(int id)
			=> _users.FirstOrDefault(x => x.Id == id);

		public User GetByCredentials(string userName, string password)
			=> _users.SingleOrDefault(x => x.Username == userName && x.Password == password);

	}
}