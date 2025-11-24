using CubArt.Domain.Common;
using CubArt.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace CubArt.Infrastructure.Extentions
{
    internal static class PropertyBuilderExtensions
    {
        public static void HasCreatedDateEntity<T>(this EntityTypeBuilder<T> builder)
            where T : class, IHasCreatedDate
            => builder.PropertyWithUnderscore(x => x.DateCreated)
                .HasDateTimeConversion()
                .HasColumnType(SqlColumnTypes.TimeStampWithTimeZone)
                .HasDefaultValueSql("now() at time zone 'utc'");

        public static PropertyBuilder<DateTime> HasDateTimeConversion(this PropertyBuilder<DateTime> builder)
            => builder.HasConversion(
                v => v,
                v => v.ToUniversalTime());

        public static PropertyBuilder<DateTime?> HasDateTimeConversion(this PropertyBuilder<DateTime?> builder)
            => builder.HasConversion(
                v => v,
                v => v.HasValue ? v.Value.ToUniversalTime() : null);


        #region Naming Column and Table

        public static void HasBaseEntityInt<T>(this EntityTypeBuilder<T> builder)
            where T : Entity<int>
        {
            builder.HasKey(x => x.Id);
            builder.PropertyWithUnderscore(x => x.Id).HasColumnNameUnderscoreStyle(nameof(Entity<int>.Id));
        }

        public static void HasBaseEntityLong<T>(this EntityTypeBuilder<T> builder)
            where T : Entity<long>
        {
            builder.HasKey(x => x.Id);
            builder.PropertyWithUnderscore(x => x.Id).HasColumnNameUnderscoreStyle(nameof(Entity<long>.Id));
        }

        public static void HasBaseEntityGuid<T>(this EntityTypeBuilder<T> builder)
            where T : Entity<Guid>
        {
            builder.HasKey(x => x.Id);
            builder.PropertyWithUnderscore(x => x.Id).HasColumnNameUnderscoreStyle(nameof(Entity<Guid>.Id));
        }

        public static EntityTypeBuilder<T> HasTableNameUnderscoreStyle<T>(this EntityTypeBuilder<T> builder, string fieldName) where T : class
            => builder.ToTable(fieldName.ToLowerCaseWithUnderscore());

        public static EntityTypeBuilder<T> HasViewNameUnderscoreStyle<T>(this EntityTypeBuilder<T> builder, string fieldName) where T : class
            => builder.ToView(fieldName.ToLowerCaseWithUnderscore());

        public static PropertyBuilder<T> HasColumnNameUnderscoreStyle<T>(this PropertyBuilder<T> builder, string fieldName)
            => builder.HasColumnName(fieldName.ToLowerCaseWithUnderscore());

        public static string ToLowerCaseWithUnderscore(this string text) => Regex
            .Replace(text, "(?<=[^_])[A-Z]", result => '_' + result.ToString().ToLower())
            .ToLower(); // на всякий случай второе приведение для других символов из строки (для всех которые -> _A || Test || Ta)

        public static PropertyBuilder<TProperty> PropertyWithUnderscore<TEntity, TProperty>(
            this EntityTypeBuilder<TEntity> builder,
            Expression<Func<TEntity, TProperty>> propertyExpression) where TEntity : class =>
            builder
                .Property(propertyExpression)
                .HasColumnNameUnderscoreStyle(propertyExpression.GetMemberAccess().Name);

        public static IndexBuilder HasIndexWithUnderscore<TEntity, TProperty>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, TProperty>> propertyExpression,
        string? customName = null) where TEntity : class
        {
            var propertyName = propertyExpression.GetMemberAccess().Name;
            var indexName = customName ?? $"idx_{typeof(TEntity).Name.ToLowerCaseWithUnderscore()}_{propertyName.ToLowerCaseWithUnderscore()}";

            // Преобразуем Expression<Func<TEntity, TProperty>> в Expression<Func<TEntity, object>>
            var convertedExpression = Expression.Lambda<Func<TEntity, object>>(
                Expression.Convert(propertyExpression.Body, typeof(object)),
                propertyExpression.Parameters
            );

            return builder.HasIndex(convertedExpression)
                .HasDatabaseName(indexName);
        }



        #endregion
    }
}
