using System.Data;
using System.Linq.Expressions;
using SqlSugar;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.IServices.Base;
using ZBlog.Core.Model;
using ZBlog.Core.Repository.Base;

namespace ZBlog.Core.Services.Base
{
    public class BaseService<TEntity> : IBaseService<TEntity> where TEntity : class, new()
    {
        public IBaseRepository<TEntity> BaseDal { get; set; }   //通过在子类的构造函数中注入，这里是基类，不用构造函数

        public BaseService(IBaseRepository<TEntity> baseDal = null)
        {
            BaseDal = baseDal;
        }

        public ISqlSugarClient Db => BaseDal.Db;

        public async Task<TEntity> QueryById(object objId)
        {
            return await BaseDal.QueryById(objId);
        }

        /// <summary>
        /// 根据ID查询一条数据
        /// </summary>
        /// <param name="objId">id（必须指定主键特性 [SugarColumn(IsPrimaryKey=true)]），如果是联合主键，请使用Where条件</param>
        /// <param name="blnUseCache">是否使用缓存</param>
        /// <returns>数据实体</returns>
        public async Task<TEntity> QueryById(object objId, bool blnUseCache = false)
        {
            return await BaseDal.QueryById(objId, blnUseCache);
        }

        /// <summary>
        /// 根据ID查询数据
        /// </summary>
        /// <param name="lstIds">id列表（必须指定主键特性 [SugarColumn(IsPrimaryKey=true)]），如果是联合主键，请使用Where条件</param>
        /// <returns>数据实体列表</returns>
        public async Task<List<TEntity>> QueryByIDs(object[] lstIds)
        {
            return await BaseDal.QueryByIDs(lstIds);
        }

        /// <summary>
        /// 写入实体数据
        /// </summary>
        /// <param name="model">博文实体类</param>
        /// <returns></returns>
        public async Task<long> Add(TEntity model)
        {
            return await BaseDal.Add(model);
        }

        /// <summary>
        /// 批量插入实体(速度快)
        /// </summary>
        /// <param name="listEntity">实体集合</param>
        /// <returns>影响行数</returns>
        public async Task<List<long>> Add(List<TEntity> listEntity)
        {
            return await BaseDal.Add(listEntity);
        }

        /// <summary>
        /// 删除指定ID的数据
        /// </summary>
        /// <param name="id">主键ID</param>
        /// <returns></returns>
        public async Task<bool> DeleteById(object id)
        {
            return await BaseDal.DeleteById(id);
        }

        /// <summary>
        /// 根据实体删除一条数据
        /// </summary>
        /// <param name="model">博文实体类</param>
        /// <returns></returns>
        public async Task<bool> Delete(TEntity model)
        {
            return await BaseDal.Delete(model);
        }

        /// <summary>
        /// 删除指定ID集合的数据(批量删除)
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<bool> DeleteByIds(object[] ids)
        {
            return await BaseDal.DeleteByIds(ids);
        }

        /// <summary>
        /// 更新实体数据
        /// </summary>
        /// <param name="model">博文实体类</param>
        /// <returns></returns>
        public async Task<bool> Update(TEntity model)
        {
            return await BaseDal.Update(model);
        }

        /// <summary>
        /// 更新实体数据
        /// </summary>
        /// <param name="model">博文实体类</param>
        /// <returns></returns>
        public async Task<bool> Update(List<TEntity> model)
        {
            return await BaseDal.Update(model);
        }

        public async Task<bool> Update(TEntity entity, string where)
        {
            return await BaseDal.Update(entity, where);
        }

        public async Task<bool> Update(object operateAnonymousObjects)
        {
            return await BaseDal.Update(operateAnonymousObjects);
        }

        public async Task<bool> Update(TEntity entity, List<string> lstColumns = null, List<string> lstIgnoreColumns = null, string where = "")
        {
            return await BaseDal.Update(entity, lstColumns, lstIgnoreColumns, where);
        }

        /// <summary>
        /// 查询所有数据
        /// </summary>
        /// <returns>数据列表</returns>
        public async Task<List<TEntity>> Query()
        {
            return await BaseDal.Query();
        }

        /// <summary>
        /// 查询数据列表
        /// </summary>
        /// <param name="where">条件</param>
        /// <returns>数据列表</returns>
        public async Task<List<TEntity>> Query(string where)
        {
            return await BaseDal.Query(where);
        }

        /// <summary>
        /// 查询数据列表
        /// </summary>
        /// <param name="whereExpression">whereExpression</param>
        /// <returns>数据列表</returns>
        public async Task<List<TEntity>> Query(Expression<Func<TEntity, bool>> whereExpression)
        {
            return await BaseDal.Query(whereExpression);
        }

        /// <summary>
        /// 查询一个列表
        /// </summary>
        /// <param name="whereExpression">条件表达式</param>
        /// <param name="orderByFields">排序字段，如name asc,age desc</param>
        /// <returns>数据列表</returns>
        public async Task<List<TEntity>> Query(Expression<Func<TEntity, bool>> whereExpression, string orderByFields)
        {
            return await BaseDal.Query(whereExpression, orderByFields);
        }

        /// <summary>
        /// 按照特定列查询数据列表
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public async Task<List<TResult>> Query<TResult>(Expression<Func<TEntity, TResult>> expression)
        {
            return await BaseDal.Query(expression);
        }

        /// <summary>
        /// 按照特定列查询数据列表带条件排序
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="expression">查询实体条件</param>
        /// <param name="whereExpression">过滤条件</param>
        /// <param name="orderByFields">排序条件</param>
        /// <returns></returns>
        public async Task<List<TResult>> Query<TResult>(Expression<Func<TEntity, TResult>> expression, Expression<Func<TEntity, bool>> whereExpression, string orderByFields)
        {
            return await BaseDal.Query(expression, whereExpression, orderByFields);
        }

        /// <summary>
        /// 查询一个列表
        /// </summary>
        /// <param name="whereExpression">条件表达式<</param>
        /// <param name="orderByExpression"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        public async Task<List<TEntity>> Query(Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, object>> orderByExpression, bool isAsc = true)
        {
            return await BaseDal.Query(whereExpression, orderByExpression, isAsc);
        }

        /// <summary>
        /// 查询一个列表
        /// </summary>
        /// <param name="where">条件</param>
        /// <param name="orderByFields">排序字段，如name asc,age desc</param>
        /// <returns>数据列表</returns>
        public async Task<List<TEntity>> Query(string where, string orderByFields)
        {
            return await BaseDal.Query(where, orderByFields);
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">完整的sql语句</param>
        /// <param name="parameters">>参数</param>
        /// <returns>泛型集合</returns>
        public async Task<List<TEntity>> QuerySql(string sql, SugarParameter[] parameters = null)
        {
            return await BaseDal.QuerySql(sql, parameters);
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">完整的sql语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>DataTable</returns>
        public async Task<DataTable> QueryTable(string sql, SugarParameter[] parameters = null)
        {
            return await BaseDal.QueryTable(sql, parameters);
        }

        /// <summary>
        /// 查询前N条数据
        /// </summary>
        /// <param name="whereExpression">条件表达式</param>
        /// <param name="top">前N条</param>
        /// <param name="orderByFields">排序字段，如name asc,age desc</param>
        /// <returns>数据列表</returns>
        public async Task<List<TEntity>> Query(Expression<Func<TEntity, bool>> whereExpression, int top, string orderByFields)
        {
            return await BaseDal.Query(whereExpression, top, orderByFields);
        }

        /// <summary>
        /// 查询前N条数据
        /// </summary>
        /// <param name="where">条件</param>
        /// <param name="top">前N条</param>
        /// <param name="orderByFields">排序字段，如name asc,age desc</param>
        /// <returns>数据列表</returns>
        public async Task<List<TEntity>> Query(string where, int top, string orderByFields)
        {
            return await BaseDal.Query(where, top, orderByFields);
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="whereExpression">条件表达式</param>
        /// <param name="pageIndex">页码（下标0）</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="orderByFields">排序字段，如name asc,age desc</param>
        /// <returns>数据列表</returns>
        public async Task<List<TEntity>> Query(Expression<Func<TEntity, bool>> whereExpression, int pageIndex, int pageSize, string orderByFields)
        {
            return await BaseDal.Query(whereExpression, pageIndex, pageSize, orderByFields);
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="where">条件</param>
        /// <param name="pageIndex">页码（下标0）</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="orderByFields">排序字段，如name asc,age desc</param>
        /// <returns>数据列表</returns>
        public async Task<List<TEntity>> Query(string where, int pageIndex, int pageSize, string orderByFields)
        {
            return await BaseDal.Query(where, pageIndex, pageSize, orderByFields);
        }

        public async Task<PageModel<TEntity>> QueryPage(Expression<Func<TEntity, bool>> whereExpression, int pageIndex = 1, int pageSize = 20, string orderByFields = null)
        {
            return await BaseDal.QueryPage(whereExpression, pageIndex, pageSize, orderByFields);
        }

        public async Task<List<TResult>> QueryMuch<T, T2, T3, TResult>(Expression<Func<T, T2, T3, object[]>> joinExpression, Expression<Func<T, T2, T3, TResult>> selectExpression, Expression<Func<T, T2, T3, bool>> whereLambda = null) where T : class, new()
        {
            return await BaseDal.QueryMuch(joinExpression, selectExpression, whereLambda);
        }

        public async Task<PageModel<TEntity>> QueryPage(PaginationModel pagination)
        {
            var express = DynamicLinqFactory.CreateLambda<TEntity>(pagination.Conditions);

            return await QueryPage(express, pagination.PageIndex, pagination.PageSize, pagination.OrderByFileds);
        }

        #region 分表

        public async Task<TEntity> QueryByIdSplit(object objId)
        {
            return await BaseDal.QueryByIdSplit(objId);
        }

        public async Task<List<long>> AddSplit(TEntity entity)
        {
            return await BaseDal.AddSplit(entity);
        }

        /// <summary>
        /// 根据实体删除一条数据
        /// </summary>
        /// <param name="entity">博文实体类</param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public async Task<bool> DeleteSplit(TEntity entity, DateTime dateTime)
        {
            return await BaseDal.DeleteSplit(entity, dateTime);
        }

        public async Task<bool> UpdateSplit(TEntity entity, DateTime dateTime)
        {
            return await BaseDal.UpdateSplit(entity, dateTime);
        }

        public async Task<PageModel<TEntity>> QueryPageSplit(Expression<Func<TEntity, bool>> whereExpression, DateTime beginTime, DateTime endTime, int pageIndex = 1, int pageSize = 20, string orderByFields = null)
        {
            return await BaseDal.QueryPageSplit(whereExpression, beginTime, endTime, pageIndex, pageSize, orderByFields);
        }

        #endregion
    }
}
