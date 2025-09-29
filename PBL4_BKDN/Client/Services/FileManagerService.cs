using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common.Models;
using Common.Networking;

namespace Client.Services
{
    public sealed class FileManagerService
    {
        public async Task<FileManagerModel> ProcessRequestAsync(FileManagerRequest request)
        {
            var model = new FileManagerModel
            {
                ClientId = Guid.NewGuid().ToString(),
                ClientName = Environment.MachineName,
                Timestamp = DateTime.UtcNow
            };

            try
            {
                switch (request.OperationType)
                {
                    case FileManagerOperationType.ListDrives:
                        model = await GetDrivesAsync();
                        break;
                    case FileManagerOperationType.ListDirectory:
                        model = await ListDirectoryAsync(request.TargetPath ?? "", request.IncludeHidden, request.IncludeSystem);
                        break;
                    case FileManagerOperationType.Delete:
                        model = await DeleteItemAsync(request.TargetPath ?? "");
                        break;
                    case FileManagerOperationType.Rename:
                        model = await RenameItemAsync(request.TargetPath ?? "", request.NewName ?? "");
                        break;
                    case FileManagerOperationType.Search:
                        model = await SearchItemsAsync(request.SearchPath ?? "", request.SearchPattern ?? "*", request.IncludeHidden);
                        break;
                    case FileManagerOperationType.Download:
                        model = await DownloadFileAsync(request.TargetPath ?? "");
                        break;
                    case FileManagerOperationType.CreateDirectory:
                        model = await CreateDirectoryAsync(request.TargetPath ?? "");
                        break;
                    default:
                        throw new NotSupportedException($"Operation type {request.OperationType} is not supported");
                }
            }
            catch (Exception ex)
            {
                model.OperationResult = new FileManagerOperationResult
                {
                    OperationType = request.OperationType,
                    Success = false,
                    ErrorMessage = ex.Message,
                    TargetPath = request.TargetPath
                };
            }

            return model;
        }

        private async Task<FileManagerModel> GetDrivesAsync()
        {
            return await Task.Run(() =>
            {
                var model = new FileManagerModel
                {
                    ClientId = Guid.NewGuid().ToString(),
                    ClientName = Environment.MachineName,
                    Timestamp = DateTime.UtcNow
                };

                var drives = DriveInfo.GetDrives();
                foreach (var drive in drives)
                {
                    var driveModel = new DriveInfoModel
                    {
                        Name = drive.Name,
                        Label = drive.IsReady ? drive.VolumeLabel : "Unknown",
                        Type = drive.DriveType.ToString(),
                        TotalBytes = drive.IsReady ? drive.TotalSize : 0,
                        FreeBytes = drive.IsReady ? drive.AvailableFreeSpace : 0,
                        IsReady = drive.IsReady
                    };
                    model.Drives.Add(driveModel);
                }

                model.OperationResult = new FileManagerOperationResult
                {
                    OperationType = FileManagerOperationType.ListDrives,
                    Success = true
                };

                return model;
            });
        }

        private async Task<FileManagerModel> ListDirectoryAsync(string path, bool includeHidden, bool includeSystem)
        {
            return await Task.Run(() =>
            {
                var model = new FileManagerModel
                {
                    ClientId = Guid.NewGuid().ToString(),
                    ClientName = Environment.MachineName,
                    Timestamp = DateTime.UtcNow,
                    CurrentPath = path
                };

                try
                {
                    if (!Directory.Exists(path))
                    {
                        throw new DirectoryNotFoundException($"Directory '{path}' not found");
                    }

                    var dirInfo = new DirectoryInfo(path);
                    var items = new List<FileSystemItemModel>();

                    // Add directories first
                    var directories = dirInfo.GetDirectories()
                        .Where(d => includeHidden || !d.Attributes.HasFlag(FileAttributes.Hidden))
                        .Where(d => includeSystem || !d.Attributes.HasFlag(FileAttributes.System))
                        .OrderBy(d => d.Name);

                    foreach (var dir in directories)
                    {
                        try
                        {
                            items.Add(new FileSystemItemModel
                            {
                                Name = dir.Name,
                                FullPath = dir.FullName,
                                Type = "Directory",
                                SizeBytes = 0,
                                CreatedDate = dir.CreationTime,
                                ModifiedDate = dir.LastWriteTime,
                                AccessedDate = dir.LastAccessTime,
                                Extension = null,
                                IsHidden = dir.Attributes.HasFlag(FileAttributes.Hidden),
                                IsReadOnly = dir.Attributes.HasFlag(FileAttributes.ReadOnly)
                            });
                        }
                        catch (UnauthorizedAccessException)
                        {
                            // Skip directories we can't access
                        }
                    }

                    // Add files
                    var files = dirInfo.GetFiles()
                        .Where(f => includeHidden || !f.Attributes.HasFlag(FileAttributes.Hidden))
                        .Where(f => includeSystem || !f.Attributes.HasFlag(FileAttributes.System))
                        .OrderBy(f => f.Name);

                    foreach (var file in files)
                    {
                        try
                        {
                            items.Add(new FileSystemItemModel
                            {
                                Name = file.Name,
                                FullPath = file.FullName,
                                Type = "File",
                                SizeBytes = file.Length,
                                CreatedDate = file.CreationTime,
                                ModifiedDate = file.LastWriteTime,
                                AccessedDate = file.LastAccessTime,
                                Extension = file.Extension,
                                IsHidden = file.Attributes.HasFlag(FileAttributes.Hidden),
                                IsReadOnly = file.Attributes.HasFlag(FileAttributes.ReadOnly)
                            });
                        }
                        catch (UnauthorizedAccessException)
                        {
                            // Skip files we can't access
                        }
                    }

                    model.Items = items;
                    model.OperationResult = new FileManagerOperationResult
                    {
                        OperationType = FileManagerOperationType.ListDirectory,
                        Success = true,
                        TargetPath = path
                    };
                }
                catch (Exception ex)
                {
                    model.OperationResult = new FileManagerOperationResult
                    {
                        OperationType = FileManagerOperationType.ListDirectory,
                        Success = false,
                        ErrorMessage = ex.Message,
                        TargetPath = path
                    };
                }

                return model;
            });
        }

        private async Task<FileManagerModel> DeleteItemAsync(string path)
        {
            return await Task.Run(() =>
            {
                var model = new FileManagerModel
                {
                    ClientId = Guid.NewGuid().ToString(),
                    ClientName = Environment.MachineName,
                    Timestamp = DateTime.UtcNow
                };

                try
                {
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }
                    else if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    else
                    {
                        throw new FileNotFoundException($"Path '{path}' not found");
                    }

                    model.OperationResult = new FileManagerOperationResult
                    {
                        OperationType = FileManagerOperationType.Delete,
                        Success = true,
                        TargetPath = path
                    };
                }
                catch (Exception ex)
                {
                    model.OperationResult = new FileManagerOperationResult
                    {
                        OperationType = FileManagerOperationType.Delete,
                        Success = false,
                        ErrorMessage = ex.Message,
                        TargetPath = path
                    };
                }

                return model;
            });
        }

        private async Task<FileManagerModel> RenameItemAsync(string oldPath, string newName)
        {
            return await Task.Run(() =>
            {
                var model = new FileManagerModel
                {
                    ClientId = Guid.NewGuid().ToString(),
                    ClientName = Environment.MachineName,
                    Timestamp = DateTime.UtcNow
                };

                try
                {
                    var directory = Path.GetDirectoryName(oldPath);
                    var newPath = Path.Combine(directory ?? "", newName);

                    if (Directory.Exists(oldPath))
                    {
                        Directory.Move(oldPath, newPath);
                    }
                    else if (File.Exists(oldPath))
                    {
                        File.Move(oldPath, newPath);
                    }
                    else
                    {
                        throw new FileNotFoundException($"Path '{oldPath}' not found");
                    }

                    model.OperationResult = new FileManagerOperationResult
                    {
                        OperationType = FileManagerOperationType.Rename,
                        Success = true,
                        TargetPath = oldPath,
                        NewPath = newPath
                    };
                }
                catch (Exception ex)
                {
                    model.OperationResult = new FileManagerOperationResult
                    {
                        OperationType = FileManagerOperationType.Rename,
                        Success = false,
                        ErrorMessage = ex.Message,
                        TargetPath = oldPath,
                        NewPath = newName
                    };
                }

                return model;
            });
        }

        private async Task<FileManagerModel> SearchItemsAsync(string searchPath, string pattern, bool includeHidden)
        {
            return await Task.Run(() =>
            {
                var model = new FileManagerModel
                {
                    ClientId = Guid.NewGuid().ToString(),
                    ClientName = Environment.MachineName,
                    Timestamp = DateTime.UtcNow,
                    CurrentPath = searchPath
                };

                try
                {
                    if (!Directory.Exists(searchPath))
                    {
                        throw new DirectoryNotFoundException($"Search path '{searchPath}' not found");
                    }

                    var items = new List<FileSystemItemModel>();
                    SearchDirectory(new DirectoryInfo(searchPath), pattern, items, includeHidden);

                    model.Items = items;
                    model.OperationResult = new FileManagerOperationResult
                    {
                        OperationType = FileManagerOperationType.Search,
                        Success = true,
                        TargetPath = searchPath
                    };
                }
                catch (Exception ex)
                {
                    model.OperationResult = new FileManagerOperationResult
                    {
                        OperationType = FileManagerOperationType.Search,
                        Success = false,
                        ErrorMessage = ex.Message,
                        TargetPath = searchPath
                    };
                }

                return model;
            });
        }

        private void SearchDirectory(DirectoryInfo directory, string pattern, List<FileSystemItemModel> items, bool includeHidden)
        {
            try
            {
                // Search files
                var files = directory.GetFiles(pattern)
                    .Where(f => includeHidden || !f.Attributes.HasFlag(FileAttributes.Hidden))
                    .Where(f => !f.Attributes.HasFlag(FileAttributes.System));

                foreach (var file in files)
                {
                    try
                    {
                        items.Add(new FileSystemItemModel
                        {
                            Name = file.Name,
                            FullPath = file.FullName,
                            Type = "File",
                            SizeBytes = file.Length,
                            CreatedDate = file.CreationTime,
                            ModifiedDate = file.LastWriteTime,
                            AccessedDate = file.LastAccessTime,
                            Extension = file.Extension,
                            IsHidden = file.Attributes.HasFlag(FileAttributes.Hidden),
                            IsReadOnly = file.Attributes.HasFlag(FileAttributes.ReadOnly)
                        });
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Skip files we can't access
                    }
                }

                // Search subdirectories
                var directories = directory.GetDirectories()
                    .Where(d => includeHidden || !d.Attributes.HasFlag(FileAttributes.Hidden))
                    .Where(d => !d.Attributes.HasFlag(FileAttributes.System));

                foreach (var subdir in directories)
                {
                    try
                    {
                        SearchDirectory(subdir, pattern, items, includeHidden);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Skip directories we can't access
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Skip directories we can't access
            }
        }

        private async Task<FileManagerModel> DownloadFileAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                var model = new FileManagerModel
                {
                    ClientId = Guid.NewGuid().ToString(),
                    ClientName = Environment.MachineName,
                    Timestamp = DateTime.UtcNow
                };

                try
                {
                    if (!File.Exists(filePath))
                    {
                        throw new FileNotFoundException($"File '{filePath}' not found");
                    }

                    var fileData = File.ReadAllBytes(filePath);
                    var fileName = Path.GetFileName(filePath);

                    model.OperationResult = new FileManagerOperationResult
                    {
                        OperationType = FileManagerOperationType.Download,
                        Success = true,
                        TargetPath = filePath,
                        FileName = fileName,
                        FileData = fileData
                    };
                }
                catch (Exception ex)
                {
                    model.OperationResult = new FileManagerOperationResult
                    {
                        OperationType = FileManagerOperationType.Download,
                        Success = false,
                        ErrorMessage = ex.Message,
                        TargetPath = filePath
                    };
                }

                return model;
            });
        }

        private async Task<FileManagerModel> CreateDirectoryAsync(string path)
        {
            return await Task.Run(() =>
            {
                var model = new FileManagerModel
                {
                    ClientId = Guid.NewGuid().ToString(),
                    ClientName = Environment.MachineName,
                    Timestamp = DateTime.UtcNow
                };

                try
                {
                    Directory.CreateDirectory(path);

                    model.OperationResult = new FileManagerOperationResult
                    {
                        OperationType = FileManagerOperationType.CreateDirectory,
                        Success = true,
                        TargetPath = path
                    };
                }
                catch (Exception ex)
                {
                    model.OperationResult = new FileManagerOperationResult
                    {
                        OperationType = FileManagerOperationType.CreateDirectory,
                        Success = false,
                        ErrorMessage = ex.Message,
                        TargetPath = path
                    };
                }

                return model;
            });
        }
    }
}
