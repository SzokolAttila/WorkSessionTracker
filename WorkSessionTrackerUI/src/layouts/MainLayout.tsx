import { Outlet } from 'react-router-dom';
import AppBar from '../components/AppBar';

const MainLayout = () => {

  return (
    <div className="min-h-screen flex flex-col">
      <AppBar title="Work Session Tracker" />
      <main className="flex-grow container mx-auto p-4">
        <Outlet />
      </main>
    </div>
  );
};

export default MainLayout;
