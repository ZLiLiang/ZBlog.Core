namespace ZBlog.Core.Model.Models.RootTkey
{
    /// <summary>
    /// Tibug 博文
    /// </summary>
    /// <typeparam name="Tkey"></typeparam>
    public class TopicDetailRoot<Tkey> : RootEntityTkey<Tkey> where Tkey : IEquatable<Tkey>
    {
        public Tkey TopicId { get; set; }
    }
}
