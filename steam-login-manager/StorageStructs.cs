using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace steam_login_manager
{
    public struct SteamLogin
    {
        public string Login, Password, Name;
        public DateTime LastAccess;
        public int Count;

        public SteamLogin(string login, string password)
        {
            Login = Name = login;
            Password = password;
            LastAccess = DateTime.Now;
            Count = 0;
        }

        public SteamLogin(string login, string password, string name,
            DateTime last, int count)
        {
            Login = login;
            Password = password;
            Name = name;
            LastAccess = last;
            Count = count;
        }
    }

    public struct LastDatabase
    {
        public string Path;
    }

    public struct SteamLoginDatabase
    {
        public List<SteamLogin> Logins;
        public string SteamPath;
    }

    internal static class SteamLoginLoader
    {
        private static string GetConfigurationPath()
        {
            return Application.UserAppDataPath;
        }

        internal static bool GetLastPath(out string lastPath)
        {
            string path = Path.Combine(GetConfigurationPath(), "last-path.xml");

            if (File.Exists(path))
            {
                try
                {
                    LastDatabase ld = new LastDatabase();

                    XmlSerializer xsr = new XmlSerializer(ld.GetType());
                    XmlReaderSettings xrs = new XmlReaderSettings();
                    xrs.CloseInput = true;
                    using (XmlReader xr = XmlReader.Create(path, xrs))
                    {
                        ld = (LastDatabase)xsr.Deserialize(xr);
                    }

                    lastPath = ld.Path;
                    return true;
                } catch (Exception)
                {
                    lastPath = "";
                    return false;
                }
            }
            else
            {
                lastPath = "";
                return false;
            }
        }

        internal static string GetDefaultDatabase()
        {
            return Path.Combine(GetConfigurationPath(), "default-db.xml");
        }

        internal static bool HaveDefaultDatabase()
        {
            string default_path = Path.Combine(GetConfigurationPath(), "default-db.xml");

            return File.Exists(default_path);
        }

        internal static bool IsValidDatabase(string path, string password,
            out List<SteamLogin> logins, out string steamPath)
        {
            if (!File.Exists(path))
            {
                logins = new List<SteamLogin>();
                steamPath = "";
                return false;
            }

            try
            {
                SteamLoginDatabase sld = new SteamLoginDatabase();

                XmlSerializer xsr = new XmlSerializer(sld.GetType());
                XmlReaderSettings xrs = new XmlReaderSettings();
                xrs.CloseInput = true;
                using (XmlReader xr = XmlReader.Create(path, xrs))
                {
                    sld = (SteamLoginDatabase)xsr.Deserialize(xr);
                }

                steamPath = sld.SteamPath;
                logins = sld.Logins;
            }
            catch (Exception)
            {
                logins = new List<SteamLogin>();
                steamPath = "";
                return false;
            }

            return true;
        }

        internal static void SaveDatabase(string path, string password,
            string steamPath, List<SteamLogin> logins)
        {
            try
            {
                SteamLoginDatabase sld = new SteamLoginDatabase();
                sld.Logins = logins;
                sld.SteamPath = steamPath;

                XmlSerializer xsr = new XmlSerializer(sld.GetType());
                XmlWriterSettings xws = new XmlWriterSettings();
                xws.CloseOutput = true;
                xws.Indent = true;
                xws.IndentChars = "\t";
                using (XmlWriter xw = XmlWriter.Create(path, xws))
                {
                    xsr.Serialize(xw, sld);
                }
            }
            catch (Exception)
            {
            }
        }

        internal static void SaveLastLocation(string databasePath)
        {
            string path = Path.Combine(GetConfigurationPath(), "last-path.xml");

            try
            {
                LastDatabase ld = new LastDatabase();
                ld.Path = databasePath;

                XmlSerializer xsr = new XmlSerializer(ld.GetType());
                XmlWriterSettings xws = new XmlWriterSettings();
                xws.Indent = true;
                xws.IndentChars = "\t";
                xws.CloseOutput = true;
                using (XmlWriter xw = XmlWriter.Create(path, xws))
                {
                    xsr.Serialize(xw, ld);
                }

            } catch (Exception)
            {

            }
        }
    }
}
