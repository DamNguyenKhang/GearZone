import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { useState, useEffect } from 'react';
import { cartApi } from '../../api/cart';

export default function Navbar() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [cartCount, setCartCount] = useState(0);

  useEffect(() => {
    if (user) {
      cartApi.get().then((cart) => {
        const c = cart as { items?: unknown[] };
        setCartCount(c?.items?.length ?? 0);
      }).catch(() => {});
    }
  }, [user]);

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  return (
    <nav style={{ padding: '1rem', borderBottom: '1px solid #ccc', display: 'flex', gap: '1rem', alignItems: 'center' }}>
      <Link to="/"><strong>GearZone</strong></Link>
      <Link to="/products">Products</Link>

      {user ? (
        <>
          <Link to="/cart">Cart ({cartCount})</Link>
          <Link to="/profile">Profile</Link>

          {user.role === 'Store Owner' && <Link to="/seller/dashboard">Seller</Link>}
          {user.role === 'Super Admin' && <Link to="/admin/dashboard">Admin</Link>}

          <button onClick={handleLogout}>Logout ({user.fullName})</button>
        </>
      ) : (
        <Link to="/login">Login</Link>
      )}
    </nav>
  );
}
