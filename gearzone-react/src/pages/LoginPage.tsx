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

  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [rememberMe, setRememberMe] = useState(false);

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
    <div className="container">
      <div className="row justify-content-center">
        <div className="col-md-5">
          <div className="card shadow-sm mt-2">
            <div className="card-body p-4">
              <h4 className="card-title mb-4 text-center">Welcome to GearZone</h4>

              <ul className="nav nav-tabs mb-4">
                <li className="nav-item">
                  <button className={`nav-link ${tab === 'login' ? 'active' : ''}`} onClick={() => setTab('login')}>Login</button>
                </li>
                <li className="nav-item">
                  <button className={`nav-link ${tab === 'register' ? 'active' : ''}`} onClick={() => setTab('register')}>Register</button>
                </li>
              </ul>

              {error && <div className="alert alert-danger py-2">{error}</div>}
              {success && <div className="alert alert-success py-2">{success}</div>}

              {tab === 'login' ? (
                <form onSubmit={handleLogin}>
                  <div className="mb-3">
                    <input className="form-control" placeholder="Email or Username" value={username}
                      onChange={e => setUsername(e.target.value)} required />
                  </div>
                  <div className="mb-3">
                    <input className="form-control" type="password" placeholder="Password" value={password}
                      onChange={e => setPassword(e.target.value)} required />
                  </div>
                  <div className="mb-3 form-check">
                    <input className="form-check-input" type="checkbox" id="rememberMe" checked={rememberMe}
                      onChange={e => setRememberMe(e.target.checked)} />
                    <label className="form-check-label" htmlFor="rememberMe">Remember me</label>
                  </div>
                  <button className="btn btn-primary w-100 mb-2" type="submit" disabled={loading}>
                    {loading && <span className="spinner-border spinner-border-sm me-2" />}
                    Sign in
                  </button>
                  <button className="btn btn-outline-secondary w-100" type="button" onClick={() => authApi.startGoogleLogin()}>
                    Continue with Google
                  </button>
                </form>
              ) : (
                <form onSubmit={handleRegister}>
                  <div className="mb-3">
                    <input className="form-control" placeholder="Full Name" value={fullName}
                      onChange={e => setFullName(e.target.value)} required />
                  </div>
                  <div className="mb-3">
                    <input className="form-control" type="email" placeholder="Email" value={email}
                      onChange={e => setEmail(e.target.value)} required />
                  </div>
                  <div className="mb-3">
                    <input className="form-control" type="password" placeholder="Password (min 6)" value={regPassword}
                      onChange={e => setRegPassword(e.target.value)} required />
                  </div>
                  <div className="mb-3">
                    <input className="form-control" type="password" placeholder="Confirm Password" value={confirmPassword}
                      onChange={e => setConfirmPassword(e.target.value)} required />
                  </div>
                  <button className="btn btn-primary w-100" type="submit" disabled={loading}>
                    {loading && <span className="spinner-border spinner-border-sm me-2" />}
                    Create account
                  </button>
                </form>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
