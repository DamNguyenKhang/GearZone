using GearZone.Application.Entities;
using System;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface IPaymentRepository : IRepository<Payment, Guid>
    {
    }
}
