using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace WebAppForMORecSys.Data
{
    /// <summary>
    /// Extension class for DbSet
    /// </summary>
    public static class DbSetExtensions
    {
        /// <summary>
        /// Adds entity to the database if not already exists
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="dbSet">database set of this type of entity</param>
        /// <param name="entity">One entity of type T</param>
        /// <param name="predicate">Predicate that checks if the entity is not already in the database</param>
        /// <returns></returns>
        public static EntityEntry<T>? AddIfNotExists<T>(this DbSet<T> dbSet, T entity, Expression<Func<T, bool>> predicate = null) where T : class, new()
        {
            var exists = predicate != null ? dbSet.Any(predicate) : dbSet.Any();
            return !exists ? dbSet.Add(entity) : null;
        }
        
    }
}
