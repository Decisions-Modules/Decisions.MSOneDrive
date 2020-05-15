// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace OneDriveApiBrowser
{
    using Microsoft.Graph;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using OneDriveLib;

    public partial class FormBrowser : Form
    {
        public const string MsaClientId = "a4ff7c3c-d1e5-47eb-a654-e0038f637a52";
        public const string MsaReturnUrl = "urn:ietf:wg:oauth:2.0:oob";
        
        private enum ClientType
        {
            Consumer,
            Business
        }

        private const int UploadChunkSize = 10 * 1024 * 1024;       // 10 MB
        //private IOneDriveClient oneDriveClient { get; set; }
        private GraphServiceClient graphClient { get; set; }
        private ClientType clientType { get; set; }
        private DriveItem CurrentFolder { get; set; }
        private DriveItem SelectedItem { get; set; }

        private OneDriveTile _selectedTile;

        public FormBrowser()
        {
            InitializeComponent();
        }

        private void ShowWork(bool working)
        {
            this.UseWaitCursor = working;
            this.progressBar1.Visible = working;

        }

        private void LoadFolderFromId(string id)
        {
            // Update the UI for loading something new
            ShowWork(true);
            LoadChildren(new DriveItem[0]);

            try
            {
                OneDriveLib.Browser.ResultItem loadResult = Browser.ListFolderFromId(graphClient, id);
                Microsoft.Graph.DriveItem parentFolder = loadResult.ParentFolder;
                Microsoft.Graph.DriveItem[] resultsItems = loadResult.ChildrenItems;

                ProcessFolder(parentFolder);
                string totalMessage = "", strMessage;
                for (int i = 0; i < resultsItems.Length; i++)
                {
                    strMessage = String.Format("Name:{0}-----ID:{1}", resultsItems[i].Name, resultsItems[i].Id);
                    if (resultsItems[i].Folder != null)
                        strMessage = strMessage + String.Format("--FolderChild:{0}", resultsItems[i].Folder.ChildCount);
                    totalMessage = String.Format("{0}\n{1}", totalMessage, strMessage);
                }
                MessageBox.Show(totalMessage);
                UpdateConnectedStateUx(true);
            }
            catch (ServiceException exception)
            {
                PresentServiceException(exception);
            }

            ShowWork(false);
        }

        private void LoadFolderFromPath(string path = null)
        {
            // Update the UI for loading something new
            ShowWork(true);
            LoadChildren(new DriveItem[0]);

            try
            {
                OneDriveLib.Browser.ResultItem retItem = Browser.ListFolderFromPath(graphClient, path);
                Microsoft.Graph.DriveItem parentFolder = retItem.ParentFolder;
                Microsoft.Graph.DriveItem[] resultsItems = retItem.ChildrenItems;

                ProcessFolder(parentFolder);
                string totalMessage = "", strMessage;
                for (int i = 0; i < resultsItems.Length; i++)
                {
                    strMessage = String.Format("Name:{0}-----ID:{1}", resultsItems[i].Name, resultsItems[i].Id);
                    if (resultsItems[i].Folder != null)
                        strMessage = strMessage + String.Format("--FolderChild:{0}", resultsItems[i].Folder.ChildCount);
                    totalMessage = String.Format("{0}\n{1}", totalMessage, strMessage);
                }
                MessageBox.Show(totalMessage);

            }
            catch (Exception exception)
            {
                PresentServiceException(exception);
            }

            ShowWork(false);
        }

        private void ProcessFolder(DriveItem folder)
        {
            if (folder != null)
            {
                this.CurrentFolder = folder;

                LoadProperties(folder);

                if (folder.Folder != null && folder.Children != null && folder.Children.CurrentPage != null)
                {
                    LoadChildren(folder.Children.CurrentPage);
                }
            }
        }

        private void LoadProperties(DriveItem item)
        {
            this.SelectedItem = item;
            objectBrowser.SelectedItem = item;
        }

        private void LoadChildren(IList<DriveItem> items)
        {
            flowLayoutContents.SuspendLayout();
            flowLayoutContents.Controls.Clear();

            // Load the children
            foreach (var obj in items)
            {
                AddItemToFolderContents(obj);
            }

            flowLayoutContents.ResumeLayout();
        }

        private void AddItemToFolderContents(DriveItem obj)
        {
            flowLayoutContents.Controls.Add(CreateControlForChildObject(obj));
        }

        private void RemoveItemFromFolderContents(DriveItem itemToDelete)
        {
            flowLayoutContents.Controls.RemoveByKey(itemToDelete.Id);
        }

        private Control CreateControlForChildObject(DriveItem item)
        {
            OneDriveTile tile = new OneDriveTile(this.graphClient);
            tile.SourceItem = item;
            tile.Click += ChildObject_Click;
            tile.DoubleClick += ChildObject_DoubleClick;
            tile.Name = item.Id;
            return tile;
        }

        void ChildObject_DoubleClick(object sender, EventArgs e)
        {
            var item = ((OneDriveTile)sender).SourceItem;

            // Look up the object by ID
            NavigateToFolder(item);
        }
        void ChildObject_Click(object sender, EventArgs e)
        {
            if (null != _selectedTile)
            {
                _selectedTile.Selected = false;
            }
            
            var item = ((OneDriveTile)sender).SourceItem;
            LoadProperties(item);
            _selectedTile = (OneDriveTile)sender;
            _selectedTile.Selected = true;
        }

        private void FormBrowser_Load(object sender, EventArgs e)
        {
            
        }

        private void NavigateToFolder(DriveItem folder)
        {
            LoadFolderFromId(folder.Id);

            // Fix up the breadcrumbs
            var breadcrumbs = flowLayoutPanelBreadcrumb.Controls;
            bool existingCrumb = false;
            foreach (LinkLabel crumb in breadcrumbs)
            {
                if (crumb.Tag == folder)
                {
                    RemoveDeeperBreadcrumbs(crumb);
                    existingCrumb = true;
                    break;
                }
            }

            if (!existingCrumb)
            {
                LinkLabel label = new LinkLabel();
                label.Text = "> " + folder.Name;
                label.LinkArea = new LinkArea(2, folder.Name.Length);
                label.LinkClicked += linkLabelBreadcrumb_LinkClicked;
                label.AutoSize = true;
                label.Tag = folder;
                flowLayoutPanelBreadcrumb.Controls.Add(label);
            }
        }

        private void linkLabelBreadcrumb_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel link = (LinkLabel)sender;

            RemoveDeeperBreadcrumbs(link);

            DriveItem item = link.Tag as DriveItem;
            if (null == item)
            {

                LoadFolderFromPath(null);
            }
            else
            {
                LoadFolderFromId(item.Id);
            }
        }

        private void RemoveDeeperBreadcrumbs(LinkLabel link)
        {
            // Remove the breadcrumbs deeper than this item
            var breadcrumbs = flowLayoutPanelBreadcrumb.Controls;
            int indexOfControl = breadcrumbs.IndexOf(link);
            for (int i = breadcrumbs.Count - 1; i > indexOfControl; i--)
            {
                breadcrumbs.RemoveAt(i);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void UpdateConnectedStateUx(bool connected)
        {
            signInMsaToolStripMenuItem.Visible = !connected;
            signOutToolStripMenuItem.Visible = connected;
            flowLayoutPanelBreadcrumb.Visible = connected;
            flowLayoutContents.Visible = connected;
        }

        private void signInMsaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.MySignIn();
        }

        private void MySignIn()
        {  
            try
            {
                graphClient = Browser.SignIn(MsaClientId);
            }
            catch (ServiceException exception)
            {
                PresentServiceException(exception);
            }

            ShowWork(true);
            LoadChildren(new DriveItem[0]);

            try
            {
                OneDriveLib.Browser.ResultItem resultItem = Browser.ListFolderFromPath(graphClient);
                
                Microsoft.Graph.DriveItem rootFolder = resultItem.ParentFolder;
                Microsoft.Graph.DriveItem[] resultsItems = resultItem.ChildrenItems;

                ProcessFolder(rootFolder);
                string totalMessage = "", strMessage;
                for (int i = 0; i < resultsItems.Length; i++)
                {
                    strMessage = String.Format("Name:{0}-----ID:{1}", resultsItems[i].Name, resultsItems[i].Id);
                    if (resultsItems[i].Folder != null)
                        strMessage = strMessage + String.Format("--FolderChild:{0}", resultsItems[i].Folder.ChildCount);
                    totalMessage = String.Format("{0}\n{1}", totalMessage, strMessage);
                }
                MessageBox.Show(totalMessage);
                UpdateConnectedStateUx(true);
            }
            catch (ServiceException exception)
            {
                PresentServiceException(exception);
            }

            ShowWork(false);
        }


        private void signOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Browser.SignOut(graphClient);

            UpdateConnectedStateUx(false);
        }

        private string MyGetFileNameForUpload(string targetFolderName, out string originalFilename)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Upload to " + targetFolderName;
            dialog.Filter = "All Files (*.*)|*.*";
            dialog.CheckFileExists = true;
            var response = dialog.ShowDialog();
            if (response != DialogResult.OK)
            {
                originalFilename = null;
                return null;
            }

            try
            {
                originalFilename = System.IO.Path.GetFileName(dialog.FileName);
                return dialog.FileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error uploading file: " + ex.Message);
                originalFilename = null;
                return null;
            }
        }


        private void simpleUploadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var targetFolder = this.CurrentFolder;

            string filename;
            string filePath = MyGetFileNameForUpload(targetFolder.Name, out filename);
            if (filePath != null)
            {
                string folderPath = targetFolder.ParentReference.Path == null
                    ? ""
                    : targetFolder.ParentReference.Path.Remove(0, 12) + "/" + Uri.EscapeUriString(targetFolder.Name);
                try
                {
                    var uploadedItem = Browser.UploadFilebyPath(graphClient, folderPath, filePath);
                    MessageBox.Show("Uploaded with ID: " + uploadedItem.Id);
                }
                catch (Exception exception)
                {
                    PresentServiceException(exception);
                }
            }
        }

        private void simpleIDbasedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var targetFolder = this.CurrentFolder;

            string filename;
            string filePath = MyGetFileNameForUpload(targetFolder.Name, out filename);
            if (filePath != null)
            {
                try
                {
                    var uploadedItem = Browser.UploadFilebyID(graphClient, targetFolder.Id, filePath);
                    MessageBox.Show("Uploaded with ID: " + uploadedItem.Id);
                }
                catch (Exception exception)
                {
                    PresentServiceException(exception);
                }
            }
        }

        private async void createFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormInputDialog dialog = new FormInputDialog("Create Folder", "New folder name:");
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrEmpty(dialog.InputText))
            {
                try
                {
                    var folderToCreate = new DriveItem { Name = dialog.InputText, Folder = new Folder() };
                    var newFolder =
                        await this.graphClient.Drive.Items[this.SelectedItem.Id].Children.Request()
                            .AddAsync(folderToCreate);

                    if (newFolder != null)
                    {
                        MessageBox.Show("Created new folder with ID " + newFolder.Id);
                        this.AddItemToFolderContents(newFolder);
                    }
                }
                catch (ServiceException exception)
                {
                    PresentServiceException(exception);

                }
                catch (Exception exception)
                {
                    PresentServiceException(exception);
                }
            }
        }

        private static void PresentServiceException(Exception exception)
        {
            string message = null;
            var oneDriveException = exception as ServiceException;
            if (oneDriveException == null)
            {
                message = exception.Message;
            }
            else
            {
                message = string.Format("{0}{1}", Environment.NewLine, oneDriveException.ToString());
            }

            MessageBox.Show(string.Format("OneDrive reported the following error: {0}", message));
        }

        private void deleteSelectedItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var itemToDelete = this.SelectedItem;
            var result = MessageBox.Show("Are you sure you want to delete " + itemToDelete.Name + "?", "Confirm Delete", MessageBoxButtons.YesNo);
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                try
                {
                    Browser.DeleteFilebyID(graphClient, itemToDelete.Id);
                    
                    RemoveItemFromFolderContents(itemToDelete);
                    MessageBox.Show("Item was deleted successfully");
                }
                catch (Exception exception)
                {
                    PresentServiceException(exception);
                }
            }
        }

        private async void getChangesHereToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var result =
                    await this.graphClient.Drive.Items[this.CurrentFolder.Id].Delta().Request().GetAsync();

                foreach (DriveItem item in result)
                {
                    Console.WriteLine(item.Name);
                }
            }
            catch (Exception ex)
            {
                PresentServiceException(ex);
            }
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void saveSelectedFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = this.SelectedItem;
            if (null == item)
            {
                MessageBox.Show("Nothing selected.");
                return;
            }

            var dialog = new SaveFileDialog();
            dialog.FileName = item.Name;
            dialog.Filter = "All Files (*.*)|*.*";
            var result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK)
                return;

            Browser.DownloadFilebyID(graphClient, item.Id, dialog.FileName);
        }

        private void shareSelectFileClick(object sender, EventArgs e)
        {
            var item = this.SelectedItem;
            if (null == item)
            {
                MessageBox.Show("Nothing selected.");
                return;
            }

            Permission linkPermission = Browser.CreateShareLink(graphClient, item.Id);
            MessageBox.Show(linkPermission.Link.WebUrl);
        }

        private void listShareSettings(object sender, EventArgs e)
        {
            var item = this.SelectedItem;
            if (null == item)
            {
                MessageBox.Show("Nothing selected.");
                return;
            }
            
            Permission [] arrayPermission = Browser.ListPermissions(graphClient, item.Id);
            String strMessage = "";
            for (int i = 0; i < arrayPermission.Length; i ++)
            {
                String roleString = "";
                foreach (var roles in arrayPermission[i].Roles)
                {
                    roleString = String.Format("{0}, {1}", roleString, roles);
                }
                String strElement = String.Format("{0}----[\"{1}\"]-----{2}\n", arrayPermission[i].Id, roleString, arrayPermission[i].Link.WebUrl);
                strMessage = strMessage + strElement; 
            }
            MessageBox.Show(strMessage);
        }
    }
}
