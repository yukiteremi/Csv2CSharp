using System;
using Excel;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
public class CSVEditor : EditorWindow
{
    [MenuItem("Tools/Csv生成c#代码")]
    static void CreateCSharpCode()
    {
        CSVEditor window = EditorWindow.CreateWindow<CSVEditor>();
        window.Show();
        window.Init();
    }

    public void Init()
    {
        biaoti = @"/************************************************************************
该文件是通过自动生成的，禁止手动修改
作者：#2
日期：#1
*************************************************************************/";
    }

    public string PathFileName;
    public string Aurthor="";
    public string biaoti = "";
    public DefaultAsset SelectPath;
    /// <summary>
    /// 读取配置文件
    /// </summary>
    /// <param name="path">文件路径</param>
    void ReaderConfigFile(string path,string JsonFileName)
    {
        string[] fileStr = File.ReadAllLines(path);
        PathFileName = path;
        //UnityEngine.Debug.Log(fileStr);
        CreateCS(fileStr, JsonFileName);
    }

    void CreateCS(string[] reflectFileName,string JsonFileName)
    {
        /************ 写入配置路径位置与创建的文件写入流 ************/
        string CSPath = "";
        if (SelectPath==null)
        {
            CSPath = $"{Application.dataPath + "/Resources"}/{JsonFileName}.cs";
        }
        else
        {
            CSPath = $"{Application.dataPath.Replace("Assets", "")+ AssetDatabase.GetAssetPath(SelectPath)}/{JsonFileName}.cs";
        }
        StreamWriter sw = new StreamWriter(CSPath);

        /************ 设置一些写入的格式符与变量 ************/
        //写入的行以\为换行符  \t==tab
        string tabKey = "\t";
        //参数类型
        string[] argumentType = reflectFileName[1].Split(',');
        //参数名称
        string[] argumentName = reflectFileName[0].Split(',');

        //string[] argumentList = reflectFileName[1].Split(',');

        string time = DateTime.Now.ToString();
        sw.WriteLine(biaoti.Replace("#1", time).Replace("#2", Aurthor));
        sw.WriteLine(GetImport());
        /************ 正式在配置流文件里开始写入代码配置 ************/
        sw.WriteLine($"public class {JsonFileName}");
        sw.WriteLine("{");
        //遍历参数列表，生成配置
        for (int i = 0; i < argumentType.Length; i++)
        {
            sw.WriteLine($"{tabKey}public {argumentType[i]} {argumentName[i]};");
        }

        sw.WriteLine("}");

        //生成解析csv文件函数
        sw.WriteLine($"public class JsonToCsv{JsonFileName}");
        sw.WriteLine("{");

        sw.WriteLine($"{tabKey}public List<{JsonFileName}> {JsonFileName}_list = new List<{JsonFileName}>();");
        sw.WriteLine($"{tabKey}public void JsonToCsvOpen()");
        sw.WriteLine($"{tabKey}" + "{");
        sw.WriteLine($"{tabKey}{tabKey}string json = \"{PathFileName}\";");
        sw.WriteLine($"{tabKey}{tabKey}string[] fileStr = File.ReadAllLines(json);");
        sw.WriteLine($"{tabKey}{tabKey}for (int i = 2; i < fileStr.Length; i++)" + "{");
        sw.WriteLine($"{tabKey}{tabKey}{tabKey}string[] list_open = fileStr[i].Split(',');");
        sw.WriteLine($"{tabKey}{tabKey}{tabKey}{JsonFileName} jsons = new {JsonFileName}();");
        for (int i = 0; i < argumentType.Length; i++)
        {
            //当前不同类型表头定义不同类型
            if (argumentType[i] == "int")
            {
                sw.WriteLine($"{tabKey}{tabKey}{tabKey}jsons.{argumentName[i]} = int.Parse(list_open[{i}]);");
            }
            else if (argumentType[i] == "string")
            {
                sw.WriteLine($"{tabKey}{tabKey}{tabKey}jsons.{argumentName[i]} = list_open[{i}];");
            }
            else if (argumentType[i] == "float")
            {
                sw.WriteLine($"{tabKey}{tabKey}{tabKey}jsons.{argumentName[i]} = float.Parse(list_open[{i}]);");
            }
        }
        sw.WriteLine($"{tabKey}{tabKey}{tabKey}{JsonFileName}_list.Add(jsons);" + "}");

        sw.WriteLine($"{tabKey}" + "}");

        //生成调用数据并返回集合
        sw.WriteLine($"{tabKey}public List<{JsonFileName}> data()" + "{");
        sw.WriteLine($"{tabKey}{tabKey}if({JsonFileName}_list.Count== 0)  ");
        sw.WriteLine($"{tabKey}{tabKey}JsonToCsvOpen();");
        sw.WriteLine($"{tabKey}return {JsonFileName}_list;" + "}");

        sw.WriteLine("");
        sw.WriteLine($"{tabKey}public void AddData({JsonFileName} data)" + "{");
        sw.WriteLine($"{tabKey}{tabKey}{JsonFileName}_list.Add(data);");
        sw.WriteLine($"{tabKey}" + "}");
        sw.WriteLine("");

        sw.WriteLine($"{tabKey}public {JsonFileName} TryGetDataByIndex(int index)" + "{");
        sw.WriteLine($"{tabKey}{tabKey}if({JsonFileName}_list[index]!=null)  ");
        sw.WriteLine($"{tabKey}{tabKey}return {JsonFileName}_list[index];");
        sw.WriteLine($"{tabKey}return null;" + "}");

        sw.WriteLine($"{tabKey}public {JsonFileName} TryGetDataByValue( {JsonFileName} data)" + "{");
        sw.WriteLine($"{tabKey}foreach (var item in {JsonFileName}_list)"+"{ ");
        sw.WriteLine($"{tabKey}{tabKey}if (item==data)");
        sw.WriteLine($"{tabKey}{tabKey}return item;");
        sw.WriteLine($"{tabKey}" + "}");
        sw.WriteLine($"{tabKey}return null;" + "}");

        sw.WriteLine("}");

        sw.Flush();
        sw.Close();
        AssetDatabase.Refresh();
        //UnityEngine.Debug.Log(CSPath);
        Process.Start(CSPath);
    }
    /// <summary>
    /// 加载调用数据
    /// </summary>
    /// <returns></returns>
    string GetImport()
    {
        string importStr = null;
        importStr += $"using UnityEngine;\r\n";
        importStr += $"using UnityEngine.UI;\r\n";
        importStr += $"using System;\r\n";
        importStr += $"using System.Collections;\r\n";
        importStr += $"using UnityEditor;\r\n";
        importStr += $"using System.IO;\r\n";
        importStr += $"using System.Collections.Generic;\r\n";
        return importStr;
    }



    TextAsset text;
    private void OnGUI()
    {
        GUILayout.Label("程序说明：");
        GUILayout.Label("本代码对CSV文件要求很高，需要注意以下几点：");
        GUILayout.Label("0.首先你选择的文件一定要是csv文件，并不是你随便txt写点东西后缀改成csv，就代表这东西是csv了");
        GUILayout.Label("1.CSV文件的编码格式应为UTF-8格式，否则自动生成的代码可能带有乱码");
        GUILayout.Label("2.CSV文件首行应为属性名称，第二行为属性类型（例如int，string）等，可以无视大小写，否则自动生成的类型会错误！");
        GUILayout.Label("3.生成的文件默认保存在Resources文件夹下，如需改动请在选择2中选择文件夹，如未创建Resources文件夹可能会发生报错！");
        GUILayout.Label("4.生成的文件名为：选择的csv文件名字+AutoCreate");
        GUILayout.Label("5.");
        GUILayout.Label("");
        GUILayout.BeginHorizontal("BOX");
        GUILayout.Label("作者：");
        Aurthor = GUILayout.TextField(Aurthor);
        EditorGUILayout.EndHorizontal();
        GUILayout.Label("请选择一个CSV文件");
        text = EditorGUILayout.ObjectField(text,typeof(TextAsset),false) as TextAsset;
        GUILayout.Label("请选择保存到的文件夹");
        DefaultAsset Floder= EditorGUILayout.ObjectField(SelectPath, typeof(DefaultAsset), false) as DefaultAsset;
        if (Floder!= SelectPath)
        {
            SelectPath = Floder;
            if (AssetDatabase.GetAssetPath(SelectPath).Contains("."))
            {
                SelectPath = null;
                UnityEngine.Debug.Log("请选择文件夹");
            }
        }
        if (GUILayout.Button("选择好了"))
        {
            if (text==null)
            {
                UnityEngine.Debug.Log("不是还没选择CSV文件吗");
            }
            else
            {
                string path = AssetDatabase.GetAssetPath(text);
                if (path.EndsWith(".csv"))
                {
                    ReaderConfigFile(path,text.name+"AutoCreate");
                    AssetDatabase.Refresh();
                }
                else
                {
                    UnityEngine.Debug.Log("请选择CSV文件！");
                }
            }
        }
    }
}
