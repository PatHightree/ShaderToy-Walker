using System;
using System.Globalization;
using System.IO;
using IniParser;
using IniParser.Model;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif

namespace Bottle
{
	public sealed class ProjectPrefs
	{
		public const string AssetPath = "/Settings.ini.txt";
		public const string ResourcesPath = "/Resources";
		public const string BuildPath = "/../Builds";

		private static readonly FileIniDataParser parser = new FileIniDataParser();
		private static IniData data = null;
		
		public static IniData Data
		{
			get
			{
				if (data == null)
				{
					var resPath = Application.isEditor ?
						Application.dataPath + ResourcesPath :
						Application.dataPath + "/..";
					var assetPath = resPath + AssetPath;

					if (!Directory.Exists(resPath))
					{
						Directory.CreateDirectory(resPath);
					}

					if (!File.Exists(assetPath))
					{
						var file = File.CreateText(assetPath);
						file.Close();
					}

					data = parser.ReadFile(assetPath);
				}

				return data;
			}
		}

		public static void SetString(string key, string value)
		{
			if (string.IsNullOrEmpty(value))
				return;

			Data.Global[key] = value.Replace("\n", "\\n").Replace("\r", "\\r");
		}

		public static string GetString(string key, string defaultValue = default(string))
		{
			var value = Data.Global[key] ?? defaultValue;
			if (string.IsNullOrEmpty(value))
				return value;

			return value.Replace("\\n", "\n").Replace("\\r", "\r");
		}

		public static void SetInt(string key, int value)
		{
			SetString(key, Convert.ToString(value, CultureInfo.InvariantCulture));
		}

		public static int GetInt(string key, int defaultValue = default(int))
		{
			var value = GetString(key);
			return value == null ? defaultValue : Convert.ToInt32(value);
		}

		public static void SetFloat(string key, float value)
		{
			SetString(key, Convert.ToString(value, CultureInfo.InvariantCulture));
		}

		public static float GetFloat(string key, float defaultValue = default(float))
		{
			var value = GetString(key);
			return value == null ? defaultValue : Convert.ToSingle(value);
		}

		public static void SetBool(string key, bool value)
		{
			SetString(key, Convert.ToString(value, CultureInfo.InvariantCulture));
		}

		public static bool GetBool(string key, bool defaultValue = default(bool))
		{
			var value = GetString(key);
			return value == null ? defaultValue : Convert.ToBoolean(value);
		}

		public static bool HasKey(string key)
		{
			return Data.Global.ContainsKey(key);
		}

		public static void DeleteKey(string key)
		{
			data.Global.RemoveKey(key);
		}

		public static void DeleteAll()
		{
			data.Global.RemoveAllKeys();
		}

		public static void Save()
		{
			parser.SaveFile(Application.dataPath + ResourcesPath + AssetPath, data);
#if UNITY_EDITOR
			UnityEditor.AssetDatabase.Refresh();	
#endif
		}

//#if UNITY_EDITOR
//		[PostProcessScene(-1)]
//		public static void OnPostprocessScene() {
//			string source = Application.dataPath + ResourcesPath + AssetPath;
//			string destination = Application.dataPath + BuildPath + AssetPath;
//			if (File.Exists(destination))
//				File.Delete(destination);
//			File.Copy(source, destination);
//		}
//#endif
	}
}