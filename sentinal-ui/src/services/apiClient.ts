const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5230/api';

export interface LoginRequest {
  username?: string;
  email?: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}

export interface LoginResponse {
  id: string;
  username: string;
  email: string;
  token: string;
}

export interface FileMetadata {
  fileId: string;
  fileName: string;
  fileType: string;
  fileDescription: string;
  folderId: string;
  dateCreated: string;
  dateUpdated: string;
}

export interface FolderMetadata {
  id: string;
  name: string;
  parentFolderId: string | null;
  createdAt: string;
  updatedAt: string;
}

class ApiClient {
  private getAuthToken(): string | null {
    return localStorage.getItem('authToken');
  }

  private getAuthHeader(): HeadersInit {
    const token = this.getAuthToken();
    return {
      'Authorization': token ? `Bearer ${token}` : '',
    };
  }

  async login(credentials: LoginRequest): Promise<LoginResponse> {
    const response = await fetch(`${API_BASE_URL}/User/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(credentials),
    });

    if (!response.ok) {
      try {
        const error = await response.json();
        throw new Error(error.message || `Login failed: ${response.statusText}`);
      } catch {
        throw new Error(`Login failed: ${response.statusText}`);
      }
    }

    const data = await response.json();
    localStorage.setItem('authToken', data.token);
    return data;
  }

  async register(credentials: RegisterRequest): Promise<LoginResponse> {
    const response = await fetch(`${API_BASE_URL}/User/register`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(credentials),
    });

    if (!response.ok) {
      try {
        const error = await response.json();
        throw new Error(error.message || `Registration failed: ${response.statusText}`);
      } catch {
        throw new Error(`Registration failed: ${response.statusText}`);
      }
    }

    const data = await response.json();
    localStorage.setItem('authToken', data.token);
    return data;
  }

  async logout(): Promise<void> {
    localStorage.removeItem('authToken');
  }

  async uploadFile(
    file: File,
    fileName: string,
    contentType: string,
    folderId?: string,
    description?: string
  ): Promise<FileMetadata> {
    const formData = new FormData();
    formData.append('File', file);
    formData.append('FileName', fileName);
    formData.append('ContentType', contentType);
    if (folderId) {
      formData.append('FolderId', folderId);
    }
    if (description) {
      formData.append('Description', description);
    }

    const response = await fetch(`${API_BASE_URL}/File`, {
      method: 'POST',
      headers: this.getAuthHeader() as HeadersInit,
      body: formData,
    });

    if (!response.ok) {
      throw new Error(`File upload failed: ${response.statusText}`);
    }

    return response.json();
  }

  async getFiles(folderId?: string): Promise<FileMetadata[]> {
    const url = folderId
      ? `${API_BASE_URL}/File/AllFilesInFolder/${folderId}`
      : `${API_BASE_URL}/File/Allfiles`;

    const response = await fetch(url, {
      method: 'GET',
      headers: this.getAuthHeader() as HeadersInit,
    });

    if (!response.ok) {
      try {
        const error = await response.json();
        if (Array.isArray(error) && error.some((e: any) => e.message?.includes('No Files found'))) {
          return [];
        }
      } catch {
        // If we can't parse the error, check status code
      }
      // Return empty array for 400/404 errors (no files found)
      if (response.status === 400 || response.status === 404) {
        return [];
      }
      throw new Error(`Failed to fetch files: ${response.statusText}`);
    }

    return response.json();
  }

  async getFolders(): Promise<FolderMetadata[]> {
    const response = await fetch(`${API_BASE_URL}/Folder/AllFolders`, {
      method: 'GET',
      headers: this.getAuthHeader() as HeadersInit,
    });

    if (!response.ok) {
      const error = await response.json();
      if (Array.isArray(error) && error[0]?.message === 'No Folders found') {
        return [];
      }
      throw new Error(`Failed to fetch folders: ${response.statusText}`);
    }

    return response.json();
  }

  async createFolder(folderName: string, parentId?: string): Promise<FolderMetadata> {
    const payload: Record<string, string> = {
      folderName: folderName,
    };
    if (parentId) {
      payload.parentId = parentId;
    }

    const response = await fetch(`${API_BASE_URL}/Folder`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...this.getAuthHeader(),
      } as HeadersInit,
      body: JSON.stringify(payload),
    });

    if (!response.ok) {
      throw new Error(`Failed to create folder: ${response.statusText}`);
    }

    return response.json();
  }

  async getFolder(folderId: string): Promise<FolderMetadata> {
    const response = await fetch(`${API_BASE_URL}/Folder/${folderId}`, {
      method: 'GET',
      headers: this.getAuthHeader() as HeadersInit,
    });

    if (!response.ok) {
      throw new Error(`Failed to fetch folder: ${response.statusText}`);
    }

    return response.json();
  }

  async downloadFile(fileId: string): Promise<Blob> {
    const response = await fetch(`${API_BASE_URL}/File/${fileId}`, {
      method: 'GET',
      headers: this.getAuthHeader() as HeadersInit,
    });

    if (!response.ok) {
      throw new Error(`Failed to download file: ${response.statusText}`);
    }

    return response.blob();
  }

  async deleteFile(fileId: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/File/${fileId}`, {
      method: 'DELETE',
      headers: this.getAuthHeader() as HeadersInit,
    });

    if (!response.ok) {
      throw new Error(`Failed to delete file: ${response.statusText}`);
    }
  }

  async updateFileName(fileId: string, newFileName: string): Promise<FileMetadata> {
    const response = await fetch(`${API_BASE_URL}/File/UpdateFileName`, {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json',
        ...this.getAuthHeader(),
      } as HeadersInit,
      body: JSON.stringify({ fileId, newName: newFileName }),
    });

    if (!response.ok) {
      throw new Error(`Failed to update file name: ${response.statusText}`);
    }

    return response.json();
  }

  async searchFiles(searchTerm: string): Promise<FileMetadata[]> {
    const params = new URLSearchParams({ searchTerm });
    const response = await fetch(`${API_BASE_URL}/File/SearchFileByName?${params}`, {
      method: 'GET',
      headers: this.getAuthHeader() as HeadersInit,
    });

    if (!response.ok) {
      try {
        const error = await response.json();
        if (Array.isArray(error) && error.some((e: any) => e.message?.includes('No Files found'))) {
          return [];
        }
      } catch {
        // If we can't parse the error, check status code
      }
      // Return empty array for 400/404 errors (no files found)
      if (response.status === 400 || response.status === 404) {
        return [];
      }
      throw new Error(`Failed to search files: ${response.statusText}`);
    }

    return response.json();
  }

  async getRecycleBinFiles(): Promise<FileMetadata[]> {
    const response = await fetch(`${API_BASE_URL}/File/AllFilesInRecycleBin`, {
      method: 'GET',
      headers: this.getAuthHeader() as HeadersInit,
    });

    if (!response.ok) {
      const error = await response.json();
      if (Array.isArray(error) && error[0]?.message === 'No Files found') {
        return [];
      }
      throw new Error(`Failed to fetch recycle bin files: ${response.statusText}`);
    }

    return response.json();
  }

  async updateFileDescription(fileId: string, newDescription: string | null): Promise<FileMetadata> {
    const response = await fetch(`${API_BASE_URL}/File/UpdateFileDescription`, {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json',
        ...this.getAuthHeader(),
      } as HeadersInit,
      body: JSON.stringify({ fileId, newDescription }),
    });

    if (!response.ok) {
      throw new Error(`Failed to update file description: ${response.statusText}`);
    }

    return response.json();
  }

  async moveFile(fileId: string, destinationFolderId: string): Promise<FileMetadata> {
    const response = await fetch(`${API_BASE_URL}/File/MoveFile`, {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json',
        ...this.getAuthHeader(),
      } as HeadersInit,
      body: JSON.stringify({ fileId, destinationFolderId }),
    });

    if (!response.ok) {
      throw new Error(`Failed to move file: ${response.statusText}`);
    }

    return response.json();
  }

  async updateFileContent(fileId: string, file: File, description?: string): Promise<FileMetadata> {
    const formData = new FormData();
    formData.append('FileId', fileId);
    formData.append('File', file);
    if (description) {
      formData.append('Description', description);
    }

    const response = await fetch(`${API_BASE_URL}/File`, {
      method: 'PUT',
      headers: this.getAuthHeader() as HeadersInit,
      body: formData,
    });

    if (!response.ok) {
      throw new Error(`Failed to update file content: ${response.statusText}`);
    }

    return response.json();
  }

  async deleteFolder(folderId: string): Promise<{ success: boolean }> {
    const response = await fetch(`${API_BASE_URL}/Folder/${folderId}`, {
      method: 'DELETE',
      headers: this.getAuthHeader() as HeadersInit,
    });

    if (!response.ok) {
      throw new Error(`Failed to delete folder: ${response.statusText}`);
    }

    return response.json();
  }

  async renameFolder(folderId: string, newName: string): Promise<FolderMetadata> {
    const response = await fetch(`${API_BASE_URL}/Folder/${folderId}/Name`, {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json',
        ...this.getAuthHeader(),
      } as HeadersInit,
      body: JSON.stringify({ newName }),
    });

    if (!response.ok) {
      throw new Error(`Failed to rename folder: ${response.statusText}`);
    }

    return response.json();
  }

  async moveFolder(folderId: string, destinationParentFolderId: string): Promise<FolderMetadata> {
    const response = await fetch(`${API_BASE_URL}/Folder/${folderId}/Move`, {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json',
        ...this.getAuthHeader(),
      } as HeadersInit,
      body: JSON.stringify({ destinationParentFolderId }),
    });

    if (!response.ok) {
      throw new Error(`Failed to move folder: ${response.statusText}`);
    }

    return response.json();
  }

  async getSubfolders(folderId: string): Promise<FolderMetadata[]> {
    const response = await fetch(`${API_BASE_URL}/Folder/Subfolders/${folderId}`, {
      method: 'GET',
      headers: this.getAuthHeader() as HeadersInit,
    });

    if (!response.ok) {
       try {
        const error = await response.json();
        if (Array.isArray(error) && error.some((e: any) => e.message?.includes('No Folders found'))) {
          return [];
        }
      } catch {
        // If we can't parse the error, check status code
      }
      // Return empty array for 400/404 errors (no subfolders found)
      if (response.status === 400 || response.status === 404) {
        return [];
      }
      throw new Error(`Failed to fetch subfolders: ${response.statusText}`);
    }

    return response.json();
  }

  async getRecycleBinFolder(): Promise<FolderMetadata> {
    const response = await fetch(`${API_BASE_URL}/Folder/RecycleBin`, {
      method: 'GET',
      headers: this.getAuthHeader() as HeadersInit,
    });

    if (!response.ok) {
      throw new Error(`Failed to fetch recycle bin folder: ${response.statusText}`);
    }

    return response.json();
  }

  async searchFolders(searchTerm: string): Promise<FolderMetadata[]> {
    const response = await fetch(`${API_BASE_URL}/Folder/SearchFolderByName/${encodeURIComponent(searchTerm)}`, {
      method: 'GET',
      headers: this.getAuthHeader() as HeadersInit,
    });

    if (!response.ok) {
      const error = await response.json();
      if (Array.isArray(error) && error[0]?.message === 'No Folders found') {
        return [];
      }
      throw new Error(`Failed to search folders: ${response.statusText}`);
    }

    return response.json();
  }

  async updatePassword(currentPassword: string, newPassword: string): Promise<{ success: boolean }> {
    const response = await fetch(`${API_BASE_URL}/User/update-password`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...this.getAuthHeader(),
      } as HeadersInit,
      body: JSON.stringify({ currentPassword, newPassword }),
    });

    if (!response.ok) {
      throw new Error(`Failed to update password: ${response.statusText}`);
    }

    return response.json();
  }

  async updateEmail(newEmail: string): Promise<LoginResponse> {
    const response = await fetch(`${API_BASE_URL}/User/update-email`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...this.getAuthHeader(),
      } as HeadersInit,
      body: JSON.stringify({ newEmail }),
    });

    if (!response.ok) {
      throw new Error(`Failed to update email: ${response.statusText}`);
    }

    const data = await response.json();
    localStorage.setItem('authToken', data.token);
    return data;
  }

  async updateUsername(newUsername: string): Promise<LoginResponse> {
    const response = await fetch(`${API_BASE_URL}/User/update-username`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...this.getAuthHeader(),
      } as HeadersInit,
      body: JSON.stringify({ newUsername }),
    });

    if (!response.ok) {
      throw new Error(`Failed to update username: ${response.statusText}`);
    }

    const data = await response.json();
    localStorage.setItem('authToken', data.token);
    return data;
  }

  async confirmEmail(): Promise<{ success: boolean }> {
    const response = await fetch(`${API_BASE_URL}/User/confirm-email`, {
      method: 'POST',
      headers: this.getAuthHeader() as HeadersInit,
      body: JSON.stringify({}),
    });

    if (!response.ok) {
      throw new Error(`Failed to confirm email: ${response.statusText}`);
    }

    return response.json();
  }

  async deleteUser(userId: string): Promise<{ success: boolean }> {
    const response = await fetch(`${API_BASE_URL}/User/${userId}`, {
      method: 'DELETE',
      headers: this.getAuthHeader() as HeadersInit,
    });

    if (!response.ok) {
      throw new Error(`Failed to delete user: ${response.statusText}`);
    }

    return response.json();
  }

  isAuthenticated(): boolean {
    return !!this.getAuthToken();
  }
}

export const apiClient = new ApiClient();