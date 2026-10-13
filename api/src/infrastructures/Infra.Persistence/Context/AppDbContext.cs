using Core.Library.Abstractions.Interfaces;
using Core.Library.Attributes;
using Infra.Persistence.Context.Conventions;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Infra.Persistence.Context;

internal sealed class AppDbContext(
    DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        ProcessAutoSeedData(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<NullableUtcDateTimeConverter>();
    }

    private static void ProcessAutoSeedData(ModelBuilder modelBuilder)
    {
        List<Type> entityTypes = [.. modelBuilder.Model.GetEntityTypes()
            .Select(x => x.ClrType)
            .Where(x => typeof(IHaveAutoSeedData).IsAssignableFrom(x))];

        foreach (Type entityType in entityTypes)
        {
            List<FieldInfo> fields = [.. entityType
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(p => p.GetCustomAttribute<AutoSeedDataAttribute>() != null)];

            if (fields.Count == 0)
                continue;

            List<object> seedData = [];

            foreach (FieldInfo field in fields)
            {
                object? value = field.GetValue(null);

                if (value != null)
                    seedData.Add(value);
            }

            if (seedData.Count > 0)
                modelBuilder.Entity(entityType).HasData(seedData);
        }
    }
}
