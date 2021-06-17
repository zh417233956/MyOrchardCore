using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace FunTest1.Controllers
{
    [Route("api/[controller]/[action]")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public ActionResult Index()
        {
            var guid = Guid.NewGuid().ToString();
            var result = new { code = 200, msg = "", uuid = guid };
            #region  StackExchange.Redis
            try
            {
                RedisHelper.Set<string>("text", "123456789");
                var valueStr = RedisHelper.Get<string>("text");

                result = new { code = 200, msg = valueStr, uuid = guid };
            }
            catch (Exception ex)
            {
                result = new { code = 403, msg = ex.ToString(), uuid = guid };
            }
            #endregion

            return Content(JsonConvert.SerializeObject(result));
        }
    }

    public static class RedisHelper
    {
        private static string Constr = "";
        private static string PreMergeKey = "";

        private static object _locker = new Object();
        private static ConnectionMultiplexer _instance = null;

        static RedisHelper()
        {

            var ConfigRedis = "192.168.4.144:6379,DefaultDatabase=0";
            var PreMergeKey = "test";
            SetCon(ConfigRedis, PreMergeKey);
        }


        /// <summary>
        /// ʹ��һ����̬���������������ӵ�ʵ��������������ʾ��������һ�� ConnectionMultiplexer �Ͽ����ӣ�����Գ�ʼ���µ�����ʵ����
        /// </summary>
        public static ConnectionMultiplexer Instance
        {
            get
            {
                if (Constr.Length == 0)
                {
                    throw new Exception("�����ַ���δ���ã�");
                }
                if (_instance == null)
                {
                    lock (_locker)
                    {
                        if (_instance == null || !_instance.IsConnected)
                        {
                            _instance = ConnectionMultiplexer.Connect(Constr);
                        }
                    }
                }
                return _instance;
            }
        }

        public static void SetCon(string config)
        {
            Constr = config;
        }
        public static void SetCon(string config, string mergeKey)
        {
            Constr = config;
            PreMergeKey = mergeKey;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IDatabase GetDatabase()
        {
            return Instance.GetDatabase();
        }

        /// <summary>
        /// ����� MergeKey ����ƴ�� Key ��ǰ׺�����岻ͬ��ҵ��ģ��ʹ�ò�ͬ��ǰ׺��
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string MergeKey(string key)
        {
            //return key;
            return PreMergeKey + key;
        }

        /// <summary>
        /// ����key��ȡ�������
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Get<T>(string key)
        {
            key = MergeKey(key);
            return Deserialize<T>(GetDatabase().StringGet(key));
        }

        /// <summary>
        /// ����key��ȡ�������
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static object Get(string key)
        {
            key = MergeKey(key);
            return Deserialize<object>(GetDatabase().StringGet(key));
        }
        /// <summary>
        /// ���û���
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expireMinutes"></param>
        public static void Set<T>(string key, T value, int expireMinutes = 0)
        {
            key = MergeKey(key);
            if (expireMinutes > 0)
            {
                GetDatabase().StringSet(key, Serialize(value), TimeSpan.FromMinutes(expireMinutes));
            }
            else
            {
                GetDatabase().StringSet(key, Serialize(value));
            }

        }

        private static T Deserialize<T>(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return default(T);
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(str);
        }

        /// <summary>
        /// ���л�����
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private static string Serialize<T>(T o)
        {
            if (o == null)
            {
                return null;
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(o);
        }
    }
}
