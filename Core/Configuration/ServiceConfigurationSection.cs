using System.Configuration;

namespace EnergonSoftware.Core.Configuration
{
    /// <summary>
    /// Useful configuration options for services
    /// </summary>
    public class ServiceConfigurationSection : ConfigurationSection
    {
        /// <summary>
        /// Gets or sets a value indicating whether the service should run as a service or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if the service should run as a service; otherwise, <c>false</c>.
        /// </value>
        [ConfigurationProperty("runAsService", IsRequired = true)]
        public bool RunAsService => (bool)base["runAsService"];

        /// <summary>
        /// Gets or sets a value indicating whether the service should shutdown gracefully.
        /// </summary>
        /// <value>
        /// <c>true</c> if the service should shutdown gracefully; otherwise, <c>false</c>.
        /// </value>
        [ConfigurationProperty("gracefulShutdown", IsRequired = true)]
        public bool GracefulShutdown => (bool)base["gracefulShutdown"];
    }
}
