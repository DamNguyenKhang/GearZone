using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Application.Common
{
    public class Constant
    {
        public static Dictionary<StoreStatus, string> subject = new()
        {
            {StoreStatus.Approved , "Your Store Application has been Approved!"},
            {StoreStatus.Rejected, "Your Store Application Status" }
        };

        public static Dictionary<StoreStatus, string> body = new Dictionary<StoreStatus, string>()
        {
            {
                StoreStatus.Rejected,
                @"<h3>Dear {0},</h3>
                <p>Thank you for applying to become a seller on GearZone with your store <strong>{1}</strong>.</p>
                <p>After careful review, we regret to inform you that your application has been <strong>rejected</strong> at this time.</p>
                <p><strong>Reason for rejection:</strong><br/>{2}</p><br/>
                <p>If you have any questions or would like to submit additional information, please reply to this email.</p><br/>
                <p>Best regards,<br/>The GearZone Team</p>"
            },

            {
                StoreStatus.Approved,
                $@"
                <h3>Congratulations {0},</h3>
                <p>We are thrilled to inform you that your store application for <strong>{1}</strong> has been <strong>approved</strong>.</p>
                <p>You can now log in to the Seller Dashboard and start managing your store.</p>
                <br/>
                <p>Best regards,<br/>The GearZone Team</p>"
            }
        };
    }
}
