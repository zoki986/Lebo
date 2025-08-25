using Lebo.Models.Contact;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Lebo.Migrations.Contact
{
    public class ContactMessageMigration : MigrationPlan
    {
        public ContactMessageMigration() : base("ContactMessagesTable")
        {
            From(string.Empty)
                .To<CreateTable>("Table_Created");
        }
    }

    public class CreateTable : AsyncMigrationBase
    {
        public CreateTable(IMigrationContext context) : base(context)
        {
        }

        protected override Task MigrateAsync()
        {
            try
            {
                Create.Table<ContactMessage>().Do();
            }
            catch
            {

            }

            return Task.CompletedTask;
        }
    }
}
