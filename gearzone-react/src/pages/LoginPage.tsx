import { useState, FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { authApi } from '../api/auth';

export default function LoginPage() {
  const { login, refresh } = useAuth();
  const navigate = useNavigate();

  const [tab, setTab] = useState<'login' | 'register'>('login');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(false);

  // Login form
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [rememberMe, setRememberMe] = useState(false);

  // Register form
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [regPassword, setRegPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');

  const handleLogin = async (e: FormEvent) => {
    e.preventDefault();
    setError(''); setLoading(true);
    try {
      await login(username, password, rememberMe);
      await refresh();
      navigate('/');
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Login failed.');
    } finally { setLoading(false); }
  };

  const handleRegister = async (e: FormEvent) => {
    e.preventDefault();
    setError(''); setLoading(true);
    try {
      await authApi.register(fullName, email, regPassword, confirmPassword);
      setSuccess('Registration successful! Please check your email to verify your account.');
      setTab('login');
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Registration failed.');
    } finally { setLoading(false); }
  };

  return (
    <div style={{ maxWidth: 400, margin: '4rem auto', padding: '2rem', border: '1px solid #ccc', borderRadius: 8 }}>
      <h2>Welcome to GearZone</h2>

      <div style={{ display: 'flex', gap: '1rem', marginBottom: '1.5rem' }}>
        <button onClick={() => setTab('login')} style={{ fontWeight: tab === 'login' ? 'bold' : 'normal' }}>Login</button>
        <button onClick={() => setTab('register')} style={{ fontWeight: tab === 'register' ? 'bold' : 'normal' }}>Register</button>
      </div>

      {error && <p style={{ color: 'red' }}>{error}</p>}
      {success && <p style={{ color: 'green' }}>{success}</p>}

      {tab === 'login' ? (
        <form onSubmit={handleLogin} style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          <input placeholder="Email or Username" value={username} onChange={e => setUsername(e.target.value)} required />
          <input type="password" placeholder="Password" value={password} onChange={e => setPassword(e.target.value)} required />
          <label><input type="checkbox" checked={rememberMe} onChange={e => setRememberMe(e.target.checked)} /> Remember me</label>
          <button type="submit" disabled={loading}>{loading ? 'Signing in…' : 'Sign in'}</button>
          <button type="button" onClick={() => authApi.startGoogleLogin()}>Continue with Google</button>
        </form>
      ) : (
        <form onSubmit={handleRegister} style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          <input placeholder="Full Name" value={fullName} onChange={e => setFullName(e.target.value)} required />
          <input type="email" placeholder="Email" value={email} onChange={e => setEmail(e.target.value)} required />
          <input type="password" placeholder="Password (min 6)" value={regPassword} onChange={e => setRegPassword(e.target.value)} required />
          <input type="password" placeholder="Confirm Password" value={confirmPassword} onChange={e => setConfirmPassword(e.target.value)} required />
          <button type="submit" disabled={loading}>{loading ? 'Creating account…' : 'Create account'}</button>
        </form>
      )}
    </div>
  );
}
