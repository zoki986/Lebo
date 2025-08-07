using Lebo.Models.Contact;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Lebo.Services
{
    public class ContactService
    {
        private readonly ISqlContext _context;
        private readonly IUmbracoDatabaseFactory _databaseFactory;
        public ContactService(ISqlContext context, IUmbracoDatabaseFactory databaseFactory)
        {
            _context = context;
            _databaseFactory = databaseFactory;
        }

        public void AddMessage(ContactMessageDto message)
        {
            var db = _databaseFactory.CreateDatabase();
            db.Insert(message.ToModel());
        }

        public void RemoveMessage(Guid id)
        {
            var db = _databaseFactory.CreateDatabase();

            var message = db.Fetch<ContactMessage>("WHERE Id = @Id", new { Id = id });

            if (message is not null)
            {
                db.Delete(message);
            }
        }

        public List<ContactMessage> GetAll()
        {
            return _databaseFactory.CreateDatabase().Fetch<ContactMessage>();
        }
    }

}
