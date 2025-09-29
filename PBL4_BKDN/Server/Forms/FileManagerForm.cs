using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common.Enums;
using Common.Models;
using Common.Networking;
using Common.Utils;
using Server.Handlers;
using Server.Networking;

namespace Server.Forms
{
    public partial class FileManagerForm : Form
    {
        private static readonly Dictionary<string, FileManagerForm> OpenedForms = new();

        public static FileManagerForm CreateNewOrGetExisting(ServerClientConnection connection, FileManagerHandler fileManagerHandler)
        {
            if (OpenedForms.TryGetValue(connection.Id, out var existing))
                return existing;

            var f = new FileManagerForm(connection, fileManagerHandler);
            f.Disposed += (_, __) => OpenedForms.Remove(connection.Id);
            OpenedForms[connection.Id] = f;
            return f;
        }

        private readonly ServerClientConnection _connection;
        private readonly FileManagerHandler _fileManagerHandler;
        private string _currentPath = string.Empty;
        private string _currentClientId = string.Empty;
        private bool _isConnected = true;

        // UI Controls
        private TreeView? _treeView;
        private ListView? _listView;
        private TextBox? _pathTextBox;
        private TextBox? _searchTextBox;
        private Button? _refreshButton;
        private Button? _backButton;
        private Button? _searchButton;
        private Button? _deleteButton;
        private Button? _renameButton;
        private Button? _downloadButton;
        private Button? _createFolderButton;
        private StatusStrip? _statusStrip;
        private ToolStripStatusLabel? _statusLabel;

        public FileManagerForm(ServerClientConnection connection, FileManagerHandler fileManagerHandler)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _fileManagerHandler = fileManagerHandler ?? throw new ArgumentNullException(nameof(fileManagerHandler));

            InitializeComponent();
            SetupUI();
            RegisterEventHandlers();
            InitializeFileManager();
        }

        private void SetupUI()
        {
            // Form properties
            this.Text = $"File Manager - {_connection.RemoteAddress}";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Icon = SystemIcons.Application;

            CreateControls();
            LayoutControls();
        }

        private void CreateControls()
        {
            // TreeView for drives
            _treeView = new TreeView
            {
                Dock = DockStyle.Left,
                Width = 200,
                ImageList = CreateImageList()
            };

            // ListView for files and folders
            _listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false
            };

            _listView.Columns.Add("Name", 300);
            _listView.Columns.Add("Type", 80);
            _listView.Columns.Add("Size", 100);
            _listView.Columns.Add("Modified", 150);
            _listView.Columns.Add("Attributes", 100);

            // Path textbox
            _pathTextBox = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 25,
                ReadOnly = true,
                BackColor = SystemColors.Control
            };

            // Search controls
            var searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 30
            };

            _searchTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                PlaceholderText = "Search files..."
            };

            _searchButton = new Button
            {
                Dock = DockStyle.Right,
                Width = 80,
                Text = "Search",
                UseVisualStyleBackColor = true
            };

            searchPanel.Controls.Add(_searchTextBox);
            searchPanel.Controls.Add(_searchButton);

            // Toolbar
            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 35
            };

            _refreshButton = new Button
            {
                Dock = DockStyle.Left,
                Width = 80,
                Text = "Refresh",
                UseVisualStyleBackColor = true
            };

            _backButton = new Button
            {
                Dock = DockStyle.Left,
                Width = 60,
                Text = "Back",
                UseVisualStyleBackColor = true,
                Enabled = false
            };

            _deleteButton = new Button
            {
                Dock = DockStyle.Left,
                Width = 80,
                Text = "Delete",
                UseVisualStyleBackColor = true,
                Enabled = false
            };

            _renameButton = new Button
            {
                Dock = DockStyle.Left,
                Width = 80,
                Text = "Rename",
                UseVisualStyleBackColor = true,
                Enabled = false
            };

            _downloadButton = new Button
            {
                Dock = DockStyle.Left,
                Width = 80,
                Text = "Download",
                UseVisualStyleBackColor = true,
                Enabled = false
            };

            _createFolderButton = new Button
            {
                Dock = DockStyle.Left,
                Width = 100,
                Text = "New Folder",
                UseVisualStyleBackColor = true
            };

            toolbar.Controls.Add(_createFolderButton);
            toolbar.Controls.Add(_downloadButton);
            toolbar.Controls.Add(_renameButton);
            toolbar.Controls.Add(_deleteButton);
            toolbar.Controls.Add(_backButton);
            toolbar.Controls.Add(_refreshButton);

            // Status strip
            _statusStrip = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel("Ready");
            _statusStrip.Items.Add(_statusLabel);

            // Add controls to form
            this.Controls.Add(_listView);
            this.Controls.Add(_treeView);
            this.Controls.Add(searchPanel);
            this.Controls.Add(_pathTextBox);
            this.Controls.Add(toolbar);
            this.Controls.Add(_statusStrip);
        }

        private void LayoutControls()
        {
            // Set tab order
            _refreshButton!.TabStop = true;
            _backButton!.TabStop = true;
            _searchTextBox!.TabStop = true;
            _searchButton!.TabStop = true;
        }

        private ImageList CreateImageList()
        {
            var imageList = new ImageList();
            imageList.Images.Add("drive", SystemIcons.Shield.ToBitmap());
            imageList.Images.Add("folder", SystemIcons.Application.ToBitmap());
            imageList.Images.Add("file", SystemIcons.Information.ToBitmap());
            return imageList;
        }

        private void RegisterEventHandlers()
        {
            _refreshButton!.Click += RefreshButton_Click;
            _backButton!.Click += BackButton_Click;
            _searchButton!.Click += SearchButton_Click;
            _deleteButton!.Click += DeleteButton_Click;
            _renameButton!.Click += RenameButton_Click;
            _downloadButton!.Click += DownloadButton_Click;
            _createFolderButton!.Click += CreateFolderButton_Click;

            _listView!.SelectedIndexChanged += ListView_SelectedIndexChanged;
            _listView!.DoubleClick += ListView_DoubleClick;
            _treeView!.AfterSelect += TreeView_AfterSelect;

            _connection.OnDisconnected += (clientId) =>
            {
                _isConnected = false;
                this.Invoke(new Action(() =>
                {
                    _statusLabel!.Text = "Client disconnected";
                    EnableControls(false);
                }));
            };

            _fileManagerHandler.OnResponseReceived += FileManagerHandler_OnResponseReceived;
        }

        private void InitializeFileManager()
        {
            _statusLabel!.Text = "Loading drives...";
            RequestDrives();
        }

        private void RequestDrives()
        {
            if (!_isConnected) return;

            var request = new FileManagerRequest
            {
                OperationType = FileManagerOperationType.ListDrives,
                ClientId = _connection.Id
            };

            SendRequest(request);
        }

        private void RequestDirectoryListing(string path)
        {
            if (!_isConnected) return;

            var request = new FileManagerRequest
            {
                OperationType = FileManagerOperationType.ListDirectory,
                TargetPath = path,
                IncludeHidden = false,
                IncludeSystem = false,
                ClientId = _connection.Id
            };

            SendRequest(request);
        }

        private void SendRequest(FileManagerRequest request)
        {
            try
            {
                var json = JsonHelper.Serialize(request);
                _ = _connection.SendAsync(json);
                _statusLabel!.Text = $"Request sent: {request.OperationType}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to send request: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FileManagerHandler_OnResponseReceived(object? sender, FileManagerResponse response)
        {
            if (response.ClientId != _connection.Id) return;

            this.Invoke(new Action(() =>
            {
                try
                {
                    switch (response.Status)
                    {
                        case ResponseStatusType.Ok:
                            HandleSuccessfulResponse(response);
                            break;
                        case ResponseStatusType.Error:
                            HandleErrorResponse(response);
                            break;
                        default:
                            _statusLabel!.Text = $"Unknown response status: {response.Status}";
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error processing response: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }));
        }

        private void HandleSuccessfulResponse(FileManagerResponse response)
        {
            if (response.Payload?.OperationResult == null) return;

            var operation = response.Payload.OperationResult.OperationType;
            switch (operation)
            {
                case FileManagerOperationType.ListDrives:
                    PopulateDrives(response.Payload.Drives);
                    break;
                case FileManagerOperationType.ListDirectory:
                    PopulateDirectory(response.Payload.Items, response.Payload.CurrentPath);
                    break;
                case FileManagerOperationType.Delete:
                    _statusLabel!.Text = "Item deleted successfully";
                    RefreshCurrentDirectory();
                    break;
                case FileManagerOperationType.Rename:
                    _statusLabel!.Text = "Item renamed successfully";
                    RefreshCurrentDirectory();
                    break;
                case FileManagerOperationType.Search:
                    PopulateSearchResults(response.Payload.Items, response.Payload.CurrentPath);
                    break;
                case FileManagerOperationType.Download:
                    HandleFileDownload(response.Payload.OperationResult);
                    break;
                case FileManagerOperationType.CreateDirectory:
                    _statusLabel!.Text = "Directory created successfully";
                    RefreshCurrentDirectory();
                    break;
            }
        }

        private void HandleErrorResponse(FileManagerResponse response)
        {
            var errorMsg = response.ErrorMessage ?? "Unknown error";
            _statusLabel!.Text = $"Error: {errorMsg}";
            MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void PopulateDrives(List<DriveInfoModel> drives)
        {
            _treeView!.Nodes.Clear();
            foreach (var drive in drives)
            {
                var node = new TreeNode(drive.Name)
                {
                    Tag = drive.Name,
                    ImageKey = "drive",
                    SelectedImageKey = "drive"
                };
                _treeView.Nodes.Add(node);
            }
            _statusLabel!.Text = $"Loaded {drives.Count} drives";
            EnableControls(_isConnected);
        }

        private void PopulateDirectory(List<FileSystemItemModel> items, string path)
        {
            _currentPath = path;
            _pathTextBox!.Text = path;
            _listView!.Items.Clear();

            foreach (var item in items)
            {
                var listItem = new ListViewItem(item.Name);
                listItem.SubItems.Add(item.Type);
                listItem.SubItems.Add(item.Type == "Directory" ? "" : FormatFileSize(item.SizeBytes));
                listItem.SubItems.Add(item.ModifiedDate.ToString("yyyy-MM-dd HH:mm"));
                listItem.SubItems.Add(FormatAttributes(item.IsHidden, item.IsReadOnly));

                listItem.Tag = item;
                listItem.ImageKey = item.Type == "Directory" ? "folder" : "file";

                _listView.Items.Add(listItem);
            }

            _statusLabel!.Text = $"Loaded {items.Count} items from {path}";
            EnableControls(_isConnected);
        }

        private void PopulateSearchResults(List<FileSystemItemModel> items, string searchPath)
        {
            _currentPath = searchPath;
            _pathTextBox!.Text = $"Search results in: {searchPath}";
            _listView!.Items.Clear();

            foreach (var item in items)
            {
                var listItem = new ListViewItem(item.Name);
                listItem.SubItems.Add(item.Type);
                listItem.SubItems.Add(item.Type == "Directory" ? "" : FormatFileSize(item.SizeBytes));
                listItem.SubItems.Add(item.ModifiedDate.ToString("yyyy-MM-dd HH:mm"));
                listItem.SubItems.Add(FormatAttributes(item.IsHidden, item.IsReadOnly));

                listItem.Tag = item;
                listItem.ImageKey = item.Type == "Directory" ? "folder" : "file";

                _listView.Items.Add(listItem);
            }

            _statusLabel!.Text = $"Found {items.Count} items";
        }

        private void HandleFileDownload(FileManagerOperationResult result)
        {
            if (result.FileData == null || string.IsNullOrEmpty(result.FileName))
            {
                MessageBox.Show("No file data received", "Download Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                FileName = result.FileName,
                Filter = "All Files (*.*)|*.*",
                Title = "Save Downloaded File"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllBytes(saveDialog.FileName, result.FileData);
                    
                    var fileInfo = new FileInfo(saveDialog.FileName);
                    var message = $"File downloaded successfully!\n\n" +
                                 $"File: {result.FileName}\n" +
                                 $"Saved to: {saveDialog.FileName}\n" +
                                 $"Size: {FormatFileSize(result.FileData.Length)}\n" +
                                 $"Location: {fileInfo.Directory?.FullName}\n\n" +
                                 $"Note: File is saved on the SERVER machine, not the client machine.";
                    
                    MessageBox.Show(message, "Download Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to save file: {ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024:F1} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024 * 1024):F1} MB";
            return $"{bytes / (1024 * 1024 * 1024):F1} GB";
        }

        private string FormatAttributes(bool isHidden, bool isReadOnly)
        {
            var attrs = new List<string>();
            if (isHidden) attrs.Add("H");
            if (isReadOnly) attrs.Add("R");
            return string.Join(",", attrs);
        }

        private void EnableControls(bool enabled)
        {
            _refreshButton!.Enabled = enabled;
            _backButton!.Enabled = enabled && CanGoBack();
            _searchButton!.Enabled = enabled;
            _createFolderButton!.Enabled = enabled;
            _deleteButton!.Enabled = enabled && _listView!.SelectedItems.Count > 0;
            _renameButton!.Enabled = enabled && _listView!.SelectedItems.Count > 0;
            _downloadButton!.Enabled = enabled && _listView!.SelectedItems.Count > 0 && GetSelectedItem()?.Type == "File";
        }

        private FileSystemItemModel? GetSelectedItem()
        {
            if (_listView?.SelectedItems.Count == 0) return null;
            return _listView?.SelectedItems[0].Tag as FileSystemItemModel;
        }

        private void RefreshCurrentDirectory()
        {
            if (!string.IsNullOrEmpty(_currentPath))
            {
                RequestDirectoryListing(_currentPath);
            }
            else
            {
                RequestDrives();
            }
        }

        private bool CanGoBack()
        {
            return !string.IsNullOrEmpty(_currentPath) && _currentPath.Length > 3; // More than just "C:\"
        }

        private string GetParentPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            
            var parent = Directory.GetParent(path);
            return parent?.FullName ?? string.Empty;
        }

        // Event Handlers
        private void RefreshButton_Click(object? sender, EventArgs e)
        {
            RefreshCurrentDirectory();
        }

        private void BackButton_Click(object? sender, EventArgs e)
        {
            if (CanGoBack())
            {
                var parentPath = GetParentPath(_currentPath);
                if (!string.IsNullOrEmpty(parentPath))
                {
                    RequestDirectoryListing(parentPath);
                }
                else
                {
                    RequestDrives(); // Go back to drives if at root
                }
            }
        }

        private void SearchButton_Click(object? sender, EventArgs e)
        {
            var searchText = _searchTextBox!.Text.Trim();
            if (string.IsNullOrEmpty(searchText))
            {
                MessageBox.Show("Please enter a search term", "Search", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!_isConnected) return;

            var request = new FileManagerRequest
            {
                OperationType = FileManagerOperationType.Search,
                SearchPattern = $"*{searchText}*",
                SearchPath = _currentPath,
                IncludeHidden = false,
                ClientId = _connection.Id
            };

            SendRequest(request);
        }

        private void DeleteButton_Click(object? sender, EventArgs e)
        {
            var selectedItem = GetSelectedItem();
            if (selectedItem == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete '{selectedItem.Name}'?", "Delete Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;

            var request = new FileManagerRequest
            {
                OperationType = FileManagerOperationType.Delete,
                TargetPath = selectedItem.FullPath,
                ClientId = _connection.Id
            };

            SendRequest(request);
        }

        private void RenameButton_Click(object? sender, EventArgs e)
        {
            var selectedItem = GetSelectedItem();
            if (selectedItem == null) return;

            var newName = ShowInputDialog("Enter new name:", "Rename", selectedItem.Name);
            if (string.IsNullOrEmpty(newName) || newName == selectedItem.Name) return;

            var request = new FileManagerRequest
            {
                OperationType = FileManagerOperationType.Rename,
                TargetPath = selectedItem.FullPath,
                NewName = newName,
                ClientId = _connection.Id
            };

            SendRequest(request);
        }

        private void DownloadButton_Click(object? sender, EventArgs e)
        {
            var selectedItem = GetSelectedItem();
            if (selectedItem?.Type != "File") return;

            var request = new FileManagerRequest
            {
                OperationType = FileManagerOperationType.Download,
                TargetPath = selectedItem.FullPath,
                ClientId = _connection.Id
            };

            SendRequest(request);
        }

        private void CreateFolderButton_Click(object? sender, EventArgs e)
        {
            var folderName = ShowInputDialog("Enter folder name:", "New Folder", "New Folder");
            if (string.IsNullOrEmpty(folderName)) return;

            var newPath = Path.Combine(_currentPath, folderName);

            var request = new FileManagerRequest
            {
                OperationType = FileManagerOperationType.CreateDirectory,
                TargetPath = newPath,
                ClientId = _connection.Id
            };

            SendRequest(request);
        }

        private void ListView_SelectedIndexChanged(object? sender, EventArgs e)
        {
            EnableControls(_isConnected);
        }

        private void ListView_DoubleClick(object? sender, EventArgs e)
        {
            var selectedItem = GetSelectedItem();
            if (selectedItem?.Type == "Directory")
            {
                RequestDirectoryListing(selectedItem.FullPath);
            }
        }

        private void TreeView_AfterSelect(object? sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag is string drivePath)
            {
                RequestDirectoryListing(drivePath);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _fileManagerHandler.OnResponseReceived -= FileManagerHandler_OnResponseReceived;
            base.OnFormClosing(e);
        }

        private string ShowInputDialog(string text, string caption, string defaultValue = "")
        {
            var prompt = new Form()
            {
                Width = 400,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var textLabel = new Label() { Left = 20, Top = 20, Text = text, Width = 350 };
            var textBox = new TextBox() { Left = 20, Top = 50, Width = 350, Text = defaultValue };
            var confirmation = new Button() { Text = "OK", Left = 230, Width = 80, Top = 80, DialogResult = DialogResult.OK };
            var cancel = new Button() { Text = "Cancel", Left = 320, Width = 80, Top = 80, DialogResult = DialogResult.Cancel };

            confirmation.Click += (sender, e) => { prompt.Close(); };
            cancel.Click += (sender, e) => { prompt.Close(); };

            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(cancel);
            prompt.AcceptButton = confirmation;
            prompt.CancelButton = cancel;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
    }
}
