# Single-Page App Authentication with Overlay and Menu

## Overview

This implementation provides a hybrid approach to user account management combining dropdown menus and modal overlays:

- Dropdown menu for quick actions (logout, profile navigation)
- Modal overlay for detailed profile view
- Maintains a single-page app (SPA) experience

## Core Components

### 1. Overlay Context System

The `OverlayContext` provides a centralized system for managing all application overlays:

- Handles which overlay is currently shown
- Passes data between components and overlays
- Manages overlay state (open/closed)

### 2. Authentication Components

#### LoginButton
- Changes appearance based on authentication state
- Shows login button for unauthenticated users
- Shows user avatar with dropdown menu for authenticated users
- Provides access to account overlay and logout functionality

#### AccountOverlay
- Shows detailed user profile information
- Presents a visually appealing profile card
- Minimal design focused on core information

### 3. Authentication Check Hook

The `useAuthCheck` hook provides a simple way to:

- Check if a user is authenticated
- Display the login overlay when needed
- Handle callbacks after successful authentication

## Implementation Details

### Authentication Flow

1. User clicks login button or tries to perform an authenticated action
2. If not authenticated, the login overlay appears
3. After successful login, the avatar appears with dropdown menu access

### User Account Flow

1. User clicks on their avatar in the navigation bar
2. Dropdown menu appears with options:
   - View Profile (opens the account overlay)
   - Logout (handles user logout)
3. The account overlay shows expanded profile information

### Backend Requirements

The backend authentication system should:

1. **Secure API endpoints** for authenticated actions
   - POST `/api/quizzes` (quiz creation)
   - GET `/api/submissions` (viewing submissions)

2. **Maintain user associations**
   - Associate quizzes with the creating user
   - Filter submissions by the current user

## Code Examples

### Checking Authentication Before an Action

```typescript
const { checkAuth } = useAuthCheck({
  message: "Custom login message",
  onAuthSuccess: () => {
    // Execute after successful login
    handleContinueAction();
  }
});

const handleProtectedAction = () => {
  const isAuthenticated = checkAuth();
  if (isAuthenticated) {
    // Continue with protected action
    handleContinueAction();
  }
  // If not authenticated, login overlay appears automatically
};
```

### Showing the Account Overlay from Menu

```typescript
import { useOverlay } from '../context/OverlayContext';

const ProfileMenuItem = () => {
  const { showOverlay } = useOverlay();
  
  const handleMenuItemClick = () => {
    // Close menu if needed
    handleMenuClose();
    // Show account overlay
    showOverlay('account');
  };
  
  return (
    <MenuItem onClick={handleMenuItemClick}>
      <PersonIcon fontSize="small" sx={{ mr: 1 }} />
      View Profile
    </MenuItem>
  );
};
```

### Conditionally Rendering Content

```typescript
// In a component that needs authentication
const { isLoggedIn } = useAuth();

// Show login prompt if not authenticated
if (!isLoggedIn) {
  return (
    <Card>
      <Typography>
        Login required to view this content
      </Typography>
      <Button onClick={checkAuth}>
        Login Now
      </Button>
    </Card>
  );
}

// Show authenticated content
return <AuthenticatedContent />;
```

## Testing the Implementation

1. **Authentication Flow**
   - Try creating a quiz when not logged in
   - Verify the login overlay appears
   - Test that after login, the original action continues

2. **Avatar and Menu**
   - Click on the user avatar when logged in
   - Verify the dropdown menu appears
   - Test each menu option (view profile, logout)

3. **Account Overlay**
   - Click "View Profile" in the dropdown menu
   - Verify the account overlay appears with correct user information
   - Test closing the overlay

4. **Component States**
   - Verify authenticated and unauthenticated states show appropriate UI
   - Check that user-specific data only loads when authenticated 