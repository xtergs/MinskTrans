using System;
using System.Threading.Tasks;
using MinskTrans.Context.Base;

namespace MinskTrans.Context.Fakes
{
    public class FakeGeolocation : IGeolocation
    {
        #region Implementation of IGeolocation

        public int MovementThreshold { get; set; }
        public uint ReportInterval { get; set; }
        public Location CurLocation { get; }
        public async Task<Permision> CheckPermision()
        {
            return Permision.Allow;
        }

        public event PositionChangedEventArgs PositionChanged;
        public event StatusChangedEventArgs StatusChanged;
        public event EventHandler<Permision> PermissionChanged;

        #endregion

        protected virtual void OnPositionChanged(PositionChangedEventArgsArgs args)
        {
            PositionChanged?.Invoke(this, args);
        }

        protected virtual void OnStatusChanged(StatusChangedEventArgsArgs args)
        {
            StatusChanged?.Invoke(this, args);
        }
    }
}