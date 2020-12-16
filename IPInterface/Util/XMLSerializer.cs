using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace PCEFTPOS.EFTClient.IPInterface
{
    public class XMLSerializer
    {
        public static string Serialize<T>(T input)
        {
            var xml = string.Empty;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (StringWriter textWriter = new StringWriter())
                {
                    using (XmlWriter xmlWriter = XmlWriter.Create(textWriter))
                    {
                        serializer.Serialize(xmlWriter, input);
                    }
                    xml = textWriter.ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return xml;
        }

        public static T Deserialize<T>(string input)
        {
            var serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(new System.IO.StringReader(input));
        }
    }
}
