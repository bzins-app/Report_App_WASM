﻿using Microsoft.EntityFrameworkCore;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Models;
using Report_App_WASM.Server.Utils.EncryptDecrypt;
using Report_App_WASM.Shared;

namespace ReportAppWASM.Server.Services.FilesManagement
{

    public class SftpService : IDisposable
    {
        // private readonly ILogger<FtpService> _logger;
        private readonly ApplicationDbContext _context;

        public SftpService(/*ILogger<FtpService> logger, */ApplicationDbContext context)
        {
            //  _logger = logger;
            _context = context;
        }

        private async Task<SFTPConfiguration> GetSFTPConfigurationAsync(int SFTPconfigurationId)
        {
            return await _context.SFTPConfiguration.Where(a => a.SFTPConfigurationId == SFTPconfigurationId).AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<SftpFile>> ListAllFilesAsync(int SFTPconfigurationId, string remoteDirectory = ".")
        {
            var _config = await GetSFTPConfigurationAsync(SFTPconfigurationId);
            using var client = new SftpClient(_config.Host, _config.Port == 0 ? 22 : _config.Port, _config.UserName, EncryptDecrypt.DecryptString(_config.Password));
            try
            {
                client.Connect();
                return client.ListDirectory(remoteDirectory);
            }
            catch (Exception exception)
            {
                //  _logger.LogError(exception, $"Failed in listing files under [{remoteDirectory}]");
                return null;
            }
            finally
            {
                client.Disconnect();
            }
        }

        public async Task<SubmitResult> UploadFileAsync(int SFTPconfigurationId, string localFilePath, string remoteDirectory, string fileName, bool tryCreateFolder = false)
        {
            var _config = await GetSFTPConfigurationAsync(SFTPconfigurationId);
            using var client = new SftpClient(_config.Host, _config.Port == 0 ? 22 : _config.Port, _config.UserName, EncryptDecrypt.DecryptString(_config.Password));
            try
            {
                client.Connect();
                if (tryCreateFolder && !client.Exists(remoteDirectory))
                {
                    client.CreateDirectory(remoteDirectory);
                }
                var destinationPath = Path.Combine(remoteDirectory, fileName);
                using FileStream fs = new(localFilePath, FileMode.Open);
                client.BufferSize = 4 * 1024;
                client.UploadFile(fs, destinationPath);
                // _logger.LogInformation($"Finished uploading file [{localFilePath}] to [{remoteDirectory}]");
            }
            catch (Exception exception)
            {
                // _logger.LogError(exception, $"Failed in uploading file [{localFilePath}] to [{remoteDirectory}]");
                return new SubmitResult() { Success = false, Message = exception.Message };
            }
            finally
            {
                client.Disconnect();
            }
            return new SubmitResult() { Success = true, Message = "Ok" };
        }

        public async Task<SubmitResult> DownloadFileAsync(int SFTPconfigurationId, string remoteFilePath, string localFilePath)
        {
            var _config = await GetSFTPConfigurationAsync(SFTPconfigurationId);
            using var client = new SftpClient(_config.Host, _config.Port == 0 ? 22 : _config.Port, _config.UserName, EncryptDecrypt.DecryptString(_config.Password));
            try
            {
                client.Connect();
                using var s = File.Create(localFilePath);
                client.DownloadFile(remoteFilePath, s);
                //  _logger.LogInformation($"Finished downloading file [{localFilePath}] from [{remoteFilePath}]");
            }
            catch (Exception exception)
            {
                //  _logger.LogError(exception, $"Failed in downloading file [{localFilePath}] from [{remoteFilePath}]");
                return new SubmitResult() { Success = false, Message = exception.Message };
            }
            finally
            {
                client.Disconnect();
            }
            return new SubmitResult() { Success = true, Message = "Ok" };
        }

        public async Task<SubmitResult> DeleteFileAsync(int SFTPconfigurationId, string remoteFilePath)
        {
            var _config = await GetSFTPConfigurationAsync(SFTPconfigurationId);
            using var client = new SftpClient(_config.Host, _config.Port == 0 ? 22 : _config.Port, _config.UserName, EncryptDecrypt.DecryptString(_config.Password));
            try
            {
                client.Connect();
                client.DeleteFile(remoteFilePath);
                //   _logger.LogInformation($"File [{remoteFilePath}] deleted.");
            }
            catch (Exception exception)
            {
                //  _logger.LogError(exception, $"Failed in deleting file [{remoteFilePath}]");
                return new SubmitResult() { Success = false, Message = exception.Message };
            }
            finally
            {
                client.Disconnect();
            }
            return new SubmitResult() { Success = true, Message = "Ok" };
        }

        public async Task<SubmitResult> TestDirectoryAsync(int SFTPconfigurationId, string remoteFilePath, bool tryCreateFolder = false)
        {
            var _config = await GetSFTPConfigurationAsync(SFTPconfigurationId);
            using var client = new SftpClient(_config.Host, _config.Port == 0 ? 22 : _config.Port, _config.UserName, EncryptDecrypt.DecryptString(_config.Password));
            bool checkAcces;
            try
            {
                client.Connect();
                checkAcces = client.Exists(remoteFilePath);
                if (!checkAcces && tryCreateFolder)
                {
                    client.CreateDirectory(remoteFilePath);
                    checkAcces = client.Exists(remoteFilePath);
                }
                //   _logger.LogInformation($"File [{remoteFilePath}] deleted.");
            }
            catch (Exception exception)
            {
                //  _logger.LogError(exception, $"Failed in deleting file [{remoteFilePath}]");
                return new SubmitResult() { Success = false, Message = exception.Message };
            }
            finally
            {
                client.Disconnect();
            }
            return new SubmitResult() { Success = checkAcces, Message = checkAcces == false ? "Cannot reach the path" : "Ok" };
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }

}