using Core.Domain.Entities.System.OutboxMessages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infra.Persistence.Configurations.System;

internal sealed class OutboxMessageConfig : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.Property(c => c.Type)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(c => c.Content)
            .HasMaxLength(100000)
            .IsRequired();

        builder.Property(c => c.OccurredOnUtc)
            .IsRequired();

        builder.Property(c => c.ProcessedOnUtc)
            .IsRequired(false);

        builder.Property(c => c.Error)
            .IsRequired(false);
    }
}
