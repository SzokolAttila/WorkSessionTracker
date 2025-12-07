import React from 'react';
import { AppBar as MuiAppBar, Toolbar, Typography, IconButton } from '@mui/material';
import MenuIcon from '@mui/icons-material/Menu';

/**
 * Props for the AppBar component.
 */
interface CustomAppBarProps {
  /** The title to display in the AppBar. */
  title: string;
  /** Optional callback function when the hamburger menu icon is clicked. */
  onMenuClick?: () => void;
}

/**
 * Reusable AppBar component for the top of the application.
 * Displays a title and a hamburger menu icon on the right.
 */
const AppBar: React.FC<CustomAppBarProps> = ({ title, onMenuClick }) => {
  return (
    <MuiAppBar position="fixed" sx={{ backgroundColor: 'black' }}>
      <Toolbar>
        <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
          {title}
        </Typography>
        <IconButton size="large" edge="end" color="inherit" aria-label="menu" onClick={onMenuClick}>
          <MenuIcon />
        </IconButton>
      </Toolbar>
    </MuiAppBar>
  );
};

export default AppBar;
