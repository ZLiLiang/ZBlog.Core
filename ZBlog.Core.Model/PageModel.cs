using Mapster;
using MapsterMapper;

namespace ZBlog.Core.Model
{
    /// <summary>
    /// 通用分页信息类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PageModel<T>
    {
        /// <summary>
        /// 当前页标
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 数据总数
        /// </summary>
        public int DataCount { get; set; } = 0;

        /// <summary>
        /// 每页大小
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// 返回数据
        /// </summary>
        public List<T> Data { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int PageCount => (int)Math.Ceiling((decimal)DataCount / PageSize);

        public PageModel() { }

        public PageModel(int pageIndex, int dataCount, int pageSize, List<T> data)
        {
            PageIndex = pageIndex;
            DataCount = dataCount;
            PageSize = pageSize;
            Data = data;
        }

        public PageModel<TOut> ConverTo<TOut>()
        {
            return new PageModel<TOut>(PageIndex, DataCount, PageSize, default);
        }

        public PageModel<TOut> AdaptTo<TOut>()
        {
            var model = ConverTo<TOut>();

            if (Data != null)
            {
                model.Data = Data.Adapt<List<TOut>>();
            }

            return model;
        }

        public PageModel<TOut> AdaptTo<TOut>(TypeAdapterConfig config)
        {
            var model = ConverTo<TOut>();

            if (Data != null)
            {
                model.Data = Data.Adapt<List<TOut>>(config);
            }

            return model;
        }
    }

    /// <summary>
    /// 所需分页参数
    /// </summary>
    public class PaginationModel
    {
        /// <summary>
        /// 当前页
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 每页大小
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// 排序字段(例如:id desc,time asc)
        /// </summary>
        public string OrderByFileds { get; set; }

        /// <summary>
        /// 查询条件( 例如:id = 1 and name = 小明)
        /// </summary>
        public string Conditions { get; set; }
    }

}
