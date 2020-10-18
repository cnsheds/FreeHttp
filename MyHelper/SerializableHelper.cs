﻿using FreeHttp.FiddlerHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace FreeHttp.MyHelper
{
    
    class SerializableHelper
    {
        public static void SerializeRuleList(ListView requestRuleListView, ListView reponseRuleListView)
        {
            string rulePath = "FreeHttp\\RuleData.xml";
            if (requestRuleListView != null && reponseRuleListView!=null)
            {
                //dynamic
                List<FiddlerRequestChange> requestList = new List<FiddlerRequestChange>();
                List<FiddlerResponseChange> responseList = new List<FiddlerResponseChange>();
                foreach (ListViewItem tempItem in requestRuleListView.Items)
                {
                    requestList.Add((FiddlerRequestChange)tempItem.Tag);
                }
                foreach (ListViewItem tempItem in reponseRuleListView.Items)
                {
                    responseList.Add((FiddlerResponseChange)tempItem.Tag);
                }
                //Stream stream = File.Open("data.xml", FileMode.Create);
                TextWriter writer = new StreamWriter(rulePath, false);
                XmlSerializer serializer = new XmlSerializer(typeof(FiddlerModificHttpRuleCollection));
                //serializer = new XmlSerializer(typeof(List<IFiddlerHttpTamper>));
                serializer.Serialize(writer, new FiddlerModificHttpRuleCollection(requestList, responseList));
                writer.Close();

                //写入版本
                System.Xml.XmlDocument xmlDocument = new System.Xml.XmlDocument();
                xmlDocument.Load(rulePath);
                System.Xml.XmlAttribute dbAtt = xmlDocument.CreateAttribute("ruleVersion");
                dbAtt.Value = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                xmlDocument.SelectSingleNode("/FiddlerModificHttpRuleCollection")?.Attributes.Append(dbAtt);
                xmlDocument.Save(rulePath);
            }
        }

        public static FiddlerModificHttpRuleCollection DeserializeRuleList()
        {
            string rulePath = "FreeHttp\\RuleData.xml";
            FiddlerModificHttpRuleCollection fiddlerModificHttpRuleCollection = null;
            if (File.Exists(rulePath))
            {
                FileStream myFileStream = new FileStream(rulePath, FileMode.Open);
                try
                {
                    using (System.Xml.XmlTextReader reader = new System.Xml.XmlTextReader(myFileStream))
                    {
                        reader.Normalization = false;
                        //版本控制
                        string ruleVersion = string.Empty;
                        //System.Version version = new Version("2.0.0");
                        while (reader.Read())
                        {
                            if (reader.NodeType == System.Xml.XmlNodeType.Element)
                            {
                                if (reader.Name == "FiddlerModificHttpRuleCollection")
                                {
                                    ruleVersion = reader.GetAttribute("ruleVersion");
                                    break;
                                }
                            }
                        }
                        if (string.IsNullOrEmpty(ruleVersion) || ruleVersion[0] == '1')
                        {
                            File.Copy(rulePath, rulePath + ".oldVersion", true);
                            XmlSerializer mySerializer = new XmlSerializer(typeof(FreeHttp.FiddlerHelper.VersionControlV1.FiddlerModificHttpRuleCollection));
                            fiddlerModificHttpRuleCollection = (FiddlerModificHttpRuleCollection)(FreeHttp.FiddlerHelper.VersionControlV1.FiddlerModificHttpRuleCollection)mySerializer.Deserialize(reader);
                        }
                        else if(ruleVersion[0]=='2')
                        {
                            XmlSerializer mySerializer = new XmlSerializer(typeof(FiddlerModificHttpRuleCollection));
                            fiddlerModificHttpRuleCollection = (FiddlerModificHttpRuleCollection)mySerializer.Deserialize(reader);
                        }
                        else
                        {
                            throw new Exception("unkonw ruleVersion",new Exception("this freehttp can not analysis the rule file , you should updata your freehttp"));
                        }
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show(string.Format("{0}\r\n{1}\r\nyour error rule file will back up in {2}", ex.Message, ex.InnerException == null ? "" : ex.InnerException.Message, Directory.GetCurrentDirectory() + rulePath + ".lastErrorFile"), "load user rule fail");
                    File.Copy(rulePath, rulePath+".lastErrorFile", true);
                }
                finally
                {
                    myFileStream.Close();
                }
            }
            return fiddlerModificHttpRuleCollection;
        }



        /// <summary>
        ///[Serializable] 标记类 （公共成员默认被序列化）
        ///[NonSerialized] 标记不需要序列化的成员 (只对终端field有效 ， 属性可以使用[System.Xml.Serialization.XmlIgnore])
        /// Serializable 需要空参数的构造函数 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="modificSettingInfo"></param>
        /// <param name="filePath"></param>
        public static void SerializeData<T>(T modificSettingInfo, string filePath)
        {
            if (modificSettingInfo != null)
            {
                TextWriter writer = new StreamWriter(filePath, false);
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                //serializer = new XmlSerializer(typeof(List<IFiddlerHttpTamper>));
                serializer.Serialize(writer, modificSettingInfo);
                writer.Close();
            }
        }

        public static T DeserializeData<T>(string filePath)
        {
            T modificSettingInfo = default(T); //对于数值类型会返回零。 对于结构，此关键字将返回初始化为零或 null 的每个结构成员，具体取决于这些结构是值类型还是引用类型,对于数值类型会返回零。 对于结构，此关键字将返回初始化为零或 null 的每个结构成员，具体取决于这些结构是值类型还是引用类型
            if (File.Exists(filePath))
            {
                XmlSerializer mySerializer = new XmlSerializer(typeof(T));
                FileStream myFileStream = new FileStream(filePath, FileMode.Open);
                try
                {
                    //modificSettingInfo = (T)mySerializer.Deserialize(myFileStream);    //默认使用XmlReader （ It doesn't have a property for controlling normalization, as the XmlTextReader does.） 导致\r\n被反序列化为\n
                    using (System.Xml.XmlTextReader reader = new System.Xml.XmlTextReader(myFileStream))
                    {
                        reader.Normalization = false;
                        modificSettingInfo = (T)mySerializer.Deserialize(reader);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("{0}\r\n{1}", ex.Message, ex.InnerException == null ? "" : ex.InnerException.Message), "DeserializeData fail");
                    File.Copy(filePath, string.Format("{0}.lastErrorFile", filePath), true);
                    modificSettingInfo = default(T);
                }
                finally
                {
                    myFileStream.Close();
                }
            }
            return modificSettingInfo;
        }


        /// <summary>
        /// 『DataMemberAttribute Class』   
        /// 使用 [DataContract()] 标记class  
        /// 【如果要使用[Serializable] 默认序列化公开字段及属性，且要求其有公开的Set,用[DataContract]指没有这个限制，使用 [DataMember(Name = "ID")] / [DataMember]  标记成员】
        /// 使用 [DataMember(Name = "ID")] / [DataMember]  标记成员
        /// 并且不要求成员访问修饰符为public
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializeClass"></param>
        /// <param name="filePath"></param>
        public static void SerializeContractData<T>(T serializeClass, string filePath)
        {
            if (serializeClass != null)
            {
                FileStream fs = new FileStream(filePath, FileMode.Create);
                System.Xml.XmlDictionaryWriter writer = System.Xml.XmlDictionaryWriter.CreateTextWriter(fs);
                System.Runtime.Serialization.DataContractSerializer ser = new System.Runtime.Serialization.DataContractSerializer(typeof(T));
                ser.WriteObject(writer, serializeClass);
                writer.Close();
                fs.Close();
            }
        }

        public static T DeserializeContractData<T>(string filePath)
        {
            T serializeClass = default(T); //对于数值类型会返回零。 对于结构，此关键字将返回初始化为零或 null 的每个结构成员，具体取决于这些结构是值类型还是引用类型,对于数值类型会返回零。 对于结构，此关键字将返回初始化为零或 null 的每个结构成员，具体取决于这些结构是值类型还是引用类型
             if (File.Exists(filePath))
             {
                 FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate);
                 DataContractSerializer ser = new DataContractSerializer(typeof(T));
                 try
                 {
                     serializeClass = (T)ser.ReadObject(fs);
                 }
                 catch (Exception ex)
                 {
                     MessageBox.Show(string.Format("{0}\r\n{1}", ex.Message, ex.Message, ex.InnerException == null ? "" : ex.InnerException.Message), "DeserializeContractData Fail");
                     File.Copy(filePath, string.Format("{0}.lastErrorFile", filePath), true);
                     serializeClass = default(T);
                 }
                 finally
                 {
                     fs.Close();
                 }
                 
             }
             return serializeClass;
        }

    }

}
