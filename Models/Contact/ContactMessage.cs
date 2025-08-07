using NPoco;

namespace Lebo.Models.Contact
{
    [TableName("ContactMessages")]
    [PrimaryKey("Id")]
    [ExplicitColumns]
    public class ContactMessage
    {
        [Column("Id")]
        public Guid Id { get; set; }

        [Column("Name")]
        public string Name { get; set; } = default!;

        [Column("Email")]
        public string Email { get; set; } = default!;

        [Column("Message")]
        public string Message { get; set; } = default!;

        [Column("SubmittedAt")]
        public DateTime SubmittedAt { get; set; }
    }


    public class ContactMessageDto
    {
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Message { get; set; } = default!;
        public DateTime SubmittedAt { get; set; } = DateTime.Today;

        public ContactMessage ToModel()
        {
            return new ContactMessage
            {
                Name = Name,
                Email = Email,
                Message = Message,
                SubmittedAt = SubmittedAt
            };
        }

        public ContactMessageDto FromModel(ContactMessage message)
        {
            return new ContactMessageDto
            {
                Name = message.Name,
                Email = message.Email,
                Message = message.Message,
                SubmittedAt = message.SubmittedAt,
            };
        }
    }
}
