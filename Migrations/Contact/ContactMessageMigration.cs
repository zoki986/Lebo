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

    public class CreateTable : MigrationBase
    {
        public CreateTable(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            Create.Table<ContactMessage>().Do();
        }
    }
}
