using Script.Mode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Script.Services
{
	public interface IScriptManager
	{
		/// <summary>
		/// 创建新脚本
		/// </summary>
		Ret CreateScript(ref ScriptMetadata metadata, string name);

		/// <summary>
		/// 保存脚本
		/// </summary>
		Ret SaveScript(ScriptMetadata metadata, ref ScriptContent content);

		/// <summary>
		/// 删除脚本
		/// </summary>
		Ret DeleteScript(string name);

		/// <summary>
		/// 获取脚本元数据
		/// </summary>
		Ret GetScriptMetadata(string name, ref ScriptMetadata metadata);

		/// <summary>
		/// 获取脚本内容
		/// </summary>
		Ret GetScriptContent(string name, ref ScriptContent content);

		/// <summary>
		/// 获取所有脚本元数据
		/// </summary>
		Ret GetAllScripts(out List<ScriptMetadata> metadatas);

		/// <summary>
		/// 获取所有脚本内容
		/// </summary>
		Ret GetAllContents(out List<ScriptContent> contents);

		/// <summary>
		/// 按分组获取脚本
		/// </summary>
		Ret GetScriptsByGroup(out Dictionary<string, List<ScriptMetadata>> scripts);

		/// <summary>
		/// 获取分组列表
		/// </summary>
		Ret GetScriptGroups(out List<string> groups);


		/// <summary>
		/// 从文件中加载脚本
		/// </summary>
		Ret LoadScriptFromFile();
	}
}
