using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common.Models;
using Common.Networking;
using Common.Utils;

namespace Client.Services
{
    public sealed class FileManagerService
    {
        private readonly Dictionary<string, FileStream> _activeUploads = new();
        private readonly Dictionary<string, int> _expectedChunkIndex = new();
        private readonly Dictionary<string, CancellationTokenSource> _cancellationSources = new();
        private readonly TimeSpan UploadTimeout = TimeSpan.FromMinutes(10);

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
                    case FileManagerOperationType.UploadStart:
                        model = await UploadStartAsync(request.TransferId ?? "", request.TargetPath ?? "");
                        break;
                    case FileManagerOperationType.UploadChunk:
                        model = await UploadChunkAsync(request.UploadInfo ?? new FileUploadInfo());
                        break;
                    case FileManagerOperationType.UploadComplete:
                        model = await UploadCompleteAsync(request.UploadInfo ?? new FileUploadInfo());
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
        // Start a new upload session -> Client prepare FileStream and Path to save the file 
        private async Task<FileManagerModel> UploadStartAsync(String transferId, String targetPath)
        {
			return await Task.Run(() =>
            {
                var model = new FileManagerModel
                {
                    ClientId = Guid.NewGuid().ToString(),
                    ClientName = Environment.MachineName,
                    Timestamp = DateTime.UtcNow
                };

                try {
                    if (_activeUploads.ContainsKey(transferId))
                    {
                        throw new InvalidOperationException($"Upload session ID '{transferId}' already active.");
                    }

                    var cts = new CancellationTokenSource();
                    _cancellationSources.Add(transferId, cts);

                    _ = Task.Delay(UploadTimeout, cts.Token).ContinueWith(t =>
                    {
                        if (t.IsCanceled) return;
                        CleanupUploadSession(transferId, "Upload timed out after 10 minutes.");
                    }, TaskContinuationOptions.None);

                    var fileStream = File.Create(targetPath);
                    _activeUploads.Add(transferId, fileStream);
                    _expectedChunkIndex.Add(transferId, 0);
                    model.OperationResult = new FileManagerOperationResult
                    {
                        OperationType = FileManagerOperationType.UploadStart,
                        Success = true,
                        TargetPath = targetPath,
                        TransferId = transferId
                    };
                } 
                catch (Exception ex)
                {
                    _activeUploads.Remove(transferId, out var streamToRemove);
                    streamToRemove?.Dispose();
                    model.OperationResult = new FileManagerOperationResult
                    {
                        OperationType = FileManagerOperationType.UploadStart,
                        Success = false,
                        ErrorMessage = ex.Message,
                        TargetPath = targetPath
                    };
                }

                return model;
            });
        }

        private async Task<FileManagerModel> UploadChunkAsync(FileUploadInfo uploadInfo) 
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
                    if (!_activeUploads.TryGetValue(uploadInfo.TransferId, out var fileStream))
                    {
                        throw new KeyNotFoundException($"Active upload session ID '{uploadInfo.TransferId}' not found.");
                    }

                    int expectedIndex = _expectedChunkIndex[uploadInfo.TransferId]++;
                    if (expectedIndex != uploadInfo.ChunkIndex)
                    {
                        throw new InvalidOperationException($"Expected chunk index {expectedIndex} but received {uploadInfo.ChunkIndex}.");
                    }

                    fileStream.Write(uploadInfo.Data, 0, uploadInfo.Data.Length);
                    model.OperationResult = new FileManagerOperationResult
                    {
                        OperationType = FileManagerOperationType.UploadChunk,
                        Success = true,
                        TargetPath = uploadInfo.TargetPath,
                        TransferId = uploadInfo.TransferId
                    };
					
				}
                catch (Exception ex)
                {
                    _activeUploads.Remove(uploadInfo.TransferId, out var streamToRemove);
                    streamToRemove?.Dispose();
                    model.OperationResult = new FileManagerOperationResult
                    {
                        OperationType = FileManagerOperationType.UploadChunk,
                        Success = false,
                        ErrorMessage = ex.Message,
                        TargetPath = uploadInfo.TargetPath,
                    };
                }
                return model;
            });
        }

        private async Task<FileManagerModel> UploadCompleteAsync(FileUploadInfo uploadInfo) 
        {
            return await Task.Run(() =>
            {
                var model = new FileManagerModel
                {
                    ClientId = Guid.NewGuid().ToString(),
                    ClientName = Environment.MachineName,
                    Timestamp = DateTime.UtcNow
                };

                string targetPath = uploadInfo.TargetPath;
                string transferId = uploadInfo.TransferId;
                try
                {
                    if (!_activeUploads.TryGetValue(uploadInfo.TransferId, out var fileStream))
                    {
                        throw new KeyNotFoundException($"Active upload session ID '{uploadInfo.TransferId}' not found.");
                    }

				    if (_activeUploads.Remove(uploadInfo.TransferId, out fileStream))
                    {
                        _expectedChunkIndex.Remove(uploadInfo.TransferId);
						_cancellationSources.Remove(transferId, out var cts); 
						cts?.Cancel();
						cts?.Dispose();

						fileStream.Close();
						fileStream.Dispose();
					}
					else
					{
						throw new InvalidOperationException("FileStream cleanup failed or already completed.");
					}

                    string calculatedHash;
                    using (var stream = File.OpenRead(targetPath))
                    {
                        calculatedHash = HashHelper.ComputeSHA256(stream);
                    }

                    if (!HashHelper.CompareSHA256Hashes(calculatedHash, uploadInfo.FileHash ?? ""))
                    {
                        File.Delete(targetPath);
                        throw new InvalidOperationException(
                            $"Hash mismatch! Expected: {uploadInfo.FileHash}, Calculated: {calculatedHash}. File deleted.");
					}

					model.OperationResult = new FileManagerOperationResult
					{
						OperationType = FileManagerOperationType.UploadComplete,
						Success = true,
						TargetPath = targetPath,
						TransferId = transferId,
                        CalculatedFileHash = calculatedHash
					};
				}
                catch (Exception ex)
                {
					CleanupUploadSession(transferId, $"Error during completion: {ex.Message}");

					model.OperationResult = new FileManagerOperationResult
					{
						OperationType = FileManagerOperationType.UploadComplete,
						Success = false,
						ErrorMessage = ex.Message,
						TargetPath = targetPath,
						TransferId = transferId
					};
				}

                return model;
            });
        }
        private void CleanupUploadSession(string transferId, string reason)
        {
            //1. Hủy Timer/Task
            if (_cancellationSources.Remove(transferId, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
            }

            //2. Đóng FileStream
            if (_activeUploads.Remove(transferId, out var fileStream))
            {
                Console.WriteLine($"Cleanup: Upload {transferId} failed ({reason}). Deleting file...");

                string filePath = fileStream.Name;
                fileStream.Close();
                fileStream.Dispose();

                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                } catch { }
			}

            _expectedChunkIndex.Remove(transferId);
        }
    }

}
