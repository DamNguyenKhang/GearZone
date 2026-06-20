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
    try { await (lock ? adminApi.users.lock(id) : adminApi.users.unlock(id)); fetchUsers(search); }
    finally { setProcessing(null); }
  };

  const handleRoleChange = async (id: string, role: string) => {
    setProcessing(id);
    try { await adminApi.users.changeRole(id, role); fetchUsers(search); }
    finally { setProcessing(null); }
  };

  return (
    <div className="container">
      <h2 className="mb-4">Users</h2>

      <div className="input-group mb-4" style={{ maxWidth: 400 }}>
        <input className="form-control" placeholder="Search users…" value={search}
          onChange={e => setSearch(e.target.value)}
          onKeyDown={e => e.key === 'Enter' && fetchUsers(search)} />
        <button className="btn btn-outline-secondary" onClick={() => fetchUsers(search)}>Search</button>
      </div>

      {loading ? (
        <div className="text-center py-4"><div className="spinner-border text-primary" /></div>
      ) : (
        <div className="card shadow-sm">
          <div className="table-responsive">
            <table className="table table-hover mb-0">
              <thead className="table-light">
                <tr>
                  <th>Name</th><th>Email</th><th>Role</th>
                  <th className="text-center">Verified</th>
                  <th className="text-center">Status</th>
                  <th className="text-center">Actions</th>
                </tr>
              </thead>
              <tbody>
                {users.map(u => (
                  <tr key={u.id}>
                    <td className="fw-semibold">{u.fullName}</td>
                    <td className="text-muted small">{u.email}</td>
                    <td>
                      <select className="form-select form-select-sm" value={u.role}
                        onChange={e => handleRoleChange(u.id, e.target.value)}
                        disabled={processing === u.id}>
                        <option>Customer</option>
                        <option>Store Owner</option>
                        <option>Super Admin</option>
                      </select>
                    </td>
                    <td className="text-center">
                      <span className={`badge bg-${u.isEmailVerified ? 'success' : 'danger'}`}>
                        {u.isEmailVerified ? '✓' : '✗'}
                      </span>
                    </td>
                    <td className="text-center">
                      <span className={`badge bg-${u.isLocked ? 'danger' : 'success'}`}>
                        {u.isLocked ? 'Locked' : 'Active'}
                      </span>
                    </td>
                    <td className="text-center">
                      <button
                        className={`btn btn-sm ${u.isLocked ? 'btn-outline-success' : 'btn-outline-danger'}`}
                        onClick={() => handleLock(u.id, !u.isLocked)}
                        disabled={processing === u.id}>
                        {processing === u.id ? <span className="spinner-border spinner-border-sm" /> : (u.isLocked ? 'Unlock' : 'Lock')}
                      </button>
                    </td>
                  </tr>
                ))}
                {users.length === 0 && (
                  <tr><td colSpan={6} className="text-center text-muted py-4">No users found.</td></tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}
