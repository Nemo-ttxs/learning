namespace Microsoft.Extensions.Configuration.Consul
{
    /// <summary>
    /// 配置解析选择器
    /// </summary>
    public interface IConsulConfigurationParserSelector
    {
        /// <summary>
        /// 选择配置解析器
        /// </summary>
        /// <param name="relativePath">配置元素相对于 <see cref="ConsulConfigurationSource.ConsulKeyPath"/> 的路径，去除了 / 前导符号</param>
        /// <returns></returns>
        IConsulConfigurationParser Select(string relativePath);

        /// <summary>
        /// 为指定的配置元素设置解析器
        /// </summary>
        /// <param name="relativePath">配置元素相对于 <see cref="ConsulConfigurationSource.ConsulKeyPath"/> 的路径，去除了 / 前导符号</param>
        /// <param name="parser">解析器</param>
        void SetParser(string relativePath, IConsulConfigurationParser parser);

        /// <summary>
        /// 为指定后缀设置解析器
        /// </summary>
        /// <param name="extension">后缀，以点开头哦，忽略大小写</param>
        /// <param name="parser">解析器</param>
        void SetParserForExtension(string extension, IConsulConfigurationParser parser);
    }
}