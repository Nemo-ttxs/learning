using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.Common.Redis
{
    /// <summary>
    /// redis helper class
    /// </summary>
    public class RedisHelper
    {
        private int DbNum { get; set; }
        private readonly ConnectionMultiplexer _conn;
        /// <summary>
        /// 自定义前缀
        /// </summary>
        public static string CustomKey { get; set; } = "learning:";

        #region 构造函数

        /// <summary>
        /// init <see cref="RedisHelper"/>
        /// </summary>
        /// <param name="dbNum"></param>
        /// <param name="readWriteHosts"></param>
        public RedisHelper(int dbNum, string readWriteHosts)
        {
            DbNum = dbNum;
            _conn = RedisConnectionHelp.GetConnectionMultiplexer(readWriteHosts);
        }

        #region 辅助方法

        private string AddSysCustomKey(string oldKey)
        {
            if (oldKey?.IndexOf("{") > -1)
            {
                return oldKey;
            }
            var prefixKey = CustomKey;
            return prefixKey + oldKey;
        }

        private T Do<T>(Func<IDatabase, T> func)
        {
            var database = _conn.GetDatabase(DbNum);
            return func(database);
        }

        private void Do(Action<IDatabase> action)
        {
            var database = _conn.GetDatabase(DbNum);
            action(database);
        }


        private string ConvertJson<T>(T value)
        {
            var result = value is string ? value.ToString() : JsonConvert.SerializeObject(value);
            return result;
        }

        private T ConvertObj<T>(RedisValue value)
        {
            if (value.IsNullOrEmpty)
            {
#pragma warning disable CS8603 // 可能的 null 引用返回。
                return default;
#pragma warning restore CS8603 // 可能的 null 引用返回。
            }
            return JsonConvert.DeserializeObject<T>(value.ToString());
        }

        private List<T> ConvetList<T>(RedisValue[] values)
        {
            var result = new List<T>();
            foreach (var item in values)
            {
                var model = ConvertObj<T>(item);
                result.Add(model);
            }
            return result;
        }

        /// <summary>
        /// HashEntry[]转list集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        private List<T> ConvetList<T>(HashEntry[] values)
        {
            var result = new List<T>();
            foreach (var item in values)
            {
                var model = ConvertObj<T>(item.Value);
                result.Add(model);
            }
            return result;
        }

        private RedisKey[] ConvertRedisKeys(List<string> redisKeys) => redisKeys.Select(redisKey => (RedisKey)redisKey).ToArray();

        #endregion 辅助方法

        #endregion 构造函数
        #region String

        #region 同步方法

        /// <summary>
        /// 保存单个key value
        /// </summary>
        /// <param name="key">Redis Key</param>
        /// <param name="value">保存的值</param>
        /// <param name="expiry">过期时间</param>
        /// <returns></returns>
        public bool StringSet(string key, string value, TimeSpan? expiry = default)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.StringSet(key, value, expiry));
        }

        /// <summary>
        /// string SetNX
        /// </summary>
        /// <param name="key">Redis Key</param>
        /// <param name="value">保存的值</param>
        /// <param name="expiry">过期时间</param>
        /// <returns></returns>
        public bool StringSetNx(string key, string value, TimeSpan? expiry = default)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.StringSet(key, value, expiry, When.NotExists));
        }

        /// <summary>
        /// 保存多个key value
        /// </summary>
        /// <param name="keyValues">键值对</param>
        /// <returns></returns>
        public bool StringSet(List<KeyValuePair<RedisKey, RedisValue>> keyValues)
        {
            var newkeyValues =
             keyValues.Select(p => new KeyValuePair<RedisKey, RedisValue>(AddSysCustomKey(p.Key), p.Value)).ToList();
            return Do(db => db.StringSet(newkeyValues.ToArray()));
        }


        /// <summary>
        /// 保存一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public bool StringSet<T>(string key, T obj, TimeSpan? expiry = default)
        {
            key = AddSysCustomKey(key);
            var json = ConvertJson(obj);
            return Do(db => db.StringSet(key, json, expiry));
        }

        /// <summary>
        /// 获取单个key的值
        /// </summary>
        /// <param name="key">Redis Key</param>
        /// <returns></returns>
        public string StringGet(string key)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.StringGet(key));
        }

        /// <summary>
        /// 获取多个Key
        /// </summary>
        /// <param name="listKey">Redis Key集合</param>
        /// <returns></returns>
        public RedisValue[] StringGet(List<string> listKey)
        {
            var newKeys = listKey.Select(AddSysCustomKey).ToList();
            return Do(db => db.StringGet(ConvertRedisKeys(newKeys)));
        }

        /// <summary>
        /// 获取一个key的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T StringGet<T>(string key)
        {
            key = AddSysCustomKey(key);
            return Do(db => ConvertObj<T>(db.StringGet(key)));
        }

        /// <summary>
        /// 为数字增长val
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val">可以为负</param>
        /// <returns>增长后的值</returns>
        public double StringIncrement(string key, double val = 1)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.StringIncrement(key, val));
        }

        /// <summary>
        /// 为数字减少val
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val">可以为负</param>
        /// <returns>减少后的值</returns>
        public double StringDecrement(string key, double val = 1)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.StringDecrement(key, val));
        }

        #endregion 同步方法

        #region 异步方法

        /// <summary>
        /// 保存单个key value
        /// </summary>
        /// <param name="key">Redis Key</param>
        /// <param name="value">保存的值</param>
        /// <param name="expiry">过期时间</param>
        /// <returns></returns>
        public Task<bool> StringSetAsync(string key, string value, TimeSpan? expiry = default)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.StringSetAsync(key, value, expiry));
        }

        /// <summary>
        /// 保存多个key value
        /// </summary>
        /// <param name="keyValues">键值对</param>
        /// <returns></returns>
        public Task<bool> StringSetAsync(List<KeyValuePair<RedisKey, RedisValue>> keyValues)
        {
            var newkeyValues =
             keyValues.Select(p => new KeyValuePair<RedisKey, RedisValue>(AddSysCustomKey(p.Key), p.Value)).ToList();
            return Do(db => db.StringSetAsync(newkeyValues.ToArray()));
        }

        /// <summary>
        /// 保存一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public Task<bool> StringSetAsync<T>(string key, T obj, TimeSpan? expiry = default)
        {
            key = AddSysCustomKey(key);
            var json = ConvertJson(obj);
            return Do(db => db.StringSetAsync(key, json, expiry));
        }

        /// <summary>
        /// 获取单个key的值
        /// </summary>
        /// <param name="key">Redis Key</param>
        /// <returns></returns>
        public async Task<string> StringGetAsync(string key)
        {
            key = AddSysCustomKey(key);
            return await Do(db => db.StringGetAsync(key));
        }

        /// <summary>
        /// 获取多个Key
        /// </summary>
        /// <param name="listKey">Redis Key集合</param>
        /// <returns></returns>
        public Task<RedisValue[]> StringGetAsync(List<string> listKey)
        {
            var newKeys = listKey.Select(AddSysCustomKey).ToList();
            return Do(db => db.StringGetAsync(ConvertRedisKeys(newKeys)));
        }

        /// <summary>
        /// 获取多个Key
        /// </summary>
        /// <param name="listKey">Redis Key集合</param>
        /// <returns></returns>
        public async Task<List<T>> StringGetAsync<T>(List<string> listKey)
        {
            var newKeys = listKey.Select(AddSysCustomKey).ToList();
            var values = await Do(db => db.StringGetAsync(ConvertRedisKeys(newKeys)));
            return ConvetList<T>(values);
        }

        /// <summary>
        /// 获取一个key的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T> StringGetAsync<T>(string key)
        {
            key = AddSysCustomKey(key);
            string result = await Do(db => db.StringGetAsync(key));
            return ConvertObj<T>(result);
        }

        /// <summary>
        /// 为数字增长val
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val">可以为负</param>
        /// <returns>增长后的值</returns>
        public Task<double> StringIncrementAsync(string key, double val = 1)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.StringIncrementAsync(key, val));
        }

        /// <summary>
        /// 为数字减少val
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val">可以为负</param>
        /// <returns>减少后的值</returns>
        public Task<double> StringDecrementAsync(string key, double val = 1)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.StringDecrementAsync(key, val));
        }

        #endregion 异步方法

        #endregion String
        #region List

        #region 同步方法

        /// <summary>
        /// 移除指定ListId的内部List的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void ListRemove<T>(string key, T value)
        {
            key = AddSysCustomKey(key);
            _ = Do(db => db.ListRemove(key, ConvertJson(value)));
        }

        /// <summary>
        /// 获取指定key的List
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<T> ListRange<T>(string key)
        {
            key = AddSysCustomKey(key);
            return Do(redis =>
            {
                var values = redis.ListRange(key);
                return ConvetList<T>(values);
            });
        }

        /// <summary>
        /// 获取指定key的List
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public RedisValue[] ListRangeNOConvert<T>(string key)
        {
            key = AddSysCustomKey(key);
            return Do(redis =>
            {
                var values = redis.ListRange(key);
                return values;
            });
        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void ListRightPush<T>(string key, T value)
        {
            key = AddSysCustomKey(key);
            _ = Do(db => db.ListRightPush(key, ConvertJson(value)));
        }

        /// <summary>
        /// 出队
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T ListRightPop<T>(string key)
        {
            key = AddSysCustomKey(key);
            return Do(db =>
            {
                var value = db.ListRightPop(key);
                return ConvertObj<T>(value);
            });
        }

        /// <summary>
        ///  出队
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public RedisValue ListRightPopNoConvert(string key)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.ListRightPop(key));
        }

        /// <summary>
        /// 入栈
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void ListLeftPush<T>(string key, T value)
        {
            key = AddSysCustomKey(key);
            Do(db => db.ListLeftPush(key, ConvertJson(value)));
        }

        /// <summary>
        /// 出栈
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T ListLeftPop<T>(string key)
        {
            key = AddSysCustomKey(key);
            return Do(db =>
            {
                var value = db.ListLeftPop(key);
                if (value.IsNull)
                {
#pragma warning disable CS8603 // 可能的 null 引用返回。
                    return default;
#pragma warning restore CS8603 // 可能的 null 引用返回。
                }
                return ConvertObj<T>(value);
            });
        }

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long ListLength(string key)
        {
            key = AddSysCustomKey(key);
            return Do(redis => redis.ListLength(key));
        }

        #endregion 同步方法

        #region 异步方法

        /// <summary>
        /// 移除指定ListId的内部List的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public async Task<long> ListRemoveAsync<T>(string key, T value)
        {
            key = AddSysCustomKey(key);
            return await Do(db => db.ListRemoveAsync(key, ConvertJson(value)));
        }

        /// <summary>
        /// 获取指定key的List
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<List<T>> ListRangeAsync<T>(string key)
        {
            key = AddSysCustomKey(key);
            var values = await Do(redis => redis.ListRangeAsync(key));
            return ConvetList<T>(values);
        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public Task<long> ListRightPushAsync<T>(string key, T value)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.ListRightPushAsync(key, ConvertJson(value)));
        }

        /// <summary>
        /// 出队
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T> ListRightPopAsync<T>(string key)
        {
            key = AddSysCustomKey(key);
            var value = await Do(db => db.ListRightPopAsync(key));
            return ConvertObj<T>(value);
        }

        /// <summary>
        /// 入栈
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public Task<long> ListLeftPushAsync<T>(string key, T value)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.ListLeftPushAsync(key, ConvertJson(value)));
        }

        /// <summary>
        /// 出栈
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T> ListLeftPopAsync<T>(string key)
        {
            key = AddSysCustomKey(key);
            var value = await Do(db => db.ListLeftPopAsync(key));
            return ConvertObj<T>(value);
        }

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Task<long> ListLengthAsync(string key)
        {
            key = AddSysCustomKey(key);
            return Do(redis => redis.ListLengthAsync(key));
        }

        #endregion 异步方法

        #endregion List
        #region Hash

        #region 同步方法

        /// <summary>
        /// 判断某个数据是否已经被缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public bool HashExists(string key, string dataKey)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.HashExists(key, dataKey));
        }

        /// <summary>
        /// 存储数据到hash表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool HashSet<T>(string key, string dataKey, T t)
        {
            key = AddSysCustomKey(key);
            return Do(db =>
            {
                var json = ConvertJson(t);
                return db.HashSet(key, dataKey, json);
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="entrys"></param>
        /// <param name="timeSpan"></param>
        public void HashSet(string key, HashEntry[] entrys, TimeSpan timeSpan)
        {
            key = AddSysCustomKey(key);
            Do(db =>
            {
                db.HashSet(key, entrys);
                _ = db.KeyExpire(key, timeSpan);
            });
        }

        /// <summary>
        /// 存储数据到hash表
        /// </summary>
        /// <param name="key"></param>
        /// <param name="entry"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public bool HashSet(string key, HashEntry entry, TimeSpan timeSpan)
        {
            key = AddSysCustomKey(key);
            return Do(db =>
            {
                var res = db.HashSet(key, entry.Name, entry.Value);
                _ = db.KeyExpire(key, timeSpan);
                return res;
            });
        }

        /// <summary>
        /// 存储数据到hash表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="t"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public bool HashSet<T>(string key, string dataKey, T t, TimeSpan timeSpan)
        {
            key = AddSysCustomKey(key);
            return Do(db =>
            {
                var json = ConvertJson(t);
                var res = db.HashSet(key, dataKey, json);
                db.KeyExpire(key, timeSpan);
                return res;
            });
        }

        /// <summary>
        /// 移除hash中的某值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public bool HashDelete(string key, string dataKey)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.HashDelete(key, dataKey));
        }

        /// <summary>
        /// 移除hash中的多个值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKeys"></param>
        /// <returns></returns>
        public long HashDelete(string key, List<RedisValue> dataKeys)
        {
            key = AddSysCustomKey(key);
            //List<RedisValue> dataKeys1 = new List<RedisValue>() {"1","2"};
            return Do(db => db.HashDelete(key, dataKeys.ToArray()));
        }

        /// <summary>
        /// 从hash表获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public T HashGet<T>(string key, string dataKey)
        {
            key = AddSysCustomKey(key);
            return Do(db =>
            {
                var value = db.HashGet(key, dataKey);
                return ConvertObj<T>(value);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKeys"></param>
        /// <returns></returns>
        public RedisValue[] HashGet(string key, List<string> dataKeys)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.HashGet(key, dataKeys.Select(t => (RedisValue)t).ToArray()));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public HashEntry[] HashGetAll(string key)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.HashGetAll(key));
        }

        /// <summary>
        /// 获取hash表中对应datakey的数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public Task<RedisValue> HashGetAsync(string key, string dataKey)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.HashGetAsync(key, dataKey));
        }

        /// <summary>
        /// 获取hash表中对应datakey的数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public Task<RedisValue[]> HashGetAsync(string key, RedisValue[] dataKey)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.HashGetAsync(key, dataKey));
        }

        /// <summary>
        /// 获取hash表所有数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Task<HashEntry[]> HashGetAllAsync(string key) => Do(db => db.HashGetAllAsync(key));

        /// <summary>
        /// 从hash表获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<List<T>> HashGetAllAsync<T>(string key)
        {
            key = AddSysCustomKey(key);
            var values = await Do(db => db.HashGetAllAsync(key));
            return ConvetList<T>(values);
        }

        /// <summary>
        /// 为数字增长val
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="val">可以为负</param>
        /// <returns>增长后的值</returns>
        public double HashIncrement(string key, string dataKey, double val = 1)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.HashIncrement(key, dataKey, val));
        }

        /// <summary>
        /// 为数字减少val
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="val">可以为负</param>
        /// <returns>减少后的值</returns>
        public double HashDecrement(string key, string dataKey, double val = 1)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.HashDecrement(key, dataKey, val));
        }

        /// <summary>
        /// 获取hashkey所有Redis key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<T> HashKeys<T>(string key)
        {
            key = AddSysCustomKey(key);
            return Do(db =>
            {
                var values = db.HashKeys(key);
                return ConvetList<T>(values);
            });
        }

        #endregion 同步方法

        #region 异步方法

        /// <summary>
        /// 判断某个数据是否已经被缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public Task<bool> HashExistsAsync(string key, string dataKey)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.HashExistsAsync(key, dataKey));
        }

        /// <summary>
        /// 存储数据到hash表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public Task<bool> HashSetAsync<T>(string key, string dataKey, T t)
        {
            key = AddSysCustomKey(key);
            return Do(db =>
            {
                var json = ConvertJson(t);
                return db.HashSetAsync(key, dataKey, json);
            });
        }

        /// <summary>
        /// 设置hash表的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Task<bool> HashSetAsync(string key, HashEntry value)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.HashSetAsync(key, value.Name, value.Value));
        }

        /// <summary>
        /// 添加多个键值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public Task HashSetAsync(string key, HashEntry[] values) => Do(db => db.HashSetAsync(AddSysCustomKey(key), values));

        /// <summary>
        /// 移除hash中的某值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public Task<bool> HashDeleteAsync(string key, string dataKey)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.HashDeleteAsync(key, dataKey));
        }

        /// <summary>
        /// 移除hash中的多个值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKeys"></param>
        /// <returns></returns>
        public Task<long> HashDeleteAsync(string key, List<RedisValue> dataKeys)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.HashDeleteAsync(key, dataKeys.ToArray()));
        }

        /// <summary>
        /// 从hash表获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public async Task<T> HashGetAsync<T>(string key, string dataKey)
        {
            key = AddSysCustomKey(key);
            var value = await Do(db => db.HashGetAsync(key, dataKey));
            return ConvertObj<T>(value);
        }

        /// <summary>
        /// 为数字增长val
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="val">可以为负</param>
        /// <returns>增长后的值</returns>
        public Task<double> HashIncrementAsync(string key, string dataKey, double val = 1)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.HashIncrementAsync(key, dataKey, val));
        }

        /// <summary>
        /// 为数字减少val
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="val">可以为负</param>
        /// <returns>减少后的值</returns>
        public Task<double> HashDecrementAsync(string key, string dataKey, double val = 1)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.HashDecrementAsync(key, dataKey, val));
        }

        /// <summary>
        /// 获取hashkey所有Redis key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<List<T>> HashKeysAsync<T>(string key)
        {
            key = AddSysCustomKey(key);
            var values = await Do(db => db.HashKeysAsync(key));
            return ConvetList<T>(values);
        }

        /// <summary>
        ///  获取Lua脚本Result
        /// </summary>
        /// <param name="luaScript">脚本</param>
        /// <param name="keys">keys</param>
        /// <param name="values">values</param>
        /// <returns></returns>
        public Task<RedisResult> LuaScriptResultAsync(string luaScript, RedisKey[]? keys = default, RedisValue[]? values = default) => Do(db => db.ScriptEvaluateAsync(luaScript, keys, values));
        #endregion 异步方法

        #endregion Hash
        #region key

        #region 同步方法
        /// <summary>
        /// 删除单个key
        /// </summary>
        /// <param name="key">redis key</param>
        /// <returns>是否删除成功</returns>
        public bool KeyDelete(string key)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.KeyDelete(key));
        }

        /// <summary>
        /// 删除多个key
        /// </summary>
        /// <param name="keys">rediskey</param>
        /// <returns>成功删除的个数</returns>
        public long KeyDelete(List<string> keys)
        {
            var newKeys = keys.Select(AddSysCustomKey).ToList();
            return Do(db => db.KeyDelete(ConvertRedisKeys(newKeys)));
        }

        /// <summary>
        /// 判断key是否存储
        /// </summary>
        /// <param name="key">redis key</param>
        /// <returns></returns>
        public bool KeyExists(string key)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.KeyExists(key));
        }

        /// <summary>
        /// 重新命名key
        /// </summary>
        /// <param name="key">就的redis key</param>
        /// <param name="newKey">新的redis key</param>
        /// <returns></returns>
        public bool KeyRename(string key, string newKey)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.KeyRename(key, newKey));
        }

        /// <summary>
        /// 设置Key的时间
        /// </summary>
        /// <param name="key">redis key</param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public bool KeyExpire(string key, TimeSpan? expiry = default)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.KeyExpire(key, expiry));
        }
        #endregion

        #region 异步方法

        /// <summary>
        /// 删除单个key
        /// </summary>
        /// <param name="key">redis key</param>
        /// <returns>是否删除成功</returns>
        public Task<bool> KeyDeleteAsync(string key)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.KeyDeleteAsync(key));
        }

        /// <summary>
        /// 判断key是否存储
        /// </summary>
        /// <param name="key">redis key</param>
        /// <returns></returns>
        public Task<bool> KeyExistsAsync(string key)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.KeyExistsAsync(key));
        }

        /// <summary>
        /// 设置Key的时间
        /// </summary>
        /// <param name="key">redis key</param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public Task<bool> KeyExpireAsync(string key, TimeSpan? expiry = default)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.KeyExpireAsync(key, expiry));
        }

        #endregion 

        #endregion key

        #region  lua script 
        /// <summary>
        /// lua脚本执行 注：1. redis集群不支持脚本预加载 2.redis集群传递Key需要加上"{}"
        /// </summary>
        /// <param name="script"></param>
        /// <param name="keys"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public Task<RedisResult> ScriptEvaluateAsync(string script, RedisKey[] keys, RedisValue[] values)
        {
            var database = _conn.GetDatabase(DbNum);
            return database.ScriptEvaluateAsync(script, keys, values, CommandFlags.FireAndForget);
        }

        /// <summary>
        /// 加锁
        /// </summary>
        public Task<bool> LockTakeAsync(string key, string data, TimeSpan seconds)
        {
            return Do(db => db.LockTakeAsync(key, data, seconds));
        }

        /// <summary>
        /// 解锁
        /// </summary>
        public Task<bool> LockReleaseAsync(string key, string data)
        {
            return Do(db => db.LockReleaseAsync(key, data));
        }

        #endregion

        #region SortedSet

        #region 同步方法

        /// <summary>
        /// sortedset add entrys
        /// </summary>
        /// <param name="key"></param>
        /// <param name="entrys"></param>
        /// <param name="timeSpan"></param>
        public void SortedSetAdd(string key,
                                 SortedSetEntry[] entrys,
                                 TimeSpan timeSpan) => Do((db) =>
                                 {
                                     _ = db.SortedSetAdd(key, entrys);
                                     _ = db.KeyExpire(key, timeSpan);
                                 });

        /// <summary>
        /// sortedset 获取指定 <strong>[</strong><paramref name="start"/>,<paramref name="end"/><strong>]</strong> 分数区间 的前 <paramref name="take"/> 个元素
        /// </summary>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="order"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        public RedisValue[] SortedGetTop(string key,
                                         double start,
                                         double end,
                                         Order order,
                                         int take) => Do(db => db.SortedSetRangeByScore(key, start, stop: end, order: order, take: take));

        /// <summary>
        /// 删除单个或多个
        /// </summary>
        /// <param name="key">Redis Key</param>
        /// <param name="member">The member to add to the sorted set.</param>
        /// <returns></returns>
        public bool SortedSetRemove(RedisKey key, RedisValue member)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.SortedSetRemove(key, member));
        }

        /// <summary>
        /// 删除单个或多个
        /// </summary>
        /// <param name="key">Redis Key</param>
        /// <param name="members">The member to add to the sorted set.</param>
        /// <returns></returns>
        public long SortedSetRemove(RedisKey key, RedisValue[] members)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.SortedSetRemove(key, members));
        }

        #endregion

        #region 异步方法


        /// <summary>
        /// 保存单个
        /// </summary>
        /// <param name="key">Redis Key</param>
        /// <param name="member">The member to add to the sorted set.</param>
        /// <param name="score">he score for the member to add to the sorted set.</param>
        /// <returns></returns>
        public Task<bool> SortedSetAddAsync(RedisKey key, RedisValue member, double score)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.SortedSetAddAsync(key, member, score));
        }

        /// <summary>
        /// 保存单个
        /// </summary>
        /// <param name="key">Redis Key</param>
        /// <param name="members">The member to add to the sorted set.</param>
        /// <returns></returns>
        public Task<long> SortedSetAddAsync(RedisKey key, SortedSetEntry[] members)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.SortedSetAddAsync(key, members));
        }

        /// <summary>
        /// 获取区间数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        public async Task<RedisValue[]> SortedSetRangeByScoreAsync(RedisKey key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity)
        {
            key = AddSysCustomKey(key);
            var values = await Do(db => db.SortedSetRangeByScoreAsync(key, start, stop));
            return values;
        }


        /// <summary>
        /// Returns the specified range of elements in the sorted set stored at key. By default the elements are considered to be ordered from the lowest to the highest score. Lexicographical order is used for elements with equal score.
        /// Start and stop are used to specify the min and max range for score values. Similar to other range methods the values are inclusive.
        /// </summary>
        /// <param name="key">The key of the sorted set.</param>
        /// <param name="start">The minimum score to filter by.</param>
        /// <param name="stop">The maximum score to filter by.</param>
        /// <param name="exclude">Which of <paramref name="start" /> and <paramref name="stop" /> to exclude (defaults to both inclusive).</param>
        /// <param name="order">The order to sort by (defaults to ascending).</param>
        /// <param name="skip">How many items to skip.</param>
        /// <param name="take">How many items to take.</param>
        /// <param name="flags">The flags to use for this operation.</param>
        /// <returns>List of elements in the specified score range.</returns>
        /// <remarks>https://redis.io/commands/zrangebyscore</remarks>
        /// <remarks>https://redis.io/commands/zrevrangebyscore</remarks>
        public async Task<RedisValue[]> SortedSetRangeByScoreAsync(RedisKey key, long skip, long take, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, Exclude exclude = Exclude.None, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
        {
            key = AddSysCustomKey(key);
            var values = await Do(db =>
                db.SortedSetRangeByScoreAsync(key, start, stop, exclude, order, skip, take, flags));
            return values;
        }

        /// <summary>
        /// 删除单个或多个
        /// </summary>
        /// <param name="key">Redis Key</param>
        /// <param name="members">The member to add to the sorted set.</param>
        /// <returns></returns>
        public async Task<long> SortedSetRemoveAsync(RedisKey key, RedisValue[] members)
        {
            key = AddSysCustomKey(key);
            return await Do(db => db.SortedSetRemoveAsync(key, members));
        }

        /// <summary>
        /// 删除单个或多个
        /// </summary>
        /// <param name="key">Redis Key</param>
        /// <param name="member">The member to add to the sorted set.</param>
        /// <returns></returns>
        public async Task<bool> SortedSetRemoveAsync(RedisKey key, RedisValue member)
        {
            key = AddSysCustomKey(key);
            return await Do(db => db.SortedSetRemoveAsync(key, member));
        }

        #endregion

        #endregion
    }
}
