# Sentinal UI

A React TypeScript file management application for secure file storage and sharing.

## Project Structure

```
sentinal-ui/
├── src/
│   ├── pages/              # Page components
│   │   ├── Login.tsx       # User authentication
│   │   └── Dashboard.tsx   # File management interface
│   ├── services/           # API integration
│   │   └── apiClient.ts    # .NET API wrapper
│   ├── styles/             # Component styles
│   │   ├── Login.css
│   │   └── Dashboard.css
│   ├── App.tsx             # Main router component
│   ├── main.tsx            # Application entry point
│   ├── index.css           # Global styles
│   └── vite-env.d.ts       # Vite type definitions
├── vite.config.ts          # Vite configuration
├── tsconfig.json           # TypeScript configuration
├── package.json            # Dependencies
└── .env                    # Environment variables
```

## Prerequisites

- Node.js 16+ with npm
- .NET API running on `http://localhost:5230`

## Setup

1. **Install dependencies**:
   ```bash
   npm install
   ```

2. **Configure API URL** (optional, defaults to localhost):
   ```bash
   cp .env.example .env
   # Edit .env to change VITE_API_URL if needed
   ```

3. **Start development server**:
   ```bash
   npm run dev
   ```
   The app will open at `http://localhost:5173` (or the next available port)

## Available Scripts

- `npm run dev` - Start the development server with hot module replacement
- `npm run build` - Build the application for production
- `npm run preview` - Preview the production build locally
- `npm run lint` - Run ESLint to check code quality

## Features

### Authentication
- **Login**: Enter username/email and password to authenticate
- **JWT Storage**: Auth token is stored in `localStorage` under `authToken`
- **Session Persistence**: User stays logged in until logout or token expiration

### File Management
- **Upload Files**: Select and upload files to the current folder
- **Download Files**: Download individual files from storage
- **Delete Files**: Soft delete files (retained for 7 days)
- **File Listing**: View files with metadata (size, type, creation date)

### Folder Management
- **Create Folders**: Create new folders within the current directory
- **Folder Navigation**: Navigate through folder hierarchy
- **Breadcrumb Navigation**: Visual folder path indication

### Search & Organization
- **Search Files**: Search for files by name
- **Folder Navigation**: Organize files into folder hierarchies
- **Special Folders**: Access RecycleBin and History folders

## API Integration

The `apiClient.ts` service provides the following methods:

```typescript
// Authentication
login(credentials: LoginRequest): Promise<LoginResponse>
logout(): Promise<void>
isAuthenticated(): boolean

// Files
uploadFile(file, fileName, contentType, folderId?, description?): Promise<FileMetadata>
getFiles(folderId?): Promise<FileMetadata[]>
downloadFile(fileId): Promise<Blob>
deleteFile(fileId): Promise<void>
updateFileName(fileId, newFileName): Promise<void>
searchFiles(query): Promise<FileMetadata[]>

// Folders
createFolder(folderName, parentFolderId?): Promise<FolderMetadata>
getFolders(): Promise<FolderMetadata[]>
getFolder(folderId): Promise<FolderMetadata>
```

## Environment Variables

Create a `.env` file in the root with:

```env
VITE_API_URL=http://localhost:5230/api
```

For Docker deployment, set `VITE_API_URL` to the appropriate API endpoint.

## Styling

The application uses:
- **Custom CSS** for responsive, styled components
- **CSS Grid/Flexbox** for layouts
- **Gradient Backgrounds** for modern UI design
- **Mobile-responsive** design for all screen sizes

## Type Safety

Full TypeScript support with:
- Strict type checking enabled
- Type definitions for all API responses
- React Router type definitions
- DOM and Node.js type definitions

## Browser Support

Modern browsers supporting:
- ES2020 JavaScript
- CSS Grid and Flexbox
- Local Storage API
- File API (for uploads)
- Blob API (for downloads)

## Development Notes

- The app redirects unauthenticated users to `/login`
- JWT token is validated on API calls via the `Authorization: Bearer` header
- All file operations are scoped to the logged-in user
- Soft deletes prevent accidental data loss (7-day retention)
- API errors are displayed to the user with clear messaging

## Building for Production

```bash
npm run build
```

This creates an optimized build in the `dist/` directory. Deploy the contents to a static hosting service or web server.
