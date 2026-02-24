using GearZone.Domain.Entities;
using System;

namespace GearZone.Domain.Abstractions.Persistence
{
    public interface IPaymentRepository : IRepository<Payment, Guid>
    {
    }
}
