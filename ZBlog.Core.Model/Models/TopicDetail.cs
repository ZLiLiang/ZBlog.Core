using SqlSugar;
using ZBlog.Core.Model.Models.RootTkey;

namespace ZBlog.Core.Model.Models
{
    /// <summary>
    /// Tibug 博文
    /// </summary>
    public class TopicDetail : TopicDetailRoot<long>
    {
        public TopicDetail()
        {
            UpdateTime = DateTime.Now;
        }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string Logo { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string Name { get; set; }

        [SugarColumn(Length = 2000, IsNullable = true)]
        public string Content { get; set; }

        [SugarColumn(Length = 2000, IsNullable = true)]
        public string Detail { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string SectendDetail { get; set; }

        public bool IsDelete { get; set; } = false;
        public int Read { get; set; }
        public int Commend { get; set; }
        public int Good { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public int Top { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string Author { get; set; }


        [SugarColumn(IsIgnore = true)]
        public virtual Topic Topic { get; set; }

    }
}
