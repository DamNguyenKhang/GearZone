import { Outlet } from 'react-router-dom';
import Navbar from './Navbar';

export default function Layout() {
  return (
    <div className="d-flex flex-column min-vh-100">
      <Navbar />
      <main className="flex-grow-1 py-4">
        <Outlet />
      </main>
      <footer className="bg-dark text-light text-center py-3 small">
        &copy; {new Date().getFullYear()} GearZone. All rights reserved.
      </footer>
    </div>
  );
}
