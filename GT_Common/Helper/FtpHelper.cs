using GT_Common.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace GT_Common.Helper
{
    public  class FtpHelper
    {
        //服务器IP
        string ftpSreverIP;
        //服务器路径
        string ftpemotePath;
        //用户名
        static string ftpUserID= CommonConfig.FtpUserName;
        //密码
        static string ftpPassword= CommonConfig.FtpPassWord;

        //FTP路径地址
        string ftpURI;
                
        public FtpHelper(string FtpServerIP, string FtpRemotePath, string FtpUseID, string FtpPassword)
        {
            ftpSreverIP = FtpServerIP;
            ftpemotePath = FtpRemotePath;
            ftpUserID = FtpUseID;
            ftpPassword = FtpPassword;

            ftpURI = "FTP://" + FtpServerIP+ "/" + FtpRemotePath + "/";
        }

        public static bool FtpFileExistsInFolder(string ftpFolderPath, string targetFileName, string ftpUsername, string ftpPassword)
        {
            try
            {
                // Create an FTP request for the directory listing
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpFolderPath);
                request.Method = WebRequestMethods.Ftp.ListDirectory;

                // Provide credentials
                request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

                // Get the response from the FTP server
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string line = null;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // Check if the target file exists in the directory listing
                        if (line.Equals(targetFileName, StringComparison.OrdinalIgnoreCase))
                        {
                            return true; // File found
                        }
                    }
                }

                return false; // File not found
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response != null && response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    // Folder not available
                    return false;
                }
                throw; // Re-throw any other exceptions
            }
        }

        public static bool CheckFileInCurrentAndPreviousDay(string ftpBasePath, string targetFileName, string ftpUsername, string ftpPassword,out string ftpPath)
        {
            // Get current and previous date
            DateTime currentDate = DateTime.Now;
            string currentDateFolder = ftpBasePath + currentDate.ToString("yyyy-MM-dd") + "/";
            string previousDateFolder = ftpBasePath + currentDate.AddDays(-1).ToString("yyyy-MM-dd") + "/";

            // Check in current date folder
            if (FtpFileExistsInFolder(currentDateFolder, targetFileName, ftpUsername, ftpPassword))
            {
                ftpPath = currentDateFolder + targetFileName;
                return true; // File found in today's folder
            }

            // Check in previous date folder
            if (FtpFileExistsInFolder(previousDateFolder, targetFileName, ftpUsername, ftpPassword))
            {
                ftpPath = currentDateFolder + targetFileName;
                return true; // File found in yesterday's folder
            }
            ftpPath = "";
            return false; // File not found in both folders
        }

        public void Upload(Stream stream, string fileName)
        {
            FtpWebRequest reqFTP;
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpURI + fileName));
            reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
            reqFTP.KeepAlive = false;
            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;

            using (Stream reqStream=reqFTP.GetRequestStream())
            {
                contentLen = stream.Read(buff, 0, buffLength);
                while (contentLen != 0)
                {
                    reqStream.Write(buff, 0, buffLength);
                    contentLen = stream.Read(buff, 0, buffLength);
                }
            }
        }

        public void Upload(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            using (FileStream fs=fileInfo.OpenRead())
            {
                Upload(fs,fileInfo.Name);
            }
        }

        public static string UploadFileToFtp(string ftpUrl, string filePath)
        {
            // 获取文件名
            string fileName = Path.GetFileName(filePath);
            // 构建完整的 FTP 上传路径
            string FoldorUrl = Path.Combine(ftpUrl, DateTime.Now.ToString("yyMMdd")).Replace("\\", "/");

            // 检查目录是否存在
            if (!DirectoryExists(FoldorUrl, ftpUserID, ftpPassword))
            {
                // 如果目录不存在，创建它
                CreateDirectory(FoldorUrl, ftpUserID, ftpPassword);
            }

            // 构建完整的 FTP 上传路径
            string fullUrl = Path.Combine(ftpUrl, DateTime.Now.ToString("yyMMdd"), fileName).Replace("\\", "/");

            // 上传文件
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(FoldorUrl + "/" + Path.GetFileName(filePath));
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(ftpUserID, ftpPassword); // 设置 FTP 用户名和密码
            request.ContentLength = new FileInfo(filePath).Length;

            // 2. 读取本地文件
            byte[] fileContents = File.ReadAllBytes(filePath);
            request.ContentLength = fileContents.Length;

            // 3. 获取请求流并写入文件数据
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(fileContents, 0, fileContents.Length);
            }

            // 4. 获取 FTP 服务器响应
            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                Console.WriteLine($"上传成功! 状态码: {response.StatusCode}");
            }

            return fullUrl;
        }

        // 检查目录是否存在
        public static bool DirectoryExists(string folderUrl, string username, string password)
        {
            try
            {
                // 创建 FtpWebRequest 对象
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(folderUrl));
                request.Method = WebRequestMethods.Ftp.ListDirectory;  // 请求列出目录
                request.Credentials = new NetworkCredential(username, password);

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    // 如果目录存在，返回 true
                    return true;
                }
            }
            catch (WebException ex)
            {
                // 目录不存在时会抛出异常
                if (ex.Response != null)
                {
                    using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        string responseText = reader.ReadToEnd();
                        // 检查响应是否为 550 错误（文件夹不存在）
                        if (responseText.Contains("550"))
                        {
                            return false;  // 目录不存在
                        }
                    }
                }
                return false;  // 如果无法连接，认为目录不存在
            }
        }

        // 创建目录
        public static void CreateDirectory(string folderUrl, string username, string password)
        {
            try
            {
                // 创建 FtpWebRequest 对象
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(folderUrl));
                request.Method = WebRequestMethods.Ftp.MakeDirectory;  // 请求创建目录
                request.Credentials = new NetworkCredential(username, password);

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    Console.WriteLine("Directory created successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating directory: {ex.Message}");
            }
        }

    }
}
