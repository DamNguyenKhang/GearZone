using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Domain.Enums
{
    public enum OrderStatus
    {
        Pending,
        Paid,
        Processing,
        Shipped,
        Completed,
        Cancelled,
        Refunded
    }
}
