// 使用枚举来表示资源加载的方式，可以根据实际需求选择不同的加载方式，例如在编辑器中使用AssetDatabase，在运行时使用Resources或AssetBundle。
public enum LoadMode
{
    AssetDatabase,
    Resources,
    AssetBundle
}
