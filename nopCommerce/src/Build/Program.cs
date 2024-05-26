namespace ClearPluginAssemblies;
{
    public class Program
    {
        protected const string FILES_TO_DELETE = "dotnet-bundle.exe;Nop.Web.pdp;Nop.Web.exe;Nop.Web.exe.config";
    
        protected static void Clear(string paths, IList<string> fileNames, bool saveLocalFolders)
        {
            foreach (var pluginPath in paths.Split(';'))
            {
                try
                {
                    var pluginDirectoryInfo = new DirectoryInfo(pluginPath);
                    var allDirectoryInfo = new List<DirectoryInfo> { pluginDirectoryInfo };

                    if (!saveLocalFolders)
                        allDirectoryInfo.AddRange(pluginDirectoryInfo.GetDirectories());

                    foreach (var directoryInfo in allDirectoryInfo)
                    {
                        foreach (var fileName in fileNames)
                        {
                            //delete dll file if it exists in current path
                            var dllfilePath = Path.Combine(directoryInfo.FullName, fileName + ".dll");
                            if (File.Exists(dllfilePath))
                                File.Delete(dllfilePath);
                            //delete pdb file if it exists in current path
                            var pdbfilePath = Path.Combine(directoryInfo.FullName, fileName + ".pdb");
                                if (File.Exists(pdbfilePath))
                                File.Delete(pdbfilePath);
                        }

                        foreach (var fileName in FILES_TO_DELETE.Split(';'))
                        {
                            //delete file if it exists in current path
                            var pdbfilePath = Path.Combine(directoryInfo.FullName, fileName);
                            if (File.Exists(pdbfilePath))
                                File.Delete(pdbfilePath);
                        }

                        if (directoryInfo.GetFiles().Length == 0 && directoryInfo.GetDirectories().Length == 0 && !saveLocalFolders)
                            directoryInfo.Delete(true);
                    }
                }
                catch
                {
                    //do nothing
                }
            }
        }

        public static void Main(string[] args)
        {
            var OutputPath = string.Empty;
            var pluginPaths = string.Empty;
            var saveLocalFolders = true;
            
            var settings = args.FirstOrDefault(a => a.Contains('|'))??string.Empty;
            if(string.IsNullOrEmpty(settings))
                return;

            foreach (var arg in settings.Split('|'))
            {
                var data = arg.Split("=").Select(p => p.Trim()).ToList();

                var name = data[0];
                var value = data.Count > 1 ? data[1] : string.Empty;
                
                switch (name)
                {
                    case "OutputPath":
                        OutputPath = value;
                        break;
                    case "PluginPaths":
                        pluginPaths = value;
                        break;
                    case "SaveLocalFolders":
                        _ = bool.TryParse(value, out saveLocalFolders);
                        break;
                }
            }
            
            if(!Directory.Exists(OutputPath))
                return;

            var di = new DirectoryInfo(OutputPath);
            var separator = Path.DirectorySeparatorChar;
            var folderToIgnore = string.Concat(separator, "Plugins", separator);
            var fileNames = di.GetFiles("*.dll", SearchOption.AllDirectories)
                .Where(fi => !fi.FullName.Contains(folderToIgnore))
                .Select(fi => fi.NameReplace(fi.Extension, "")).ToList();

            if (string.IsNullOrEmpty(pluginPaths) || fileNames.Count == 0)
            {
                return;
            }

            Clear(pluginPaths, fileNames, saveLocalFolders);
        }
    }
}
 
