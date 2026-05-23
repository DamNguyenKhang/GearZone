import { useEffect, useState } from 'react';
import { adminApi } from '../../api/admin';

interface Brand {
  id: string;
  name: string;
  slug: string;
  logoUrl?: string;
  productCount?: number;
}

export default function AdminBrandsPage() {
  const [brands, setBrands] = useState<Brand[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editId, setEditId] = useState<string | null>(null);
  const [form, setForm] = useState({ name: '', slug: '', logoUrl: '' });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const fetchBrands = () => {
    adminApi.brands.list()
      .then(d => setBrands((d as { items?: Brand[] }).items ?? (d as Brand[]) ?? []))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchBrands(); }, []);

  const resetForm = () => { setShowForm(false); setEditId(null); setForm({ name: '', slug: '', logoUrl: '' }); };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true); setError('');
    try {
      if (editId) await adminApi.brands.update(editId, form);
      else await adminApi.brands.create(form);
      resetForm(); fetchBrands();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to save brand.');
    } finally { setSaving(false); }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this brand?')) return;
    await adminApi.brands.delete(id);
    fetchBrands();
  };

  if (loading) return <div className="container text-center py-5"><div className="spinner-border text-primary" /></div>;

  return (
    <div className="container">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h2 className="mb-0">Brands</h2>
        <button className="btn btn-primary" onClick={() => { resetForm(); setShowForm(true); }}>+ Add Brand</button>
      </div>

      {showForm && (
        <div className="card shadow-sm mb-4">
          <div className="card-header"><h5 className="mb-0">{editId ? 'Edit Brand' : 'New Brand'}</h5></div>
          <div className="card-body">
            <form onSubmit={handleSubmit}>
              <div className="row g-3">
                <div className="col-md-4">
                  <label className="form-label">Name</label>
                  <input className="form-control" value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} required />
                </div>
                <div className="col-md-4">
                  <label className="form-label">Slug</label>
                  <input className="form-control" placeholder="e.g. asus" value={form.slug} onChange={e => setForm(f => ({ ...f, slug: e.target.value }))} required />
                </div>
                <div className="col-md-4">
                  <label className="form-label">Logo URL (optional)</label>
                  <input className="form-control" value={form.logoUrl} onChange={e => setForm(f => ({ ...f, logoUrl: e.target.value }))} />
                </div>
              </div>
              {error && <div className="alert alert-danger mt-3 py-2">{error}</div>}
              <div className="d-flex gap-2 mt-3">
                <button type="submit" className="btn btn-success" disabled={saving}>
                  {saving ? <span className="spinner-border spinner-border-sm me-1" /> : null}Save
                </button>
                <button type="button" className="btn btn-outline-secondary" onClick={resetForm}>Cancel</button>
              </div>
            </form>
          </div>
        </div>
      )}

      <div className="row row-cols-2 row-cols-md-4 row-cols-lg-5 g-3">
        {brands.map(b => (
          <div key={b.id} className="col">
            <div className="card h-100 shadow-sm text-center">
              <div className="card-body">
                {b.logoUrl && <img src={b.logoUrl} alt={b.name} style={{ width: 56, height: 56, objectFit: 'contain' }} className="mb-2" />}
                <p className="fw-semibold mb-0">{b.name}</p>
                <small className="text-muted d-block">{b.slug}</small>
                {b.productCount !== undefined && <small className="text-muted">{b.productCount} products</small>}
              </div>
              <div className="card-footer d-flex gap-1 justify-content-center">
                <button className="btn btn-sm btn-outline-primary" onClick={() => { setEditId(b.id); setForm({ name: b.name, slug: b.slug, logoUrl: b.logoUrl ?? '' }); setShowForm(true); }}>Edit</button>
                <button className="btn btn-sm btn-outline-danger" onClick={() => handleDelete(b.id)}>Delete</button>
              </div>
            </div>
          </div>
        ))}
        {brands.length === 0 && <p className="text-muted">No brands yet.</p>}
      </div>
    </div>
  );
}
