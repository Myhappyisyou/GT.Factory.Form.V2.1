using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace GT_Common.Helper
{
    public static class SerializeHelper
    {
        #region 泛型xml序列化
        /// <summary>
        /// 序列化为XML文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="filename"></param>
        public static void SaveXml<T>(T obj, string filename)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                XmlSerializer xmlS = new XmlSerializer(typeof(T));
                xmlS.Serialize(fs, obj);
                fs.Flush();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                fs?.Close();
            }
        }

        public static void SaveXmlEncode<T>(T obj, string filename)
        {
            // 注册编码提供程序（.NET Core/.NET 5+需要）
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // 创建使用 GB2312 编码的 XmlWriterSettings
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Encoding = Encoding.GetEncoding("GB2312"),
                Indent = true,
                OmitXmlDeclaration = false // 确保包含XML声明
            };

            // 使用 using 语句确保资源正确释放
            using (XmlWriter writer = XmlWriter.Create(filename, settings))
            {
                XmlSerializer xmlS = new XmlSerializer(typeof(T));
                xmlS.Serialize(writer, obj);
            }
        }

        /// <summary>
        /// 从XML文件反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T LoadXml<T>(string path)
        {
            try
            {
                using (StreamReader reader = File.OpenText(path))
                {
                    XmlSerializer xmlS = new XmlSerializer(typeof(T));
                    return (T)xmlS.Deserialize(reader);
                }
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region 泛型二进制序列化
        /// <summary>
        /// 序列化为二进制文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="filename"></param>
        public static void SaveBinary<T>(T obj, string filename)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, obj);
                fs.Flush();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                fs?.Close();
            }
        }

        /// <summary>
        /// 从二进制文件反序列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T LoadBinary<T>(string path)
        {
            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    return (T)formatter.Deserialize(stream);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

    }
}
