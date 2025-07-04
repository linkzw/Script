using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Script.Utils
{
	internal class Utils
	{

		public static T DeepCopy<T>(T obj)
		{
			if (obj == null)
			{
				return obj;
			}
			var type = obj.GetType();
			if (obj is string || type.IsValueType)
			{
				return obj;
			}

			var result = Activator.CreateInstance(type);
			var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
			foreach (var field in fields)
			{
				field.SetValue(result, field.GetValue(obj));
			}
			return (T)result;
		}

		public static bool SaveModel<T>(T model, string path, bool IsAbsoulte = true)
		{
			string absoultePath = IsAbsoulte ? path : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
			try
			{
				MakeDir(absoultePath);
				JsonSerializerOptions options = new JsonSerializerOptions
				{
					Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
					WriteIndented = true
				};
				string jsonData = System.Text.Json.JsonSerializer.Serialize(model, options);
				//string jsonData = JsonConvert.SerializeObject(model, Formatting.Indented, jsonSetting);
				using (StreamWriter sw = new StreamWriter(absoultePath, false))
				{
					sw.Write(jsonData);
				}
				return true;
			}
			catch (Exception ex)
			{
				//logService.Error($"保存配置失败,Type:{typeof(T).Name}", ex);
				return false;
			}
		}

		public static T LoadModel<T>(string path, bool IsAbsoulte = true, T? defaultT = null) where T : class
		{
			string absoultePath = IsAbsoulte ? path : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
			try
			{
				MakeDir(absoultePath);

				if (File.Exists(absoultePath))
				{
					using (StreamReader reader = new StreamReader(absoultePath))
					{
						string data = reader.ReadToEnd();
						T result = System.Text.Json.JsonSerializer.Deserialize<T>(data);
						//T result = JsonConvert.DeserializeObject<T>(data, jsonSetting);
						return result;
					}
				}
				else
				{
					var t = defaultT ?? default(T);  //如果文件不存在，自动添加默认设置
					if (t != null)
					{
						File.WriteAllText(absoultePath, System.Text.Json.JsonSerializer.Serialize(t));
						//File.WriteAllText(absoultePath, JsonConvert.SerializeObject(t));
					}
					return t;
				}
			}
			catch (Exception ex)
			{
				//logService.Error($"加载配置失败,Type:{typeof(T).Name}", ex);
				return default;
			}
		}

		private static void MakeDir(string fullFileName)
		{
			var fileInfo = new FileInfo(fullFileName);
			if (!fileInfo.Directory.Exists) fileInfo.Directory.Create(); //文件夹不存在则自动创建，否则会报错
		}
	 	public static string ComputeSHA256Hash(string input)
		{
			using (System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create())
			{
				byte[] inputBytes = Encoding.UTF8.GetBytes(input);
				byte[] hashBytes = sha256.ComputeHash(inputBytes);

				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < hashBytes.Length; i++)
				{
					sb.Append(hashBytes[i].ToString("x2"));
				}
				return sb.ToString();
			}
		}

		public object CreateInstance(string typeName)
		{
			Type type = GetTypeforName(typeName);
			if (type == null)
				throw new ArgumentException($"类型 '{typeName}' 未找到");

			return Activator.CreateInstance(type);
		}

		public static Type GetTypeforName(string typeName)
		{
			Type type = null;
			Assembly[] assemblyArray = AppDomain.CurrentDomain.GetAssemblies();
			int assemblyArrayLength = assemblyArray.Length;
			for (int i = 0; i < assemblyArrayLength; ++i)
			{
				type = assemblyArray[i].GetType(typeName);
				if (type != null)
				{
					return type;
				}
			}

			for (int i = 0; (i < assemblyArrayLength); ++i)
			{
				Type[] typeArray = assemblyArray[i].GetTypes();
				int typeArrayLength = typeArray.Length;
				for (int j = 0; j < typeArrayLength; ++j)
				{
					if (typeArray[j].Name.Equals(typeName))
					{
						return typeArray[j];
					}
				}
			}
			return type;
		}

	}
}
