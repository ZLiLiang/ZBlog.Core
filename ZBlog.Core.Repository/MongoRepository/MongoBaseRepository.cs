using MongoDB.Bson;
using MongoDB.Driver;

namespace ZBlog.Core.Repository.MongoRepository
{
    public class MongoBaseRepository<TEntity> : IMongoBaseRepository<TEntity> where TEntity : class, new()
    {
        private readonly MongoDbContext _context;

        public MongoBaseRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(TEntity entity)
        {
            await _context.Db.GetCollection<TEntity>(typeof(TEntity).Name)
                .InsertOneAsync(entity);
        }

        public async Task<TEntity> GetAsync(int Id)
        {
            var filter = Builders<TEntity>.Filter.Eq("Id", Id);

            return await _context.Db.GetCollection<TEntity>(typeof(TEntity).Name)
                .Find(filter)
                .FirstOrDefaultAsync();
        }

        public async Task<List<TEntity>> GetListAsync()
        {
            return await _context.Db.GetCollection<TEntity>(typeof(TEntity).Name)
                .Find(new BsonDocument())
                .ToListAsync();
        }

        public async Task<TEntity> GetByObjectIdAsync(string Id)
        {
            var filter = Builders<TEntity>.Filter.Eq("_id", ObjectId.Parse(Id));

            return await _context.Db.GetCollection<TEntity>(typeof(TEntity).Name)
                .Find(filter)
                .FirstOrDefaultAsync();
        }

        public async Task<List<TEntity>> GetListFilterAsync(FilterDefinition<TEntity> filter)
        {
            return await _context.Db.GetCollection<TEntity>(typeof(TEntity).Name)
                .Find(filter)
                .ToListAsync();
        }
    }
}
