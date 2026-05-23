import { useEffect, useState } from 'react';
import { adminApi } from '../../api/admin';

interface User {
  id: string;
  fullName: string;
  email: string;
  role: string;
  isEmailVerified: boolean;
  createdAt: string;
  isLocked?: boolean;
}

export default function AdminUsersPage() {
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [processing, setProcessing] = useState<string | null>(null);

  const fetchUsers = (q = '') => {
    setLoading(true);
    adminApi.users.list({ search: q })
      .then(d => setUsers((d as { items?: User[] }).items ?? (d as User[]) ?? []))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchUsers(); }, []);

  const handleLock = async (id: string, lock: boolean) => {
    setProcessing(id);
    try {
      await (lock ? adminApi.users.lock(id) : adminApi.users.unlock(id));
      fetchUsers(search);
    } finally { setProcessing(null); }
  };

  const handleRoleChange = async (id: string, role: string) => {
    setProcessing(id);
    try {
      await adminApi.users.changeRole(id, role);
      fetchUsers(search);
    } finally { setProcessing(null); }
  };

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;

  return (
    <div style={{ padding: '2rem' }}>
      <h1>Users</h1>
      <input placeholder="Search users…" value={search} onChange={e => setSearch(e.target.value)}
        onKeyDown={e => e.key === 'Enter' && fetchUsers(search)}
        style={{ padding: '0.5rem', width: '100%', maxWidth: 400, marginBottom: '1rem' }} />

      <table style={{ width: '100%', borderCollapse: 'collapse' }}>
        <thead>
          <tr style={{ background: '#f0f0f0' }}>
            <th style={{ padding: '0.75rem', textAlign: 'left' }}>Name</th>
            <th style={{ padding: '0.75rem', textAlign: 'left' }}>Email</th>
            <th style={{ padding: '0.75rem', textAlign: 'left' }}>Role</th>
            <th style={{ padding: '0.75rem', textAlign: 'center' }}>Verified</th>
            <th style={{ padding: '0.75rem', textAlign: 'center' }}>Status</th>
            <th style={{ padding: '0.75rem', textAlign: 'center' }}>Actions</th>
          </tr>
        </thead>
        <tbody>
          {users.map(u => (
            <tr key={u.id} style={{ borderBottom: '1px solid #eee' }}>
              <td style={{ padding: '0.75rem', fontWeight: 500 }}>{u.fullName}</td>
              <td style={{ padding: '0.75rem', color: '#555', fontSize: 14 }}>{u.email}</td>
              <td style={{ padding: '0.75rem' }}>
                <select value={u.role} onChange={e => handleRoleChange(u.id, e.target.value)} disabled={processing === u.id}
                  style={{ padding: '0.25rem 0.5rem', fontSize: 13 }}>
                  <option>Customer</option>
                  <option>Store Owner</option>
                  <option>Super Admin</option>
                </select>
              </td>
              <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                <span style={{ color: u.isEmailVerified ? '#38a169' : '#e53e3e' }}>{u.isEmailVerified ? '✓' : '✗'}</span>
              </td>
              <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                <span style={{ color: u.isLocked ? '#e53e3e' : '#38a169', fontWeight: 600 }}>{u.isLocked ? 'Locked' : 'Active'}</span>
              </td>
              <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                <button onClick={() => handleLock(u.id, !u.isLocked)} disabled={processing === u.id}
                  style={{ padding: '0.25rem 0.75rem', background: u.isLocked ? '#c6f6d5' : '#fed7d7', border: 'none', borderRadius: 4, cursor: 'pointer', color: u.isLocked ? '#276749' : '#c53030' }}>
                  {processing === u.id ? '…' : u.isLocked ? 'Unlock' : 'Lock'}
                </button>
              </td>
            </tr>
          ))}
          {users.length === 0 && (
            <tr><td colSpan={6} style={{ padding: '2rem', textAlign: 'center', color: '#888' }}>No users found.</td></tr>
          )}
        </tbody>
      </table>
    </div>
  );
}
