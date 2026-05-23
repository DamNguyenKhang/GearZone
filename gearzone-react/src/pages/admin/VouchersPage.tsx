import { useEffect, useState } from 'react';
import { adminApi } from '../../api/admin';

interface Voucher {
  id: string;
  code: string;
  discountType: string;
  discountValue: number;
  usageCount: number;
  usageLimit?: number;
  expiresAt?: string;
  isActive: boolean;
  scope: 'Platform' | 'Store';
}

export default function AdminVouchersPage() {
  const [vouchers, setVouchers] = useState<Voucher[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({ code: '', discountType: 'Percentage', discountValue: '', usageLimit: '', expiresAt: '' });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const fetchVouchers = () => {
    adminApi.vouchers.list()
      .then(d => setVouchers((d as { items?: Voucher[] }).items ?? (d as Voucher[]) ?? []))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchVouchers(); }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true); setError('');
    try {
      await adminApi.vouchers.create({
        code: form.code, discountType: form.discountType,
        discountValue: Number(form.discountValue),
        usageLimit: form.usageLimit ? Number(form.usageLimit) : undefined,
        expiresAt: form.expiresAt || undefined,
      });
      setShowForm(false);
      setForm({ code: '', discountType: 'Percentage', discountValue: '', usageLimit: '', expiresAt: '' });
      fetchVouchers();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to create voucher.');
    } finally { setSaving(false); }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this voucher?')) return;
    await adminApi.vouchers.delete(id);
    fetchVouchers();
  };

  if (loading) return <div className="container text-center py-5"><div className="spinner-border text-primary" /></div>;

  return (
    <div className="container">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h2 className="mb-0">Platform Vouchers</h2>
        <button className="btn btn-primary" onClick={() => setShowForm(true)}>+ Create Voucher</button>
      </div>

      {showForm && (
        <div className="card shadow-sm mb-4">
          <div className="card-header"><h5 className="mb-0">New Platform Voucher</h5></div>
          <div className="card-body">
            <form onSubmit={handleSubmit}>
              <div className="row g-3">
                <div className="col-md-4">
                  <label className="form-label">Code</label>
                  <input className="form-control font-monospace" value={form.code}
                    onChange={e => setForm(f => ({ ...f, code: e.target.value }))} required />
                </div>
                <div className="col-md-4">
                  <label className="form-label">Discount Type</label>
                  <select className="form-select" value={form.discountType} onChange={e => setForm(f => ({ ...f, discountType: e.target.value }))}>
                    <option value="Percentage">Percentage (%)</option>
                    <option value="Fixed">Fixed Amount (₫)</option>
                  </select>
                </div>
                <div className="col-md-4">
                  <label className="form-label">Discount Value</label>
                  <input className="form-control" type="number" value={form.discountValue}
                    onChange={e => setForm(f => ({ ...f, discountValue: e.target.value }))} required />
                </div>
                <div className="col-md-6">
                  <label className="form-label">Usage Limit (optional)</label>
                  <input className="form-control" type="number" value={form.usageLimit}
                    onChange={e => setForm(f => ({ ...f, usageLimit: e.target.value }))} />
                </div>
                <div className="col-md-6">
                  <label className="form-label">Expires At</label>
                  <input className="form-control" type="date" value={form.expiresAt}
                    onChange={e => setForm(f => ({ ...f, expiresAt: e.target.value }))} />
                </div>
              </div>
              {error && <div className="alert alert-danger mt-3 py-2">{error}</div>}
              <div className="d-flex gap-2 mt-3">
                <button type="submit" className="btn btn-success" disabled={saving}>
                  {saving ? <span className="spinner-border spinner-border-sm me-1" /> : null}Create
                </button>
                <button type="button" className="btn btn-outline-secondary" onClick={() => setShowForm(false)}>Cancel</button>
              </div>
            </form>
          </div>
        </div>
      )}

      <div className="card shadow-sm">
        <div className="table-responsive">
          <table className="table table-hover mb-0">
            <thead className="table-light">
              <tr><th>Code</th><th>Discount</th><th className="text-end">Used</th><th>Expires</th><th className="text-center">Status</th><th></th></tr>
            </thead>
            <tbody>
              {vouchers.map(v => (
                <tr key={v.id}>
                  <td className="font-monospace fw-semibold">{v.code}</td>
                  <td>{v.discountType === 'Percentage' ? `${v.discountValue}%` : `${v.discountValue?.toLocaleString()} ₫`}</td>
                  <td className="text-end">{v.usageCount}/{v.usageLimit ?? '∞'}</td>
                  <td className="text-muted small">{v.expiresAt ? new Date(v.expiresAt).toLocaleDateString() : '—'}</td>
                  <td className="text-center"><span className={`badge bg-${v.isActive ? 'success' : 'danger'}`}>{v.isActive ? 'Active' : 'Inactive'}</span></td>
                  <td className="text-center"><button className="btn btn-sm btn-outline-danger" onClick={() => handleDelete(v.id)}>Delete</button></td>
                </tr>
              ))}
              {vouchers.length === 0 && <tr><td colSpan={6} className="text-center text-muted py-4">No vouchers yet.</td></tr>}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
