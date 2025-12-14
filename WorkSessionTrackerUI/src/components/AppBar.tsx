import React from 'react';
import IconButton from '@mui/material/IconButton';
import MenuIcon from '@mui/icons-material/Menu';

interface AppBarProps {
  title: string;
  onMenuClick: () => void;
}

const AppBar: React.FC<AppBarProps> = ({ title, onMenuClick }) => {
  return (
    <header className="bg-black text-white p-2 shadow-md flex justify-between items-center">
      <h1 className="text-xl font-bold">{title}</h1>
      <IconButton
        edge="start"
        color="inherit"
        aria-label="menu"
        onClick={onMenuClick}
      >
        <MenuIcon />
      </IconButton>
    </header>
  );
};

export default AppBar;
