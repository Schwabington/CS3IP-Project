using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace CybSDK
{
    public class CDataManager
    {

        const string cDataPath = "/cyberith_data.dat";

        public static void WriteToFile(string virtualizerName)
        {
            string destination = Application.persistentDataPath + cDataPath;
            FileStream file;

            if (File.Exists(destination)) file = File.OpenWrite(destination);
            else file = File.Create(destination);

            CData data = new CData(virtualizerName);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, data);
            file.Close();
        }

        public static string ReadFile()
        {
            string destination = Application.persistentDataPath + cDataPath;
            FileStream file;

            if (File.Exists(destination))
                file = File.OpenRead(destination);
            else
                return null;

            BinaryFormatter bf = new BinaryFormatter();
            CData data = (CData)bf.Deserialize(file);
            file.Close();

            return data.virtualizerName;
        }
    }
}