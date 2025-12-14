import React, { useState } from 'react';
import { Outlet } from 'react-router-dom';
import AppBar from '../components/AppBar';
import Sidebar from '../components/Sidebar';

const MainLayout = () => {
  const [isSidebarOpen, setSidebarOpen] = useState(false);

  const toggleSidebar = () => {
    setSidebarOpen(!isSidebarOpen);
  };

  return (
    <div className="min-h-screen flex flex-col">
      <AppBar title="Work Session Tracker" onMenuClick={toggleSidebar} />
      <Sidebar isOpen={isSidebarOpen} onClose={() => setSidebarOpen(false)} />
      <main className="flex-grow container mx-auto p-4">
        <Outlet />
      </main>
    </div>
  );
};

export default MainLayout;
