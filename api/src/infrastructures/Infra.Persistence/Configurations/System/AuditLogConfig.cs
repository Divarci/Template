using Core.Domain.Entities.System.AuditLogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infra.Persistence.Configurations.System;

internal sealed class AuditLogConfig : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.Property(c => c.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(c => c.EntityId)
            .IsRequired();

        builder.Property(c => c.CreatedOnUtc)
            .IsRequired();

        builder.HasIndex(c => new { c.EntityId, c.CreatedOnUtc })
            .HasDatabaseName("IX_AuditLog_EntityId_CreatedOnUtc");
    }
}
