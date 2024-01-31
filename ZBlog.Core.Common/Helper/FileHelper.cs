using System.Text;

namespace ZBlog.Core.Common.Helper
{
    public class FileHelper : IDisposable
    {
        private bool _alreadyDispose = false;

        #region 构造函数

        public FileHelper()
        {

        }

        ~FileHelper()
        {
            Dispose();
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (_alreadyDispose) return;
            _alreadyDispose = true;
        }

        #endregion

        #region IDisposable 成员

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region 取得文件后缀名

        /// <summary>
        /// 取后缀名
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <returns>.gif|.html格式</returns>
        public static string GetPostfixStr(string filename)
        {
            int start = filename.LastIndexOf(".");
            int length = filename.Length;
            string postfix = filename.Substring(start, length - start);
            return postfix;
        }

        #endregion

        #region 根据文件大小获取指定前缀的可用文件名

        /// <summary>
        /// 根据文件大小获取指定前缀的可用文件名
        /// </summary>
        /// <param name="folderPath">文件夹</param>
        /// <param name="prefix">文件前缀</param>
        /// <param name="size">文件大小(1m)</param>
        /// <param name="ext">文件后缀(.log)</param>
        /// <returns></returns>
        public static string GetAvailableFileWithPrefixOrderSize(string folderPath, string prefix, int size = 1 * 1024 * 1024, string ext = ".log")
        {
            var allFiles = new DirectoryInfo(folderPath);
            var selectFiles = allFiles.GetFiles()
                .Where(file => file.Name.ToLower().Contains(prefix.ToLower()) && file.Extension.ToLower() == ext.ToLower() && file.Length < size)
                .OrderByDescending(file => file.Name)
                .ToList();

            if (selectFiles.Count > 0)
            {
                return selectFiles.FirstOrDefault().FullName;
            }

            return Path.Combine(folderPath, $@"{prefix}_{DateTime.Now.DateToTimeStamp()}.log");
        }
        public static string GetAvailableFileNameWithPrefixOrderSize(string _contentRoot, string prefix, int size = 1 * 1024 * 1024, string ext = ".log")
        {
            var folderPath = Path.Combine(_contentRoot, "Log");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var allFiles = new DirectoryInfo(folderPath);
            var selectFiles = allFiles.GetFiles()
                .Where(file => file.Name.Contains(prefix, StringComparison.CurrentCultureIgnoreCase) && file.Extension.Equals(ext, StringComparison.CurrentCultureIgnoreCase) && file.Length < size)
                .OrderByDescending(file => file.Name)
                .ToList();

            if (selectFiles.Count > 0)
            {
                return selectFiles.FirstOrDefault().Name.Replace(".log", "");
            }

            return $@"{prefix}_{DateTime.Now.DateToTimeStamp()}";
        }

        #endregion

        #region 写文件

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="strings">文件内容</param>
        public static void WriteFile(string path, string strings)
        {
            if (!File.Exists(path))
            {
                FileStream fileStream = File.Create(path);
                fileStream.Close();
            }

            StreamWriter streamWriter = new StreamWriter(path, false, Encoding.GetEncoding("gb2312"));
            streamWriter.Write(strings);
            streamWriter.Close();
            streamWriter.Dispose();
        }

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="buf">文件内容</param>
        public static void WriteFile(string path, byte[] buf)
        {
            //if (!File.Exists(path))
            //{
            //    FileStream fileStream = File.Create(path);
            //    fileStream.Close();
            //}

            //FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            //stream.Write(buf, 0, buf.Length);
            //stream.Close();
            //stream.Dispose();

            using FileStream stream = File.Open(path, FileMode.Create, FileAccess.Write);
            stream.Write(buf, 0, buf.Length);
        }

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="strings">文件内容</param>
        /// <param name="encode">编码格式</param>
        public static void WriteFile(string path, string strings, Encoding encode)
        {
            using StreamWriter streamWriter = new(path, false, encode);
            streamWriter.Write(strings);
        }

        #endregion

        #region 读文件

        /// <summary>
        /// 读文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public static string ReadFile(string path)
        {
            if (!File.Exists(path))
            {
                return "不存在相应的目录";
            }
            else
            {
                using StreamReader streamReader = new StreamReader(path, Encoding.GetEncoding("gb2312"));
                var result = streamReader.ReadToEnd();
                return result;
            }
        }

        /// <summary>
        /// 读文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="encode">编码格式</param>
        /// <returns></returns>
        public static string ReadFile(string path, Encoding encode)
        {
            if (!File.Exists(path))
            {
                return "不存在相应的目录";
            }
            else
            {
                StreamReader streamReader = new StreamReader(path, encode);
                var result = streamReader.ReadToEnd();
                streamReader.Close();
                streamReader.Dispose();
                return result;
            }
        }

        #endregion

        #region 追加文件

        /// <summary>
        /// 追加文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="content">内容</param>
        public static void FileAdd(string path, string content)
        {
            StreamWriter streamWriter = File.AppendText(path);
            streamWriter.Write(content);
            streamWriter.Flush();
            streamWriter.Close();
        }

        #endregion

        #region 拷贝文件

        /// <summary>
        /// 拷贝文件
        /// </summary>
        /// <param name="orignFile">原始文件</param>
        /// <param name="newFile">新文件路径</param>
        public static void FileCoppy(string orignFile, string newFile)
        {
            File.Copy(orignFile, newFile, true);
        }

        #endregion

        #region 删除文件

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="path">文件路径</param>
        public static void FileDelete(string path)
        {
            File.Delete(path);
        }

        #endregion

        #region 移动文件

        /// <summary>
        /// 移动文件
        /// </summary>
        /// <param name="orignFile">原始路径</param>
        /// <param name="newFile">新文件路径</param>
        public static void FileMove(string orignFile, string newFile)
        {
            File.Move(orignFile, newFile);
        }

        #endregion

        #region 在当前目录下创建目录

        /// <summary>
        /// 在当前目录下创建目录
        /// </summary>
        /// <param name="orignFolder">当前目录</param>
        /// <param name="newFolder">新目录</param>
        public static void FolderCreate(string orignFolder, string newFolder)
        {
            Directory.SetCurrentDirectory(orignFolder);
            Directory.CreateDirectory(newFolder);
        }

        #endregion

        #region 递归删除文件夹目录及文件

        /// <summary>
        /// 递归删除文件夹目录及文件
        /// </summary>
        /// <param name="folder">文件夹目录</param>
        public static void FolderDelete(string folder)
        {
            if (Directory.Exists(folder))   //如果存在这个文件夹删除之
            {
                foreach (var item in Directory.GetFileSystemEntries(folder))
                {
                    if (File.Exists(item))
                        File.Delete(item);  //直接删除其中的文件
                    else
                        FolderDelete(item); //递归删除子文件夹
                }
                Directory.Delete(folder);   //删除已空文件夹
            }
        }

        #endregion

        #region 将指定文件夹下面的所有内容copy到目标文件夹下面 果目标文件夹为只读属性就会报错。

        /// <summary>
        /// 指定文件夹下面的所有内容copy到目标文件夹下面
        /// </summary>
        /// <param name="srcPath">原始文件夹路径</param>
        /// <param name="destPath">目标文件夹路径</param>
        public static void FolderCopy(string srcPath, string destPath)
        {
            try
            {
                // 检查目标目录是否以目录分割字符结束如果不是则添加之
                if (destPath[destPath.Length - 1] != Path.DirectorySeparatorChar)
                    destPath += Path.DirectorySeparatorChar;

                // 判断目标目录是否存在如果不存在则新建之
                if (!Directory.Exists(destPath))
                    Directory.CreateDirectory(destPath);

                // 得到源目录的文件列表，该里面是包含文件以及目录路径的一个数组
                //如果你指向copy目标文件下面的文件而不包含目录请使用下面的方法
                string[] files = Directory.GetFiles(srcPath);
                //遍历所有的文件和目录
                foreach (var file in files)
                {
                    //先当作目录处理如果存在这个目录就递归Copy该目录下面的文件
                    if (Directory.Exists(file))
                        FolderCopy(file, destPath + Path.GetFileName(file));
                    //否则直接Copy文件
                    else
                        File.Copy(file, destPath + Path.GetFileName(file), true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        #endregion
    }
}
