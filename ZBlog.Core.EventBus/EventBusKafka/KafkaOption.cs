﻿namespace ZBlog.Core.EventBus.EventBusKafka
{
    /// <summary>
    /// Kafka 配置项
    /// </summary>
    public class KafkaOption
    {
        /// <summary>
        /// 连接池数量
        /// </summary>
        public int ConnectionPoolSize { get; set; } = 10;

        /// <summary>
        /// 地址
        /// </summary>
        public string Servers { get; set; }

        /// <summary>
        /// 主题
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// 消费者组Id
        /// </summary>
        public string GroupId { get; set; }

        /// <summary>
        /// 主题分区
        /// </summary>
        public int NumPartitions { get; set; }
    }
}
