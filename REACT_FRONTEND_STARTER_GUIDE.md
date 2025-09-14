# Getting Started: .NET API + React Project Template

## Quick Start Guide

This guide shows you exactly how to create a modern web application using our secure .NET API backend with a React frontend.

## Project Structure

```
my-web-project/
├── backend/                 # .NET Web API (like our Hotel API)
│   ├── Controllers/
│   ├── Models/
│   ├── Services/
│   ├── Security/
│   └── Program.cs
├── frontend/               # React.js application
│   ├── src/
│   │   ├── components/
│   │   ├── pages/
│   │   ├── services/
│   │   ├── hooks/
│   │   └── types/
│   ├── public/
│   └── package.json
└── README.md
```

## Step 1: Backend Setup (Using Our API)

You already have a solid backend! Our Hotel Booking API provides:

```csharp
// What you already have:
✅ JWT Authentication
✅ Role-based Authorization  
✅ Rate Limiting & DDoS Protection
✅ Input Validation
✅ Security Headers
✅ MongoDB Integration
✅ Health Checks
✅ CORS Configuration
```

## Step 2: Create React Frontend

```bash
# Create React app with TypeScript
npx create-react-app hotel-booking-frontend --template typescript
cd hotel-booking-frontend

# Install essential packages
npm install @tanstack/react-query axios zustand
npm install @types/node @types/react @types/react-dom
npm install react-router-dom @types/react-router-dom

# Install UI library (choose one)
npm install @mui/material @emotion/react @emotion/styled  # Material-UI
# OR
npm install tailwindcss @headlessui/react  # Tailwind CSS
```

## Step 3: API Service Layer

Create `src/services/api.ts`:

```typescript
// src/services/api.ts
export interface User {
  id: string;
  username: string;
  email: string;
  roles: string[];
}

export interface Room {
  id: string;
  name: string;
  capacity: number;
  pricePerNight: number;
  isAvailable: boolean;
  roomType?: string;
  description?: string;
}

export interface AuthResponse {
  token: string;
  user: User;
  expiresAt: string;
}

export class ApiError extends Error {
  constructor(public status: number, message: string) {
    super(message);
    this.name = 'ApiError';
  }
}

export class RateLimitError extends ApiError {
  constructor(message: string, public retryAfter?: number) {
    super(429, message);
    this.name = 'RateLimitError';
  }
}

export class HotelBookingAPI {
  private baseURL = process.env.REACT_APP_API_URL || 'http://localhost:5000/api';
  private token: string | null = null;

  constructor() {
    // Load token from localStorage on init
    this.token = localStorage.getItem('authToken');
  }

  private async makeRequest<T>(
    endpoint: string, 
    options: RequestInit = {}
  ): Promise<T> {
    const url = `${this.baseURL}${endpoint}`;
    
    const config: RequestInit = {
      ...options,
      headers: {
        'Content-Type': 'application/json',
        ...(this.token && { Authorization: `Bearer ${this.token}` }),
        ...options.headers,
      },
    };

    try {
      const response = await fetch(url, config);
      
      // Handle rate limiting
      if (response.status === 429) {
        const retryAfter = response.headers.get('Retry-After');
        throw new RateLimitError(
          'Too many requests. Please try again later.',
          retryAfter ? parseInt(retryAfter) : undefined
        );
      }

      if (!response.ok) {
        throw new ApiError(response.status, `HTTP ${response.status}: ${response.statusText}`);
      }

      return await response.json();
    } catch (error) {
      if (error instanceof ApiError || error instanceof RateLimitError) {
        throw error;
      }
      throw new ApiError(0, 'Network error or server unavailable');
    }
  }

  // Authentication
  async login(username: string, password: string): Promise<AuthResponse> {
    const response = await this.makeRequest<AuthResponse>('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ username, password }),
    });
    
    this.token = response.token;
    localStorage.setItem('authToken', response.token);
    return response;
  }

  async register(username: string, email: string, password: string): Promise<AuthResponse> {
    const response = await this.makeRequest<AuthResponse>('/auth/register', {
      method: 'POST',
      body: JSON.stringify({ username, email, password }),
    });
    
    this.token = response.token;
    localStorage.setItem('authToken', response.token);
    return response;
  }

  logout(): void {
    this.token = null;
    localStorage.removeItem('authToken');
  }

  // Rooms API
  async getRooms(): Promise<Room[]> {
    return this.makeRequest<Room[]>('/rooms');
  }

  async getRoom(id: string): Promise<Room> {
    return this.makeRequest<Room>(`/rooms/${id}`);
  }

  async createRoom(room: Omit<Room, 'id'>): Promise<Room> {
    return this.makeRequest<Room>('/rooms', {
      method: 'POST',
      body: JSON.stringify(room),
    });
  }

  async updateRoom(id: string, room: Partial<Room>): Promise<Room> {
    return this.makeRequest<Room>(`/rooms/${id}`, {
      method: 'PUT',
      body: JSON.stringify(room),
    });
  }

  async deleteRoom(id: string): Promise<void> {
    await this.makeRequest(`/rooms/${id}`, {
      method: 'DELETE',
    });
  }
}

// Singleton instance
export const api = new HotelBookingAPI();
```

## Step 4: Authentication Hook

Create `src/hooks/useAuth.ts`:

```typescript
// src/hooks/useAuth.ts
import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { api, User, AuthResponse } from '../services/api';

interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
  login: (username: string, password: string) => Promise<void>;
  register: (username: string, email: string, password: string) => Promise<void>;
  logout: () => void;
  clearError: () => void;
  hasRole: (role: string) => boolean;
}

export const useAuth = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      isAuthenticated: false,
      isLoading: false,
      error: null,

      login: async (username: string, password: string) => {
        set({ isLoading: true, error: null });
        
        try {
          const response: AuthResponse = await api.login(username, password);
          set({
            user: response.user,
            isAuthenticated: true,
            isLoading: false,
            error: null,
          });
        } catch (error) {
          set({
            error: error instanceof Error ? error.message : 'Login failed',
            isLoading: false,
            isAuthenticated: false,
            user: null,
          });
          throw error;
        }
      },

      register: async (username: string, email: string, password: string) => {
        set({ isLoading: true, error: null });
        
        try {
          const response: AuthResponse = await api.register(username, email, password);
          set({
            user: response.user,
            isAuthenticated: true,
            isLoading: false,
            error: null,
          });
        } catch (error) {
          set({
            error: error instanceof Error ? error.message : 'Registration failed',
            isLoading: false,
            isAuthenticated: false,
            user: null,
          });
          throw error;
        }
      },

      logout: () => {
        api.logout();
        set({
          user: null,
          isAuthenticated: false,
          error: null,
        });
      },

      clearError: () => set({ error: null }),

      hasRole: (role: string) => {
        const { user } = get();
        return user?.roles?.includes(role) || false;
      },
    }),
    {
      name: 'auth-storage',
      partialize: (state) => ({
        user: state.user,
        isAuthenticated: state.isAuthenticated,
      }),
    }
  )
);
```

## Step 5: Protected Route Component

Create `src/components/ProtectedRoute.tsx`:

```tsx
// src/components/ProtectedRoute.tsx
import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

interface ProtectedRouteProps {
  children: React.ReactNode;
  requiredRole?: string;
  fallback?: React.ReactNode;
}

export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({
  children,
  requiredRole,
  fallback
}) => {
  const { isAuthenticated, hasRole } = useAuth();
  const location = useLocation();

  if (!isAuthenticated) {
    // Redirect to login with return URL
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  if (requiredRole && !hasRole(requiredRole)) {
    return fallback || <Navigate to="/unauthorized" replace />;
  }

  return <>{children}</>;
};

// Utility component for role-based rendering
interface RoleGuardProps {
  requiredRole: string;
  children: React.ReactNode;
  fallback?: React.ReactNode;
}

export const RoleGuard: React.FC<RoleGuardProps> = ({
  requiredRole,
  children,
  fallback = null
}) => {
  const { hasRole } = useAuth();
  
  return hasRole(requiredRole) ? <>{children}</> : <>{fallback}</>;
};
```

## Step 6: Room Management Component

Create `src/components/RoomList.tsx`:

```tsx
// src/components/RoomList.tsx
import React from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api, Room, RateLimitError } from '../services/api';
import { useAuth } from '../hooks/useAuth';
import { RoleGuard } from './ProtectedRoute';

export const RoomList: React.FC = () => {
  const { hasRole } = useAuth();
  const queryClient = useQueryClient();

  // Fetch rooms with React Query (respects our rate limiting)
  const {
    data: rooms,
    isLoading,
    error,
    refetch
  } = useQuery({
    queryKey: ['rooms'],
    queryFn: api.getRooms,
    staleTime: 30000, // Cache for 30 seconds
    retry: (failureCount, error) => {
      // Don't retry rate limited requests
      if (error instanceof RateLimitError) {
        return false;
      }
      return failureCount < 3;
    },
  });

  // Delete room mutation
  const deleteRoomMutation = useMutation({
    mutationFn: api.deleteRoom,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['rooms'] });
    },
    onError: (error) => {
      console.error('Delete failed:', error);
      // Handle error (show toast, etc.)
    },
  });

  const handleDelete = (roomId: string) => {
    if (window.confirm('Are you sure you want to delete this room?')) {
      deleteRoomMutation.mutate(roomId);
    }
  };

  if (isLoading) {
    return (
      <div className="flex justify-center items-center p-8">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        <span className="ml-2">Loading rooms...</span>
      </div>
    );
  }

  if (error) {
    if (error instanceof RateLimitError) {
      return (
        <div className="bg-yellow-100 border border-yellow-400 text-yellow-700 px-4 py-3 rounded">
          <strong className="font-bold">Rate Limited!</strong>
          <span className="block sm:inline"> Please try again in a moment.</span>
          <button 
            onClick={() => refetch()}
            className="mt-2 bg-yellow-500 hover:bg-yellow-600 text-white px-4 py-2 rounded"
          >
            Retry
          </button>
        </div>
      );
    }

    return (
      <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
        <strong className="font-bold">Error:</strong>
        <span className="block sm:inline"> {error.message}</span>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <div className="flex justify-between items-center">
        <h2 className="text-2xl font-bold">Hotel Rooms</h2>
        
        {/* Only show "Add Room" button for Managers/Admins */}
        <RoleGuard requiredRole="Manager">
          <button className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded">
            Add New Room
          </button>
        </RoleGuard>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {rooms?.map((room) => (
          <div key={room.id} className="bg-white shadow-md rounded-lg p-6">
            <h3 className="text-xl font-semibold mb-2">{room.name}</h3>
            <div className="space-y-2 text-sm text-gray-600">
              <p>Capacity: {room.capacity} guests</p>
              <p>Price: ${room.pricePerNight}/night</p>
              <p>
                Status: 
                <span className={`ml-2 px-2 py-1 rounded text-xs ${
                  room.isAvailable 
                    ? 'bg-green-100 text-green-800' 
                    : 'bg-red-100 text-red-800'
                }`}>
                  {room.isAvailable ? 'Available' : 'Occupied'}
                </span>
              </p>
              {room.description && (
                <p className="text-gray-500">{room.description}</p>
              )}
            </div>

            {/* Admin/Manager actions */}
            <RoleGuard requiredRole="Manager">
              <div className="mt-4 flex space-x-2">
                <button className="bg-blue-500 hover:bg-blue-600 text-white px-3 py-1 rounded text-sm">
                  Edit
                </button>
                <button 
                  onClick={() => handleDelete(room.id)}
                  disabled={deleteRoomMutation.isPending}
                  className="bg-red-500 hover:bg-red-600 text-white px-3 py-1 rounded text-sm disabled:opacity-50"
                >
                  {deleteRoomMutation.isPending ? 'Deleting...' : 'Delete'}
                </button>
              </div>
            </RoleGuard>
          </div>
        ))}
      </div>

      {rooms?.length === 0 && (
        <div className="text-center text-gray-500 py-8">
          No rooms available. 
          <RoleGuard requiredRole="Manager">
            <span> Add some rooms to get started!</span>
          </RoleGuard>
        </div>
      )}
    </div>
  );
};
```

## Step 7: Login Page

Create `src/pages/LoginPage.tsx`:

```tsx
// src/pages/LoginPage.tsx
import React, { useState } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

export const LoginPage: React.FC = () => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const { login, isAuthenticated, isLoading, error } = useAuth();
  const location = useLocation();

  // Redirect if already authenticated
  if (isAuthenticated) {
    const from = location.state?.from?.pathname || '/';
    return <Navigate to={from} replace />;
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      await login(username, password);
      // Redirect will happen automatically due to state change
    } catch (error) {
      // Error is already handled in the auth store
      console.error('Login failed:', error);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50">
      <div className="max-w-md w-full space-y-8">
        <div>
          <h2 className="mt-6 text-center text-3xl font-extrabold text-gray-900">
            Sign in to Hotel Booking
          </h2>
        </div>
        
        <form className="mt-8 space-y-6" onSubmit={handleSubmit}>
          {error && (
            <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
              {error}
            </div>
          )}
          
          <div>
            <input
              type="text"
              required
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              placeholder="Username"
              className="appearance-none rounded-md relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
            />
          </div>
          
          <div>
            <input
              type="password"
              required
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Password"
              className="appearance-none rounded-md relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
            />
          </div>

          <button
            type="submit"
            disabled={isLoading}
            className="group relative w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50"
          >
            {isLoading ? 'Signing in...' : 'Sign in'}
          </button>
        </form>
      </div>
    </div>
  );
};
```

## Step 8: Main App Component

Update `src/App.tsx`:

```tsx
// src/App.tsx
import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';

import { LoginPage } from './pages/LoginPage';
import { RoomList } from './components/RoomList';
import { ProtectedRoute } from './components/ProtectedRoute';
import { useAuth } from './hooks/useAuth';

// Create React Query client with our backend rate limiting in mind
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30000, // 30 seconds
      retry: (failureCount, error: any) => {
        // Don't retry rate limited requests
        if (error?.status === 429) {
          return false;
        }
        return failureCount < 3;
      },
    },
  },
});

const Navigation: React.FC = () => {
  const { isAuthenticated, user, logout } = useAuth();

  if (!isAuthenticated) return null;

  return (
    <nav className="bg-blue-600 text-white p-4">
      <div className="container mx-auto flex justify-between items-center">
        <h1 className="text-xl font-bold">Hotel Booking System</h1>
        <div className="flex items-center space-x-4">
          <span>Welcome, {user?.username}!</span>
          <span className="text-sm opacity-75">({user?.roles.join(', ')})</span>
          <button 
            onClick={logout}
            className="bg-blue-700 hover:bg-blue-800 px-3 py-1 rounded"
          >
            Logout
          </button>
        </div>
      </div>
    </nav>
  );
};

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <Router>
        <div className="App">
          <Navigation />
          
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            
            <Route 
              path="/rooms" 
              element={
                <ProtectedRoute>
                  <div className="container mx-auto p-4">
                    <RoomList />
                  </div>
                </ProtectedRoute>
              } 
            />
            
            <Route 
              path="/admin" 
              element={
                <ProtectedRoute requiredRole="Admin">
                  <div className="container mx-auto p-4">
                    <h1>Admin Dashboard</h1>
                    <p>Only admins can see this page!</p>
                  </div>
                </ProtectedRoute>
              } 
            />
            
            <Route path="/" element={<Navigate to="/rooms" replace />} />
            
            <Route 
              path="/unauthorized" 
              element={
                <div className="container mx-auto p-4 text-center">
                  <h1 className="text-2xl font-bold text-red-600">Unauthorized</h1>
                  <p>You don't have permission to access this page.</p>
                </div>
              } 
            />
          </Routes>
        </div>
      </Router>
      
      {/* React Query DevTools (only in development) */}
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  );
}

export default App;
```

## Step 9: Environment Configuration

Create `.env` file:

```bash
# .env
REACT_APP_API_URL=http://localhost:5000/api
REACT_APP_ENV=development
```

Create `.env.production` file:

```bash
# .env.production
REACT_APP_API_URL=https://your-api-domain.com/api
REACT_APP_ENV=production
```

## Step 10: Run the Full Stack

```bash
# Terminal 1: Start the .NET API (our backend)
cd HotelBookingAPI
dotnet run --urls="http://localhost:5000"

# Terminal 2: Start React frontend
cd hotel-booking-frontend
npm start
```

## What You Get

🎉 **Working Full-Stack Application with:**

- ✅ **Secure Authentication**: JWT tokens, role-based access
- ✅ **Rate Limiting Integration**: React Query respects backend limits
- ✅ **Type Safety**: TypeScript throughout
- ✅ **Modern UI**: Responsive, component-based architecture
- ✅ **Error Handling**: Graceful degradation for API errors
- ✅ **Caching**: Intelligent data fetching and caching
- ✅ **Security Headers**: CORS, CSP, XSS protection
- ✅ **Professional Structure**: Scalable, maintainable code

## Next Steps

1. **Add more pages**: User profiles, booking history, admin dashboard
2. **Enhance UI**: Add a proper design system (Material-UI, Chakra UI)
3. **Add real-time features**: SignalR for live updates
4. **Implement testing**: Jest, React Testing Library
5. **Deploy**: Vercel/Netlify for frontend, Azure/AWS for backend

This gives you a professional, production-ready foundation for any web application! 🚀
