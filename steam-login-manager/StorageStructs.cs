using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
        public bool HideInTray;
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
            out List<SteamLogin> logins, out string steamPath, out bool hide)
        {
            if (!File.Exists(path))
            {
                logins = new List<SteamLogin>();
                steamPath = "";
                hide = true;

                return false;
            }

            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    byte[] prologue = new byte[]
                    {
                        // SLM0001
                        83, 76, 77, 1, 0, 0, 0
                    };

                    byte[] readed = new byte[7];
                    fs.Read(readed, 0, 7);

                    if (!prologue.SequenceEqual(readed))
                    {
                        logins = new List<SteamLogin>();
                        steamPath = "";
                        hide = true;

                        return false;
                    }

                    SteamLoginDatabase sld = new SteamLoginDatabase();

                    RijndaelManaged rm = new RijndaelManaged();
                    using (CryptoStream cs = new CryptoStream(fs,
                        rm.CreateDecryptor(CreateKeyFrom(password), GetIV()),
                        CryptoStreamMode.Read))
                    {

                        XmlSerializer xsr = new XmlSerializer(sld.GetType());
                        XmlReaderSettings xrs = new XmlReaderSettings();
                        xrs.CloseInput = false;
                        using (XmlReader xr = XmlReader.Create(cs, xrs))
                        {
                            sld = (SteamLoginDatabase)xsr.Deserialize(xr);
                        }

                        steamPath = sld.SteamPath;
                        logins = sld.Logins;
                        hide = sld.HideInTray;
                    }
                }
            }
            catch (Exception)
            {
                logins = new List<SteamLogin>();
                steamPath = "";
                hide = true;
                return false;
            }

            return true;
        }

        internal static void SaveDatabase(string path, string password,
            string steamPath, List<SteamLogin> logins, bool hide)
        {
            try
            {
                SteamLoginDatabase sld = new SteamLoginDatabase();
                sld.Logins = logins;
                sld.SteamPath = steamPath;
                sld.HideInTray = hide;

                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    fs.Write("SLM");
                    fs.Write(1);

                    RijndaelManaged rm = new RijndaelManaged();
                    using (CryptoStream cs = new CryptoStream(fs,
                        rm.CreateEncryptor(CreateKeyFrom(password), GetIV()),
                        CryptoStreamMode.Write))
                    {

                        XmlSerializer xsr = new XmlSerializer(sld.GetType());
                        XmlWriterSettings xws = new XmlWriterSettings();
                        xws.Indent = true;
                        xws.IndentChars = "\t";
                        xws.CloseOutput = false;
                        using (XmlWriter xw = XmlWriter.Create(cs, xws))
                        {
                            xsr.Serialize(xw, sld);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private static byte[] CreateKeyFrom(string password)
        {
            var staticBytes = new byte[32] { 118, 123, 23, 17, 161, 152, 35, 68,
                126, 213, 16, 115, 68, 217, 58, 108, 56, 218, 5, 78, 28, 128,
                113, 208, 61, 56, 10, 87, 187, 162, 233, 38 };
            var bytes = Encoding.UTF8.GetBytes(password);

            if (bytes.Length > 32)
                return bytes.Take(32).ToArray();
            else if (bytes.Length < 32)
            {
                var lst = new List<byte>(bytes);
                lst.AddRange(staticBytes);
                return lst.Take(32).ToArray();
            }

            return bytes;
        }

        private static byte[] GetIV()
        {
            return new byte[16] { 33, 241, 14, 16, 103, 18, 14, 248, 4, 54, 18,
                5, 60, 76, 16, 191 };
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
