using LitJson;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 序列化和反序列化Json时  使用的是哪种方案
/// </summary>
public enum JsonType
{
    JsonUtlity,
    LitJson,
}

/// <summary>
/// Json数据管理类 主要用于进行 Json的序列化存储到硬盘 和 反序列化从硬盘中读取到内存中
/// </summary>
public class JsonMgr
{
    private static JsonMgr instance = new JsonMgr();
    public static JsonMgr Instance => instance;

    private JsonMgr() { }

    //存储Json数据 序列化
    public void SaveData(object data, string fileName, JsonType type = JsonType.LitJson)
    {
        //确定存储路径
        string path = Application.persistentDataPath + "/" + fileName + ".json";
        //序列化 得到Json字符串
        string jsonStr = "";
        switch (type)
        {
            case JsonType.JsonUtlity:
                jsonStr = JsonUtility.ToJson(data);
                break;
            case JsonType.LitJson:
                jsonStr = JsonMapper.ToJson(data);
                break;
        }
        //把序列化的Json字符串 存储到指定路径的文件中
        File.WriteAllText(path, jsonStr);
    }

    //读取指定文件中的 Json数据 反序列化
    public T LoadData<T>(string fileName, JsonType type = JsonType.LitJson) where T : new()
    {
        //确定从哪个路径读取
        //首先先判断 默认数据文件夹中是否有我们想要的数据 如果有 就从中获取
        string path = Application.streamingAssetsPath + "/" + fileName + ".json";
        //先判断 是否存在这个文件
        //如果不存在默认文件 就从 读写文件夹中去寻找
        if(!File.Exists(path))
            path = Application.persistentDataPath + "/" + fileName + ".json";
        //如果读写文件夹中都还没有 那就返回一个默认对象
        if (!File.Exists(path))
            return new T();

        //进行反序列化
        string jsonStr = File.ReadAllText(path);
        //数据对象
        T data = default(T);
        switch (type)
        {
            case JsonType.JsonUtlity:
                data = JsonUtility.FromJson<T>(jsonStr);
                break;
            case JsonType.LitJson:
                data = JsonMapper.ToObject<T>(jsonStr);
                break;
        }

        //把对象返回出去
        return data;
    }
    /// <summary>
    /// 删除指定 Json 数据文件
    /// </summary>
    public void DeleteData(string fileName)
    {
        string path = Application.persistentDataPath + "/" + fileName + ".json";

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("[JsonMgr] 删除文件成功: " + path);
        }
        else
        {
            Debug.LogWarning("[JsonMgr] 删除失败，文件不存在: " + path);
        }
    }

    /// <summary>
    /// 判断指定 Json 文件是否存在（只检查 persistentDataPath）
    /// </summary>
    public bool HasData(string fileName)
    {
        string path = Application.persistentDataPath + "/" + fileName + ".json";
        return File.Exists(path);
    }


}

/// <summary>
///总结上述功能:
///1.提供了一个Json数据管理类 用于进行Json数据的序列化和反序列化
///2.提供了两种Json方案 供我们选择使用
///3.提供了一个保存数据的方法 传入数据对象和文件名 就可以把数据序列化成Json字符串 存储到指定路径的文件中
///4.提供了一个读取数据的方法 传入文件名 就可以从指定路径的文件中读取Json字符串 反序列化成数据对象返回出去
///5.在读取数据时 会先从默认数据文件夹中寻找 如果没有 就从读写文件夹中寻找 如果还没有 就返回一个默认对象
///6.在保存数据时 会直接存储到读写文件夹中
/// </summary>