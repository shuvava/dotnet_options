using System;
using System.Text;

using Microsoft.Extensions.Options;

using OptionsExampleApp.Models;

/**
 * The difference between IOptionsSnapshot and IOptionsMonitor is that the IOptionsSnapshot will
 * just give you a snapshot of the options at the time the IOptionsSnapshot<T> object is being constructed.
 *
 * That’s why the usage is exactly the same as with IOptions<T>: You inject it in the constructor and then
 * store the options.Value in the instance to access the options later. At that point, that object is fixed
 * and will never change. It’s just that the IOptionsSnapshot<T> is registered as a scoped dependency instead
 * of a singleton dependency like IOptions<T>, so it gets the chance to get the current configuration
 * values on every request instead of just once.
 *
 * The IOptionsMonitor<T> however is a singleton service that allows you to retrieve the current
 * configuration value at any given time. So it is especially useful for singleton services that need
 * to get the current configuration whenever they need it. In addition, the options monitor offers a
 * push mechanism to get notified of configuration changes by the configuration sources. That way,
 * you can explicitly handle configuration changes.
 *
 * Options snapshots are designed to be used for transient or scoped dependencies, so you will be
 * fine just using those most of the time. Only in rare occasions when you have to use a singleton
 * service that also needs to have the most updated configuration, you should have the need to use an options monitor.
 * In those cases, note that just switching from snapshots to a monitor will not be enough. Usually, you will have to
 * handle changed configuration in some way (for example clean up state, clear caches etc.). So you should always think
 * about whether you actually need reloadable configuration for everything or if just restarting the application isn’t a viable alternative
 */
namespace OptionsExampleApp.Services
{
    public class SomeService : ISomeService
    {
        private readonly Guid _serviceId;
        private readonly IPresenter _presenter;


        // IOptionsMonitor is used for notifications when TOptions instances change. I
        // OptionsMonitor supports reloadable options, change notifications, and IPostConfigureOptions.
        private readonly IOptionsMonitor<Settings> _settingsMonitor;


        // IOptionsSnapshot supports reloading options with minimal processing overhead.
        // give you a snapshot of the options
        // Options are computed once at the time the IOptionsSnapshot<T> object is being constructed.
        //private readonly IOptionsSnapshot<Settings> _settingsSnapshot;
        private readonly Settings _settingsStatic;


        public SomeService(
            IOptions<Settings> settingsStatic,
            IOptionsSnapshot<Settings> settingsSnapshot,
            IOptionsMonitor<Settings> settingsMonitor,
            IPresenter presenter)
        {
            _settingsStatic = settingsStatic.Value;
            //_settingsSnapshot = settingsSnapshot;
            _settingsMonitor = settingsMonitor;

            _settingsMonitor.OnChange((Settings config, string val) => Console.WriteLine($"IOptionsMonitor changed: {val}"));

            _serviceId = Guid.NewGuid();
            _presenter = presenter;
        }


        private Settings SettingsMonitor => _settingsMonitor.CurrentValue;
        //private Settings SettingsSnapshot => _settingsSnapshot.Value;


        public string PrintCurrentConfig()
        {
            var builder = new StringBuilder();
            builder.AppendLine("====================================");
            builder.AppendLine($"service id: {_serviceId}");
            builder.AppendLine("====================================");
            builder.AppendLine("IOptions<Settings> value:");
            builder.AppendLine(_presenter.Serialize(_settingsStatic));
            //builder.AppendLine("------------------------------------");
            //builder.AppendLine("IOptionsSnapshot<Settings> value:");
            //builder.AppendLine(_presenter.Serialize(SettingsSnapshot));
            builder.AppendLine("------------------------------------");
            builder.AppendLine("IOptionsMonitor<Settings> value:");
            builder.AppendLine(_presenter.Serialize(SettingsMonitor));
            builder.AppendLine("------------------------------------");

            return builder.ToString();
        }
    }
}
