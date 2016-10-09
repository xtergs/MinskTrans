using System;
using System.Threading.Tasks;

namespace MinskTrans.Context.Base
{
    public enum Permision
    {
        Allow,
        Denied,
        Unspecified
    }
    public interface IGeolocation
    {
        int MovementThreshold { get; set; }
        uint ReportInterval { get; set; }
        Location CurLocation { get; }

        Task<Permision> CheckPermision();

        event PositionChangedEventArgs PositionChanged;
        event StatusChangedEventArgs StatusChanged;
        event EventHandler<Permision> PermissionChanged;
    }

    public delegate void PositionChangedEventArgs(object sender, PositionChangedEventArgsArgs args);

    public class PositionChangedEventArgsArgs : EventArgs
    {
        public Location NewLocation { get; set; }
        public Location PrevLocation { get; set; }
    }

    public delegate void StatusChangedEventArgs(object sender, StatusChangedEventArgsArgs args);

    public class StatusChangedEventArgsArgs : EventArgs
    {
        public PositionStatus Status { get; set; }
    }

    public enum PositionStatus
    {
        Ready,
        NotAvailable,
        Disabled
    }
}