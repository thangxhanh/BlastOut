#define ATOMIC_WRITE

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Newtonsoft.Json;
using Dacodelaac.DebugUtils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Dacodelaac.DataStorage
{
    public class DataStorage
    {
        string BackupPath => path + "-bak";
        string path;

        PersistentData data;

        public DataStorage(string name)
        {
            path = GetDataPath(name);
            var bakPath = BackupPath;

            if (File.Exists(bakPath) && !File.Exists(path))
            {
                Dacoder.LogErrorFormat("Recover {0} from {1}", path, bakPath);
                File.Move(bakPath, path);
            }

            if (!File.Exists(path))
            {
                data = new PersistentData();
                return;
            }

            try
            {
                using (var stream = new FileStream(path, FileMode.OpenOrCreate))
                {
                    data = Deserialize(stream);
                }
            }
            catch (Exception e)
            {
                Dacoder.LogErrorFormat("Exception deserialize {0}: {1} {2}", path, e.Message, e.StackTrace);
            }

            if (data == null)
            {
                data = new PersistentData();
            }
        }

        public void Save()
        {
            var bakPath = BackupPath;
            var tmpPath = path + "-tmp";

            try
            {
                if (File.Exists(path))
                {
                    if (File.Exists(bakPath)) File.Delete(bakPath);
                    File.Move(path, bakPath);
                }

                using (var stream = new FileStream(tmpPath, FileMode.Create))
                {
                    Serialize(data, stream);
                }

                File.Move(tmpPath, path);
                File.Delete(bakPath);

                Dacoder.LogFormat("Saving {0} successfully", path);
            }
            catch (Exception e)
            {
                Dacoder.LogErrorFormat("Saving {0} error {1} {2}", path, e.Message, e.StackTrace);
                throw;
            }
        }

        string GetDataPath(string name)
        {
            var persistentDataPath = GetPersistentDataPath();
            if (!Directory.Exists(persistentDataPath))
            {
                Directory.CreateDirectory(persistentDataPath);
            }

            return Path.Combine(persistentDataPath, name);
        }

        public static string GetPersistentDataPath()
        {
#if UNITY_EDITOR
            return Path.Combine(Directory.GetParent(Application.dataPath).FullName, "TempDataStorage");
#else
            return Application.persistentDataPath;
#endif
        }

        public object this[string key]
        {
            get => data.TryGetValue(key, out var pd) ? pd : default;
            set => data.Set(key, value);
        }

        public bool ContainsKey(string key)
        {
            return data.ContainsKey(key);
        }

        public void Remove(string key)
        {
            data.Remove(key);
        }

        public void Clear()
        {
            data.Clear();
        }

        static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            /* KHÔNG bật TypeNameHandling: nó nhét tên kiểu .NET vào file, làm sống lại đúng hai
               vấn đề vừa bỏ (đổi tên hay chuyển assembly là hỏng save) và là lỗ hổng thực thi mã. */
            TypeNameHandling = TypeNameHandling.None,
            Formatting = Formatting.None
        };

        void Serialize(object d, Stream stream)
        {
            var json = JsonConvert.SerializeObject(d, JsonSettings);

            /* Ghi thẳng byte, không bọc StreamWriter: writer đóng luôn stream mà nơi gọi mới là chủ. */
            var bytes = new UTF8Encoding(false).GetBytes(json);
            stream.Write(bytes, 0, bytes.Length);
        }

        PersistentData Deserialize(Stream stream)
        {
            /* File JSON mở đầu bằng '{'; BinaryFormatter mở đầu bằng header nhị phân. Nhận dạng
               bằng byte đầu để save của bản cũ vẫn đọc được, rồi tự ghi lại thành JSON. */
            var first = stream.ReadByte();
            if (first < 0) return null;

            stream.Seek(0, SeekOrigin.Begin);

            if (first == '{') return DeserializeJson(stream);

            Dacoder.LogFormat("Save cũ dạng nhị phân — đọc xong sẽ ghi lại thành JSON ở lần Save tới");
            return DeserializeLegacyBinary(stream);
        }

        static PersistentData DeserializeJson(Stream stream)
        {
            using (var reader = new StreamReader(stream, new UTF8Encoding(false), true, 1024, true))
            {
                return JsonConvert.DeserializeObject<PersistentData>(reader.ReadToEnd(), JsonSettings);
            }
        }

        /* Chỉ dùng để đọc save của bản cũ. Đường này đi qua reflection nên có thể hỏng trên
           IL2CPP kèm strip — đúng lý do phải bỏ nó. Mỗi máy chỉ chạy qua đây một lần. */
        static PersistentData DeserializeLegacyBinary(Stream stream)
        {
            var formatter = new BinaryFormatter();
            return formatter.Deserialize(stream) as PersistentData;
        }
        T To<T>(object input, T defaultValue)
        {
            /* Xem chú thích ở PersistentData.To<T> — fast path tránh boxing của Convert.ChangeType. */
            if (input is T direct) return direct;

            var result = defaultValue;

            if (typeof(T).IsEnum)
            {
                result = (T)Enum.ToObject(typeof(T), To(input, Convert.ToInt32(defaultValue)));
            }
            else
            {
                result = (T)Convert.ChangeType(input, typeof(T));
            }

            return result;
        }

        public T Get<T>(string key, T defaultValue = default(T))
        {
            if (data.TryGetValue(key, out var value))
            {
                return To(value, defaultValue);
            }

            return defaultValue;
        }

        public void Set<T>(string key, T value)
        {
            this[key] = value;
        }

        public void Load(IDataPersistent persistentData, bool root = false)
        {
            data.Load(persistentData, root);
        }

        public void Store(IDataPersistent persistentData, bool root = false)
        {
            data.Store(persistentData, root);
        }
    }
}