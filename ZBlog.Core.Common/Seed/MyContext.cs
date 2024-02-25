using SqlSugar;
using ZBlog.Core.Common.DB;

namespace ZBlog.Core.Common.Seed
{
    public class MyContext
    {
        private static MutiDBOperate _connectObject => GetMainConnectionDb();
        private static string _connectionString = _connectObject.Connection;
        private static DbType _dbType = (DbType)_connectObject.DbType;
        private SqlSugarScope _db;
        public static string ConnId = _connectObject.ConnId;

        /// <summary>
        /// 连接字符串 
        /// Blog.Core
        /// </summary>
        /// <returns></returns>
        public static MutiDBOperate GetMainConnectionDb()
        {
            var mainConnectDb = BaseDBConfig.MutiConnectionString.allDbs.Find(x => x.ConnId == MainDb.CurrentDbConnId);
            if (BaseDBConfig.MutiConnectionString.allDbs.Count > 0)
            {
                if (mainConnectDb == null)
                    mainConnectDb = BaseDBConfig.MutiConnectionString.allDbs[0];
            }
            else
            {
                throw new Exception("请确保appsettigns.json中配置连接字符串,并设置Enabled为true;");
            }

            return mainConnectDb;
        }

        /// <summary>
        /// 连接字符串 
        /// Blog.Core
        /// </summary>
        public static string ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }

        /// <summary>
        /// 数据库类型 
        /// Blog.Core 
        /// </summary>
        public static DbType DbType
        {
            get { return _dbType; }
            set { _dbType = value; }
        }

        /// <summary>
        /// 数据连接对象 
        /// Blog.Core 
        /// </summary>
        public SqlSugarScope Db
        {
            get { return _db; }
            private set { _db = value; }
        }

        /// <summary>
        /// 功能描述:构造函数
        /// 作　　者:Blog.Core
        /// </summary>
        public MyContext(ISqlSugarClient sqlSugarClient)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new ArgumentNullException("数据库连接字符串为空");

            _db = sqlSugarClient as SqlSugarScope;

        }


        #region 实例方法

        /// <summary>
        /// 功能描述:获取数据库处理对象
        /// 作　　者:Blog.Core
        /// </summary>
        /// <returns>返回值</returns>
        public SimpleClient<T> GetEntityDB<T>() where T : class, new()
        {
            return new SimpleClient<T>(_db);
        }

        #endregion

        #region 根据实体类生成数据库表

        /// <summary>
        /// 功能描述:根据实体类生成数据库表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="blnBackupTable">是否备份表</param>
        /// <param name="lstEntitys">指定的实体</param>
        public void CreateTableByEntity<T>(bool blnBackupTable, params T[] lstEntitys)
            where T : class, new()
        {
            Type[] lstTypes = null;
            if (lstEntitys != null)
            {
                lstTypes = new Type[lstEntitys.Length];
                for (int i = 0; i < lstEntitys.Length; i++)
                {
                    T t = lstEntitys[i];
                    lstTypes[i] = typeof(T);
                }
            }
            CreateTableByEntity(blnBackupTable, lstTypes);
        }

        /// <summary>
        /// 功能描述:根据实体类生成数据库表
        /// </summary>
        /// <param name="blnBackupTable">是否备份表</param>
        /// <param name="lstEntitys">指定的实体</param>
        public void CreateTableByEntity(bool blnBackupTable, params Type[] lstEntitys)
        {
            if (blnBackupTable)
                _db.CodeFirst.BackupTable().InitTables(lstEntitys);
            else
                _db.CodeFirst.InitTables(lstEntitys);
        }

        #endregion

        #region 静态方法

        /// <summary>
        /// 功能描述:设置初始化参数
        /// </summary>
        /// <param name="strConnectionString">连接字符串</param>
        /// <param name="enmDbType">数据库类型</param>
        public static void Init(string strConnectionString, DbType enmDbType = SqlSugar.DbType.SqlServer)
        {
            _connectionString = strConnectionString;
            _dbType = enmDbType;
        }

        /// <summary>
        /// 功能描述:创建一个链接配置
        /// </summary>
        /// <param name="blnIsAutoCloseConnection">是否自动关闭连接</param>
        /// <param name="blnIsShardSameThread">是否夸类事务</param>
        /// <returns>ConnectionConfig</returns>
        public static ConnectionConfig GetConnectionConfig(bool blnIsAutoCloseConnection = true, bool blnIsShardSameThread = false)
        {
            ConnectionConfig config = new ConnectionConfig()
            {
                ConnectionString = _connectionString,
                DbType = _dbType,
                IsAutoCloseConnection = blnIsAutoCloseConnection,
                ConfigureExternalServices = new ConfigureExternalServices()
                {
                    //DataInfoCacheService = new HttpRuntimeCache()
                },
                //IsShardSameThread = blnIsShardSameThread
            };
            return config;
        }

        /// <summary>
        /// 功能描述:获取一个自定义的DB
        /// </summary>
        /// <param name="config">config</param>
        /// <returns>返回值</returns>
        public static SqlSugarScope GetCustomDB(ConnectionConfig config)
        {
            return new SqlSugarScope(config);
        }

        /// <summary>
        /// 功能描述:获取一个自定义的数据库处理对象
        /// </summary>
        /// <param name="sugarClient">sugarClient</param>
        /// <returns>返回值</returns>
        public static SimpleClient<T> GetCustomEntityDB<T>(SqlSugarScope sugarClient) where T : class, new()
        {
            return new SimpleClient<T>(sugarClient);
        }

        /// <summary>
        /// 功能描述:获取一个自定义的数据库处理对象
        /// </summary>
        /// <param name="config">config</param>
        /// <returns>返回值</returns>
        public static SimpleClient<T> GetCustomEntityDB<T>(ConnectionConfig config) where T : class, new()
        {
            SqlSugarScope sugarClient = GetCustomDB(config);
            return GetCustomEntityDB<T>(sugarClient);
        }

        #endregion
    }
}
