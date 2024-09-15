using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using Microsoft.Extensions.Options;

namespace IMilosk.Data.FluentMigrator.Utils;

public class CustomConnectionStringAccessor : IConnectionStringAccessor
{
    public CustomConnectionStringAccessor(IConfigureOptions<ProcessorOptions> processorOptions)
    {
    }

    public string ConnectionString { get; set; } = string.Empty;
}