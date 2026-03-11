using UnityEngine;

//资源加载器接口 定义了加载资源的方法 让不同的加载器实现这个接口 来实现不同的加载方式
public interface IResLoader
{
    T Load<T>(string path) where T : Object;
}
