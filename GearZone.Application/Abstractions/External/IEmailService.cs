using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Application.Abstractions.External
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string htmlBody);
    }
}
