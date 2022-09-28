namespace BDUtil.Library
{

    public class DefaultKey : Key
    {
        static DefaultKey _main;
        public static DefaultKey main => _main ??= AssetBundle.main.GetOrCreate(_main);
    }
}