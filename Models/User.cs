using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace api.Models
{
    public class User : IdentityUser
    {

        public List<RefreshToken> RefreshTokens { get; private set; }

        public List<DeviceToken> DeviceTokens { get; private set; }

        public List<ScheduledRun> ScheduledRuns { get; private set; }

        public List<ExpoPushToken> ExpoPushTokens { get; private set; }

        public List<WateringScheduleItem> WateringSchedules { get; private set; }

        public User()
        {
            this.WateringSchedules = new List<WateringScheduleItem>();
            this.RefreshTokens = new List<RefreshToken>();
            this.DeviceTokens = new List<DeviceToken>();
            this.ScheduledRuns = new List<ScheduledRun>();
            this.ExpoPushTokens = new List<ExpoPushToken>();
        }

        public List<ExpoPushToken> GetActiveExpoPushTokens()
        {
            return ExpoPushTokens.FindAll(pt => pt.IsActive == true);
        }

        public void AddExpoPushToken(string token)
        {
            ExpoPushTokens.Add(new ExpoPushToken(token, true));
        }

        public bool HasValidRefreshToken(string refreshToken)
        {
            return RefreshTokens.Any(rt => rt.Token == refreshToken && rt.Active);
        }

        public void AddRefreshToken(string token, double daysToExpire = 5)
        {
            RefreshTokens.Add(new RefreshToken(token, DateTime.UtcNow.AddDays(daysToExpire), this.Id));
        }

        public void RemoveRefreshToken(string refreshToken)
        {
            RefreshTokens.Remove(RefreshTokens.First(t => t.Token == refreshToken));
        }

        public void AddDeviceToken(string deviceToken)
        {
            this.DeviceTokens.Add(new DeviceToken(deviceToken, this.Id));
        }

        public void ClearDeviceTokens()
        {
            this.DeviceTokens.Clear();
        }

        public void ClearRefreshTokens()
        {
            this.RefreshTokens.Clear();
        }

        public void RemoveDeviceToken(string deviceToken)
        {
            this.DeviceTokens.Remove(this.DeviceTokens.First(t => t.Token == deviceToken));
        }

        public string GetFirstDeviceToken()
        {
            if (this.DeviceTokens.Count > 0)
            {
                return this.DeviceTokens[0].Token;
            }
            else
            {
                return null;
            }

        }
    }
}