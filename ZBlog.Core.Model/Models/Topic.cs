using SqlSugar;
using ZBlog.Core.Model.Models.RootTkey;

namespace ZBlog.Core.Model.Models
{
    /// <summary>
    /// Tibug 类别
    /// </summary>
    public class Topic : RootEntityTkey<long>
    {
        public Topic()
        {
            TopicDetail = new List<TopicDetail>();
            UpdateTime = DateTime.Now;
        }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string Logo { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string Name { get; set; }

        [SugarColumn(Length = 400, IsNullable = true)]
        public string Detail { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string Author { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string SectendDetail { get; set; }

        public bool IsDelete { get; set; }
        public int Read { get; set; }
        public int Commend { get; set; }
        public int Good { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }

        [SugarColumn(IsIgnore = true)]
        public virtual ICollection<TopicDetail> TopicDetail { get; set; }
    }
}
