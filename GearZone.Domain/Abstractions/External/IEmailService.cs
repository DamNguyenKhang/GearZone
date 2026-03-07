using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Domain.Abstractions.External
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string htmlBody);
    }
}
