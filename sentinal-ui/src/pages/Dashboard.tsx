import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Document, Page } from 'react-pdf';
import { pdfjs } from 'react-pdf';
import 'react-pdf/dist/Page/AnnotationLayer.css';
import 'react-pdf/dist/Page/TextLayer.css';
import { apiClient, FileMetadata, FolderMetadata } from '../services/apiClient';
import '../styles/Dashboard.css';

// Set up pdfjs worker from node_modules
pdfjs.GlobalWorkerOptions.workerSrc = new URL(
  'pdfjs-dist/build/pdf.worker.min.mjs',
  import.meta.url,
).href;

export default function Dashboard() {
  const navigate = useNavigate();
  const [files, setFiles] = useState<FileMetadata[]>([]);
  const [subfolders, setSubfolders] = useState<FolderMetadata[]>([]);
  const [currentFolder, setCurrentFolder] = useState<FolderMetadata | null>(null);
  const [rootFolderId, setRootFolderId] = useState<string | null>(null);
  const [breadcrumbs, setBreadcrumbs] = useState<FolderMetadata[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [uploadFile, setUploadFile] = useState<File | null>(null);
  const [uploadError, setUploadError] = useState('');
  const [isUploading, setIsUploading] = useState(false);
  const [newFolderName, setNewFolderName] = useState('');
  const [showNewFolderForm, setShowNewFolderForm] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [isSearching, setIsSearching] = useState(false);
  const [showUploadForm, setShowUploadForm] = useState(false);
  const [isViewingRecycleBin, setIsViewingRecycleBin] = useState(false);
  const [previewFile, setPreviewFile] = useState<FileMetadata | null>(null);
  const [previewContent, setPreviewContent] = useState<string | Blob | null>(null);
  const [isLoadingPreview, setIsLoadingPreview] = useState(false);
  const [pdfPageCount, setPdfPageCount] = useState(0);
  const [pdfCurrentPage, setPdfCurrentPage] = useState(1);
  const [isEditingPreview, setIsEditingPreview] = useState(false);
  const [editingFileName, setEditingFileName] = useState('');
  const [editingDescription, setEditingDescription] = useState('');
  const [renamingFolderId, setRenamingFolderId] = useState<string | null>(null);
  const [renamingFolderName, setRenamingFolderName] = useState('');
  const [movingFile, setMovingFile] = useState<FileMetadata | null>(null);
  const [showMoveDialog, setShowMoveDialog] = useState(false);
  const [allFoldersForMove, setAllFoldersForMove] = useState<FolderMetadata[]>([]);
  const [isDarkMode, setIsDarkMode] = useState(() => {
    return localStorage.getItem('theme') === 'dark';
  });

  useEffect(() => {
    if (isDarkMode) {
      document.documentElement.setAttribute('data-theme', 'dark');
      localStorage.setItem('theme', 'dark');
    } else {
      document.documentElement.removeAttribute('data-theme');
      localStorage.setItem('theme', 'light');
    }
  }, [isDarkMode]);

  useEffect(() => {
    if (!apiClient.isAuthenticated()) {
      navigate('/login');
      return;
    }

    initializeDashboard();
  }, [navigate]);

  useEffect(() => {
    if (currentFolder) {
      loadFolderContents();
    }
  }, [currentFolder]);

  const initializeDashboard = async () => {
    try {
      setIsLoading(true);
      setError('');
      const allFolders = await apiClient.getFolders();
      const root = allFolders.find(f => f.parentFolderId === null);
      if (root) {
        setRootFolderId(root.id);
        setCurrentFolder(root);
        setBreadcrumbs([root]);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to initialize dashboard');
    } finally {
      setIsLoading(false);
    }
  };

  const loadFolderContents = async () => {
    if (!currentFolder) return;
    try {
      setIsLoading(true);
      setError('');

      const [filesData, subfoldersData] = await Promise.all([
        apiClient.getFiles(currentFolder.id),
        apiClient.getSubfolders(currentFolder.id),
      ]);

      setFiles(filesData);
      setSubfolders(subfoldersData);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load folder contents');
    } finally {
      setIsLoading(false);
    }
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files?.[0]) {
      setUploadFile(e.target.files[0]);
      setUploadError('');
    }
  };

  const handleFileUpload = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!uploadFile) {
      setUploadError('Please select a file');
      return;
    }

    // Check file size before uploading (30MB limit)
    const maxFileSize = 30 * 1024 * 1024; // 30MB in bytes
    if (uploadFile.size > maxFileSize) {
      setUploadError(
        `File is too large (${(uploadFile.size / (1024 * 1024)).toFixed(2)}MB). Maximum file size is 30MB.`
      );
      return;
    }

    try {
      setIsUploading(true);
      setUploadError('');

      await apiClient.uploadFile(
        uploadFile,
        uploadFile.name,
        uploadFile.type,
        currentFolder?.id
      );

      setUploadFile(null);
      const fileInput = document.getElementById('fileInput') as HTMLInputElement;
      if (fileInput) fileInput.value = '';

      await loadFolderContents();
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Upload failed';

      // Check if it's a file size error from the server
      if (errorMessage.includes('too large') || errorMessage.includes('body size')) {
        setUploadError('File is too large. Maximum file size is 30MB.');
      } else {
        setUploadError(errorMessage);
      }
    } finally {
      setIsUploading(false);
    }
  };

  const handleCreateFolder = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!newFolderName.trim()) {
      setError('Folder name cannot be empty');
      return;
    }

    try {
      await apiClient.createFolder(newFolderName, currentFolder?.id);
      setNewFolderName('');
      setShowNewFolderForm(false);
      await loadFolderContents();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create folder');
    }
  };

  const handleNavigateToFolder = (folder: FolderMetadata) => {
    setCurrentFolder(folder);
    setBreadcrumbs([...breadcrumbs, folder]);
  };

  const handleBackFolder = async () => {
    if (currentFolder?.parentFolderId) {
      try {
        const parentFolder = await apiClient.getFolder(currentFolder.parentFolderId);
        setCurrentFolder(parentFolder);
        setBreadcrumbs(breadcrumbs.slice(0, -1));
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to navigate back');
      }
    }
  };

  const handleSearch = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!searchQuery.trim()) {
      setIsSearching(false);
      setSearchQuery('');
      return;
    }

    try {
      setIsSearching(true);
      setError('');
      const results = await apiClient.searchFiles(searchQuery);
      setFiles(results);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to search files');
    }
  };

  const clearSearch = () => {
    setIsSearching(false);
    setSearchQuery('');
    setError('');
    if (currentFolder) {
      loadFolderContents();
    }
  };

  const handleViewRecycleBin = async () => {
    try {
      setIsLoading(true);
      setError('');
      const recycleBinFiles = await apiClient.getRecycleBinFiles();
      setFiles(recycleBinFiles);
      setSubfolders([]);
      setIsViewingRecycleBin(true);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load recycle bin');
    } finally {
      setIsLoading(false);
    }
  };

  const handleExitRecycleBin = () => {
    setIsViewingRecycleBin(false);
    setFiles([]);
    setSubfolders([]);
    if (currentFolder) {
      loadFolderContents();
    }
  };

  const handlePreviewFile = async (file: FileMetadata) => {
    try {
      setIsLoadingPreview(true);
      setPreviewFile(file);

      // Fetch file content
      const blob = await apiClient.downloadFile(file.fileId);

      // Handle different content types
      if (file.fileType.startsWith('image/')) {
        const url = window.URL.createObjectURL(blob);
        setPreviewContent(url);
      } else if (file.fileType === 'application/pdf') {
        setPreviewContent(blob);
      } else if (file.fileType === 'text/plain' || file.fileType === 'application/json') {
        const text = await blob.text();
        setPreviewContent(text);
      } else {
        setPreviewContent(blob);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to preview file');
    } finally {
      setIsLoadingPreview(false);
    }
  };

  const closePreview = () => {
    if (previewContent && typeof previewContent === 'string' && previewContent.startsWith('blob:')) {
      window.URL.revokeObjectURL(previewContent);
    }
    setPreviewFile(null);
    setPreviewContent(null);
    setPdfPageCount(0);
    setPdfCurrentPage(1);
    setIsEditingPreview(false);
  };

  const startEditingPreview = () => {
    if (previewFile) {
      setEditingFileName(previewFile.fileName);
      setEditingDescription(previewFile.fileDescription);
      setIsEditingPreview(true);
    }
  };

  const savePreviewEdits = async () => {
    if (!previewFile) return;

    try {
      // Update filename if changed
      if (editingFileName !== previewFile.fileName) {
        await apiClient.updateFileName(previewFile.fileId, editingFileName);
      }

      // Update description if changed
      if (editingDescription !== previewFile.fileDescription) {
        await apiClient.updateFileDescription(previewFile.fileId, editingDescription);
      }

      // Update preview file state
      setPreviewFile({
        ...previewFile,
        fileName: editingFileName,
        fileDescription: editingDescription,
      });

      setIsEditingPreview(false);
      await loadFolderContents();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to save changes');
    }
  };

  const saveFolderRename = async (folderId: string, newName: string) => {
    if (!newName.trim()) {
      setError('Folder name cannot be empty');
      return;
    }

    try {
      await apiClient.renameFolder(folderId, newName);
      setRenamingFolderId(null);
      setRenamingFolderName('');
      await loadFolderContents();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to rename folder');
    }
  };

  const handleDownloadFile = async (file: FileMetadata) => {
    try {
      const blob = await apiClient.downloadFile(file.fileId);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = file.fileName;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to download file');
    }
  };

  const handleDeleteFile = async (fileId: string) => {
    if (confirm('Are you sure you want to delete this file?')) {
      try {
        await apiClient.deleteFile(fileId);

        // If deleting from recycle bin, go back to root folder
        if (isViewingRecycleBin && rootFolderId) {
          setIsViewingRecycleBin(false);
          setCurrentFolder(null);
          // Initialize and navigate to root
          const allFolders = await apiClient.getFolders();
          const root = allFolders.find(f => f.parentFolderId === null);
          if (root) {
            setCurrentFolder(root);
            setBreadcrumbs([root]);
          }
        } else {
          await loadFolderContents();
        }
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to delete file');
      }
    }
  };

  const handleLogout = () => {
    apiClient.logout();
    navigate('/login');
  };

  const handleOpenMoveDialog = async (file: FileMetadata) => {
    try {
      const folders = await apiClient.getFolders();
      setAllFoldersForMove(folders);
      setMovingFile(file);
      setShowMoveDialog(true);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load folders');
    }
  };

  const handleMoveFileToFolder = async (destinationFolderId: string) => {
    if (!movingFile) return;

    try {
      await apiClient.moveFile(movingFile.fileId, destinationFolderId);
      setShowMoveDialog(false);
      setMovingFile(null);
      await loadFolderContents();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to move file');
    }
  };

  const handleDeleteFolder = async (folderId: string, folderName: string) => {
    if (confirm(`Are you sure you want to delete "${folderName}"?`)) {
      try {
        await apiClient.deleteFolder(folderId);
        await loadFolderContents();
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to delete folder');
      }
    }
  };

  const formatFileSize = (bytes: number): string => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  };

  const formatDate = (dateString: string): string => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const getFileExtension = (contentType: string): string => {
    const parts = contentType.split('/');
    return parts[1]?.toUpperCase() || contentType;
  };

  return (
    <div className="dashboard-container">
      <header className="dashboard-header">
        <h1
          onClick={() => {
            initializeDashboard();
            setIsSearching(false);
            setSearchQuery('');
            setIsViewingRecycleBin(false);
          }}
          style={{ cursor: 'pointer' }}
          title="Go to home folder"
        >
          Sentinal Dashboard
        </h1>
        <div className="header-buttons">
          <button
            onClick={() => setIsDarkMode(!isDarkMode)}
            className="theme-toggle-button"
            title={isDarkMode ? 'Switch to light mode' : 'Switch to dark mode'}
          >
            {isDarkMode ? '☀️' : '🌙'}
          </button>
          <button
            onClick={handleViewRecycleBin}
            className="recycle-bin-button"
            title="View deleted files and folders"
          >
            🗑️ Recycle Bin
          </button>
          <button onClick={handleLogout} className="logout-button">
            Logout
          </button>
        </div>
      </header>

      <main className="dashboard-main">
        <section className="explorer-section">
          {isViewingRecycleBin ? (
            <>
              <h2>🗑️ Recycle Bin</h2>
              <button
                onClick={handleExitRecycleBin}
                className="back-to-explorer-button"
              >
                ← Back to Explorer
              </button>
            </>
          ) : (
            <h2>File Explorer</h2>
          )}

          {!isViewingRecycleBin && (
            <>
          {/* Search Bar */}
          <form onSubmit={handleSearch} className="search-bar">
            <input
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              placeholder="Search files by name..."
              className="search-input"
            />
            <button type="submit" className="search-button">
              🔍 Search
            </button>
            {isSearching && (
              <button
                type="button"
                onClick={clearSearch}
                className="clear-search-button"
              >
                Clear
              </button>
            )}
          </form>

          {isSearching && (
            <div className="search-status">
              Search results for: <strong>{searchQuery}</strong>
            </div>
          )}

          {/* Navigation Bar */}
          {!isSearching && (
            <>
              <div className="navigation-bar">
                <button
                  onClick={handleBackFolder}
                  disabled={!currentFolder?.parentFolderId}
                  className="back-button"
                  title="Go back to parent folder"
                >
                  ← Back
                </button>
                <div className="breadcrumb">
                  {breadcrumbs.map((breadcrumb, index) => (
                    <div key={breadcrumb.id} className="breadcrumb-item">
                      <button
                        onClick={() => {
                          setCurrentFolder(breadcrumb);
                          setBreadcrumbs(breadcrumbs.slice(0, index + 1));
                        }}
                        className={index === breadcrumbs.length - 1 ? 'active' : ''}
                      >
                        {breadcrumb.name}
                      </button>
                      {index < breadcrumbs.length - 1 && <span className="separator">/</span>}
                    </div>
                  ))}
                </div>
              </div>

              {/* New Folder & Upload Controls */}
              <div className="explorer-controls">
                {showNewFolderForm ? (
                  <form onSubmit={handleCreateFolder} className="new-folder-form">
                    <input
                      type="text"
                      value={newFolderName}
                      onChange={(e) => setNewFolderName(e.target.value)}
                      placeholder="Folder name"
                      autoFocus
                    />
                    <button type="submit">Create</button>
                    <button
                      type="button"
                      onClick={() => {
                        setShowNewFolderForm(false);
                        setNewFolderName('');
                      }}
                    >
                      Cancel
                    </button>
                  </form>
                ) : (
                  <button
                    onClick={() => setShowNewFolderForm(true)}
                    className="new-folder-button"
                  >
                    + New Folder
                  </button>
                )}

                {showUploadForm ? (
                  <form onSubmit={handleFileUpload} className="upload-file-form">
                    <input
                      id="fileInput"
                      type="file"
                      onChange={handleFileChange}
                      disabled={isUploading}
                      className="file-input"
                    />
                    <label htmlFor="fileInput" className="file-label">
                      {uploadFile ? uploadFile.name : 'Choose a file'}
                    </label>
                    <button
                      type="submit"
                      disabled={!uploadFile || isUploading}
                      className="upload-button"
                    >
                      {isUploading ? 'Uploading...' : 'Upload'}
                    </button>
                    <button
                      type="button"
                      onClick={() => {
                        setShowUploadForm(false);
                        setUploadFile(null);
                      }}
                      className="cancel-button"
                    >
                      Cancel
                    </button>
                  </form>
                ) : (
                  <button
                    onClick={() => setShowUploadForm(true)}
                    className="upload-file-button"
                  >
                    + Upload File
                  </button>
                )}
                {uploadError && <div className="error-message">{uploadError}</div>}
              </div>
            </>
          )}
            </>
          )}

          {/* Folders and Files List */}
          {isLoading ? (
            <p className="loading">Loading...</p>
          ) : error ? (
            <div className="error-message">{error}</div>
          ) : subfolders.length === 0 && files.length === 0 ? (
            <p className="empty-state">This folder is empty</p>
          ) : (
            <div className="items-table">
              <table>
                <thead>
                  <tr>
                    <th>Name</th>
                    <th>Type</th>
                    <th>Description</th>
                    <th>Created</th>
                    <th>Updated</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {/* Folders (only show when not searching or viewing recycle bin, exclude system folders) */}
                  {!isSearching &&
                    !isViewingRecycleBin &&
                    subfolders
                      .filter(
                        (folder) =>
                          !folder.name.endsWith('_RecycleBin') &&
                          !folder.name.endsWith('_History')
                      )
                      .map((folder) => (
                    <tr key={`folder-${folder.id}`} className="folder-row">
                      <td className="item-name folder">
                        {renamingFolderId === folder.id ? (
                          <div className="rename-input-group">
                            <input
                              type="text"
                              value={renamingFolderName}
                              onChange={(e) => setRenamingFolderName(e.target.value)}
                              autoFocus
                              className="rename-input"
                            />
                            <button
                              onClick={() => saveFolderRename(folder.id, renamingFolderName)}
                              className="action-button download"
                              style={{ padding: '0.25rem 0.75rem', fontSize: '0.8rem' }}
                            >
                              Save
                            </button>
                            <button
                              onClick={() => setRenamingFolderId(null)}
                              className="action-button cancel-button"
                              style={{ padding: '0.25rem 0.75rem', fontSize: '0.8rem' }}
                            >
                              Cancel
                            </button>
                          </div>
                        ) : (
                          <>
                            <span className="folder-icon">📁</span>
                            <button
                              onClick={() => handleNavigateToFolder(folder)}
                              className="folder-link"
                            >
                              {folder.name}
                            </button>
                          </>
                        )}
                      </td>
                      <td>Folder</td>
                      <td>{/* Empty description column for folders */}</td>
                      <td>{formatDate(folder.createdAt)}</td>
                      <td>{formatDate(folder.updatedAt)}</td>
                      <td className="actions">
                        {renamingFolderId !== folder.id && (
                          <>
                            <button
                              onClick={() => {
                                setRenamingFolderId(folder.id);
                                setRenamingFolderName(folder.name);
                              }}
                              className="action-button download"
                            >
                              Rename
                            </button>
                            <button
                              onClick={() => handleDeleteFolder(folder.id, folder.name)}
                              className="action-button delete"
                            >
                              Delete
                            </button>
                          </>
                        )}
                      </td>
                    </tr>
                  ))}
                  {/* Files */}
                  {files.map((file) => (
                    <tr key={`file-${file.fileId}`} className="file-row">
                      <td className="item-name file">
                        <span className="file-icon">📄</span>
                        <button
                          onClick={() => handlePreviewFile(file)}
                          className="file-link"
                          title="Click to preview"
                        >
                          {file.fileName}
                        </button>
                      </td>
                      <td>{getFileExtension(file.fileType)}</td>
                      <td className="file-description">
                        {file.fileDescription || <span className="no-description">—</span>}
                      </td>
                      <td>{formatDate(file.dateCreated)}</td>
                      <td>{formatDate(file.dateUpdated)}</td>
                      <td className="actions">
                        <button
                          onClick={() => handleDownloadFile(file)}
                          className="action-button download"
                        >
                          Download
                        </button>
                        <button
                          onClick={() => handleOpenMoveDialog(file)}
                          className="action-button download"
                        >
                          Move
                        </button>
                        <button
                          onClick={() => handleDeleteFile(file.fileId)}
                          className="action-button delete"
                        >
                          Delete
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </section>

        {/* File Preview Modal */}
        {previewFile && (
          <div className="preview-overlay" onClick={closePreview}>
            <div className="preview-modal" onClick={(e) => e.stopPropagation()}>
              <div className="preview-header">
                {isEditingPreview ? (
                  <div className="preview-edit-form">
                    <div className="edit-field">
                      <label>Filename</label>
                      <input
                        type="text"
                        value={editingFileName}
                        onChange={(e) => setEditingFileName(e.target.value)}
                        className="edit-input"
                      />
                    </div>
                    <div className="edit-field">
                      <label>Description</label>
                      <textarea
                        value={editingDescription}
                        onChange={(e) => setEditingDescription(e.target.value)}
                        className="edit-textarea"
                        rows={2}
                        placeholder="Add a description..."
                      />
                    </div>
                    <div className="edit-buttons">
                      <button onClick={savePreviewEdits} className="action-button download">
                        Save
                      </button>
                      <button
                        onClick={() => setIsEditingPreview(false)}
                        className="action-button cancel-button"
                      >
                        Cancel
                      </button>
                    </div>
                  </div>
                ) : (
                  <>
                    <div className="preview-header-content">
                      <p style={{ margin: 0, fontSize: '0.85rem', color: '#999', marginBottom: '0.5rem' }}>📄 File Preview</p>
                      <h2>{previewFile.fileName}</h2>
                      {previewFile.fileDescription && (
                        <p className="preview-description">{previewFile.fileDescription}</p>
                      )}
                    </div>
                    <div className="preview-header-buttons">
                      <button
                        onClick={startEditingPreview}
                        className="edit-button"
                        title="Edit filename and description"
                      >
                        ✎ Edit
                      </button>
                      <button className="close-button" onClick={closePreview}>
                        ✕
                      </button>
                    </div>
                  </>
                )}
              </div>
              <div className="preview-content">
                {isLoadingPreview ? (
                  <p className="loading">Loading preview...</p>
                ) : previewFile.fileType.startsWith('image/') && typeof previewContent === 'string' ? (
                  <img src={previewContent} alt={previewFile.fileName} className="preview-image" />
                ) : (previewFile.fileType === 'text/plain' || previewFile.fileType === 'application/json') &&
                  typeof previewContent === 'string' ? (
                  <pre className="preview-text">{previewContent}</pre>
                ) : previewFile.fileType === 'application/pdf' && previewContent instanceof Blob ? (
                  <div className="preview-pdf-container">
                    <Document
                      file={previewContent}
                      onLoadSuccess={({ numPages }) => setPdfPageCount(numPages)}
                      loading={<p>Loading PDF...</p>}
                      error={<p>Error loading PDF</p>}
                    >
                      <Page pageNumber={pdfCurrentPage} />
                    </Document>
                    {pdfPageCount > 1 && (
                      <div className="pdf-controls">
                        <button
                          onClick={() => setPdfCurrentPage(Math.max(1, pdfCurrentPage - 1))}
                          disabled={pdfCurrentPage === 1}
                          className="pdf-button"
                        >
                          ← Previous
                        </button>
                        <span className="pdf-page-info">
                          Page {pdfCurrentPage} of {pdfPageCount}
                        </span>
                        <button
                          onClick={() => setPdfCurrentPage(Math.min(pdfPageCount, pdfCurrentPage + 1))}
                          disabled={pdfCurrentPage === pdfPageCount}
                          className="pdf-button"
                        >
                          Next →
                        </button>
                      </div>
                    )}
                  </div>
                ) : (
                  <div className="preview-unsupported">
                    <p>Preview not available for this file type.</p>
                    <button
                      onClick={() => handleDownloadFile(previewFile)}
                      className="action-button download"
                    >
                      Download File
                    </button>
                  </div>
                )}
              </div>
            </div>
          </div>
        )}

        {/* Move File Dialog */}
        {showMoveDialog && movingFile && (
          <div className="preview-overlay" onClick={() => setShowMoveDialog(false)}>
            <div className="preview-modal" onClick={(e) => e.stopPropagation()}>
              <div className="preview-header">
                <div className="preview-header-content">
                  <h2>Move File</h2>
                  <p>Select a destination folder for: <strong>{movingFile.fileName}</strong></p>
                </div>
                <button className="close-button" onClick={() => setShowMoveDialog(false)}>
                  ✕
                </button>
              </div>
              <div className="preview-content" style={{ maxHeight: '400px', overflowY: 'auto' }}>
                <div className="folder-list">
                  {allFoldersForMove
                    .filter(
                      (folder) =>
                        folder.id !== currentFolder?.id &&
                        !folder.name.endsWith('_RecycleBin') &&
                        !folder.name.endsWith('_History')
                    )
                    .map((folder) => (
                      <div key={folder.id} className="folder-option">
                        <button
                          onClick={() => handleMoveFileToFolder(folder.id)}
                          className="folder-select-button"
                        >
                          📁 {folder.name}
                        </button>
                      </div>
                    ))}
                </div>
              </div>
            </div>
          </div>
        )}
      </main>
    </div>
  );
}