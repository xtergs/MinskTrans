using System.Threading.Tasks;
using Autofac;
using MetroLog;
using MetroLog.Targets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MinskTrans.Context;
using MinskTrans.Net;
using MinskTrans.Utilites.Base.IO;
using MinskTrans.Utilites.Base.Net;
using MinskTrans.Utilites.Desktop;

namespace DesktopUnitTests
{
    [TestClass]
    public class NewsManagerTests
    {
        private static IContainer container;

        [ClassInitialize]
        public static void Configuration(TestContext context)
        {
            var configuration = new LoggingConfiguration();

            //configuration.AddTarget(LogLevel.Debug, new PortableFileTarget(new DesktopFileSystem(), StorageType.Local));
            //configuration.AddTarget(LogLevel.Debug, new StreamingFileTarget());
            configuration.AddTarget(LogLevel.Debug, new DebugTarget());

            configuration.IsEnabled = true;

            LogManagerFactory.DefaultConfiguration = configuration;

            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterType<NewsManagerDesktop>();
            builder.RegisterType<FileHelperDesktop>().As<FileHelperBase>();
            builder.RegisterType<InternetHelperDesktop>().As<InternetHelperBase>();
            builder.RegisterInstance<ILogManager>(LogManagerFactory.DefaultLogManager).SingleInstance();
            builder.RegisterType<FilePathsSettings>().SingleInstance();
            builder.RegisterType<BaseNewsContext>().As<INewsContext>().SingleInstance();

            container = builder.Build();
        }

        [TestMethod]
        public async Task ParseNews()
        {
            var manager = container.Resolve<NewsManagerDesktop>();

            var result = await manager.CheckHotNewsAsync();
        }
    }
}
