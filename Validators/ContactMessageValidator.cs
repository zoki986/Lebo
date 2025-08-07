using FluentValidation;
using Lebo.Models.Contact;

namespace Lebo.Validators
{
    public class ContactMessageValidator : AbstractValidator<ContactMessageDto>
    {
        public ContactMessageValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Message).NotEmpty();
        }
    }
}
