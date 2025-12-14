import React from 'react';
import CloseIcon from '@mui/icons-material/Close';
import IconButton from '@mui/material/IconButton';

interface SidebarProps {
  isOpen: boolean;
  onClose: () => void;
}

const Sidebar: React.FC<SidebarProps> = ({ isOpen, onClose }) => {
  return (
    <>
      {/* Backdrop */}
      <div
        onClick={onClose}
        className={`fixed inset-0 bg-black bg-opacity-50 z-40 transition-opacity duration-300 ${
          isOpen ? 'opacity-40' : 'opacity-0 pointer-events-none'
        }`}
      />

      {/* Sidebar Panel */}
      <aside
        className={`fixed top-0 right-0 h-full w-72 bg-black text-white shadow-lg z-50 transform transition-transform duration-300 ease-in-out ${
          isOpen ? 'translate-x-0' : 'translate-x-full'
        }`}
      >
        <div className="flex flex-col h-full p-4 text-left">
          <div className="flex justify-between items-center mb-8">
            <h2 className="text-2xl font-bold">Menu</h2>
            <IconButton onClick={onClose} color="inherit" aria-label="Close menu">
              <CloseIcon />
            </IconButton>
          </div>
          <nav className="flex-grow">
            <button className="w-full p-2 border border-white hover:bg-white hover:text-black active:bg-white active:text-black text-left rounded-lg transition-colors">Connect to Company</button>
          </nav>
          <button className="w-full p-2 border border-white hover:bg-white hover:text-black active:bg-white active:text-black text-left rounded-lg transition-colors">Logout</button>
        </div>
      </aside>
    </>
  );
};

export default Sidebar;
