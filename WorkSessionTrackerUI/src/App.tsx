import { Routes, Route } from 'react-router-dom';
import MainLayout from './layouts/MainLayout';
import HomePage from './pages/HomePage';
import LoginPage from './pages/LoginPage';

function App() {
  return (
    <Routes>
      {/* Routes with the main layout (includes AppBar and Sidebar) */}
      <Route path="/" element={<MainLayout />}>
        <Route index element={<HomePage />} />
      </Route>
      {/* Routes without the main layout */}
      <Route path="/login" element={<LoginPage />} />
    </Routes>
  );
}

export default App
