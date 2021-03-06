﻿using System;
using System.IO;
using System.Xml;
using DynamoServices;

namespace Asana
{
    public static class Authentication
    {
        public static string DefaultXMLPath => Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDi‌​rectory, "keys.xml"));
        public static string APItoken { get; private set; }

        public static bool GetTokenFromXMLFile(string filePath)
        {
            XmlDocument xml = new XmlDocument();

            if(string.IsNullOrEmpty(filePath) || string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException("Supplied filepath is empty or null.");
            if (!File.Exists(filePath)) throw new FileNotFoundException("Could not locate settings file to read API key at :" + Environment.NewLine + filePath);
            try
            {
                xml.Load(filePath);
                APItoken = xml.SelectSingleNode("Asana/token").InnerText;
                return true;
            }
            catch (Exception)
            {
                throw new Exception("Could not locate the token in the XML file. Make sure it is under <Asana>" + Environment.NewLine +
                    "   <token>TOKEN HERE</token>" + Environment.NewLine +
                    "</Asana>");
            }
        }
    }
}
