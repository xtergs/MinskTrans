using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MinskTrans.Context.Base;
using MyLibrary;

namespace MinskTrans.Context
{
    public class WebBussnessLogic : GenericBussnessLogic
    {
        public WebBussnessLogic(IContext cont) : base(cont)
        {
        }

        #region Overrides of GenericBussnessLogic

        public override ISettingsModelView Settings { get; }

        public override Task<bool> UpdateNewsTableAsync(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> UpdateTimeTableAsync(CancellationToken token, bool withLightCheck = false)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
